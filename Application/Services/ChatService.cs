using Application.Extensions;
using Application.Filters;
using Application.Models;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ChatService
    {
        private readonly WebChatContext _context;
        private readonly IMapper _mapper;

        public ChatService(WebChatContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<WebChatUserViewModel>> GetOnlineUsers(Guid ignoredUserId)
        {
            var now = DateTimeOffset.UtcNow;
            return await _context.AuthenticationTickets
                .Include(t => t.User)
                .Where(t => t.UserId != ignoredUserId && (!t.Expires.HasValue || t.Expires.Value > now)).Select(t => t.User)
                .ProjectTo<WebChatUserViewModel>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<List<WebChatViewModel>> GetWebChats(Guid userId)
        {
            var now = DateTimeOffset.UtcNow;

            var chats = await _context.Chats
                .Include(c => c.Users)
                .ThenInclude(u => u.User)
                .Where(c => c.Users.Any(u => u.UserId == userId))
                .ProjectTo<WebChatViewModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            var activeTicketIds = new HashSet<Guid>(await _context.AuthenticationTickets.Where(t => !t.Expires.HasValue || t.Expires.Value > now).Select(t => t.UserId).ToListAsync());

            var chatIds = chats.Select(c => c.Id).ToList();
            var unreadMessageFromChatIds = await _context
                .Messages
                .Where(m => chatIds.Contains(m.ChatId) && m.SenderId != userId && m.Recepients.All(r => r.RecepientId != userId || !r.ReadTime.HasValue))
                .Select(m => m.ChatId)
                .ToListAsync();

            var unreadMessagesByChatCount = unreadMessageFromChatIds.GroupBy(chatId => chatId).ToDictionary(x => x.Key, x => x.Count());

            foreach (var chat in chats)
            {
                chat.IsOnline = chat.Users.Any(u => u.Id != userId && activeTicketIds.Contains(u.Id));

                var interlocutor = chat.Users.Where(u => u.Id != userId).SingleOrDefault();
                if (interlocutor != null)
                    chat.Name = interlocutor.UserName;

                if (unreadMessagesByChatCount.TryGetValue(chat.Id, out var count))
                    chat.UnreadMessagesCount = count;
            }

            return chats;
        }

        public async Task<PagedList<MessageViewModel>> GetMessages(int chatId, Guid userId, int? pageNumber, int? pageSize)
        {
            var messages = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Recepients)
                .Where(m => m.ChatId == chatId)
                .OrderByDescending(m => m.Id)
                .ToPagedListAsync(pageNumber, pageSize);

            var result = new PagedList<MessageViewModel>
            {
                Items = messages.Items.Select(m => _mapper.Map<MessageViewModel>(m)).ToList(),
                PageNumber = messages.PageNumber,
                PageSize = messages.PageSize,
                TotalCount = messages.TotalCount
            };

            var messageDictionary = messages.Items.ToDictionary(m => m.Id);
            foreach (var message in result.Items)
                message.IsRead = message.SenderId == userId
                    ? messageDictionary[message.Id].Recepients.Any()
                    : messageDictionary[message.Id].Recepients.Any(r => r.RecepientId == userId && r.ReadTime.HasValue);

            return result;
        }

        public async Task<WebChatViewModel> CreateChat(Guid firstUserId, Guid secondUserId)
        {
            var chat = await _context.Chats
                .Include(c => c.Users)
                .ThenInclude(u => u.User)
                .FirstOrDefaultAsync(c => c.Users.Any(u => u.UserId == firstUserId) && c.Users.Any(u => u.UserId == secondUserId));

            if (chat != null)
                return _mapper.Map<WebChatViewModel>(chat);

            chat = new WebChat
            {
                Name = $"{firstUserId:D}|{secondUserId:D}",
                Users = new List<WebChatToUser>
                {
                    new WebChatToUser { UserId = firstUserId },
                    new WebChatToUser { UserId = secondUserId }
                }
            };

            _context.Chats.Add(chat);

            await _context.SaveChangesAsync();
            var result = _mapper.Map<WebChatViewModel>(chat);
            result.Name = await _context.Users.Where(u => u.Id == secondUserId).Select(u => u.UserName).FirstOrDefaultAsync();

            return result;
        }

        public async Task<MessageViewModel> SendMessage(SendMessageModel messageModel)
        {
            if (string.IsNullOrEmpty(messageModel.Text))
                return null;

            var message = _mapper.Map<WebChatMessage>(messageModel);
            _context.Messages.Add(message);

            await _context.SaveChangesAsync();

            var result = _mapper.Map<MessageViewModel>(message);
            result.SenderName = messageModel.SenderName;

            return result;
        }

        public async Task<List<Guid>> GetChatUsers(int chatId)
        {
            return await _context.ChatsToUsers.Where(ctu => ctu.ChatId == chatId).Select(ctu => ctu.UserId).ToListAsync();
        }

        public async Task ReadMessage(Guid userId, int messageId)
        {
            if(!await _context.Messages.AnyAsync(m => m.Id == messageId && m.SenderId != userId))
                return;

            var now = DateTimeOffset.Now;
            var messageStatus = await _context.MessageRecepients.FirstOrDefaultAsync(mr => mr.MessageId == messageId && mr.RecepientId == userId);
            if(messageStatus == null)
            {
                messageStatus = new WebMessageToRecepient
                {
                    MessageId = messageId,
                    RecepientId = userId,
                    ReceivedTime = now
                };
                _context.MessageRecepients.Add(messageStatus);
            }

            if (messageStatus.ReadTime.HasValue)
                return;

            messageStatus.ReadTime = now;
            await _context.SaveChangesAsync();
        }
    }
}
