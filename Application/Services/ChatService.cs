using Application.Contracts;
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
    public class ChatService : IScopedService
    {
        private readonly WebChatContext _context;
        private readonly IMapper _mapper;

        public ChatService(WebChatContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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

        public async Task<List<Guid>> GetChatUsers(int chatId)
        {
            return await _context.ChatsToUsers.Where(ctu => ctu.ChatId == chatId).Select(ctu => ctu.UserId).ToListAsync();
        }

        public async Task<WebChatViewModel> CreateChat(WebChatUserViewModel firstUser, WebChatUserViewModel secondUser)
        {
            var chat = await _context.Chats
                .Include(c => c.Users)
                .ThenInclude(u => u.User)
                .FirstOrDefaultAsync(c => c.Users.Any(u => u.UserId == firstUser.Id) && c.Users.Any(u => u.UserId == secondUser.Id));

            if (chat != null)
                return _mapper.Map<WebChatViewModel>(chat);

            chat = new WebChat
            {
                Name = $"{firstUser.Id:D}|{secondUser.Id:D}",
                Users = new List<WebChatToUser>
                {
                    new WebChatToUser { UserId = firstUser.Id },
                    new WebChatToUser { UserId = secondUser.Id }
                }
            };

            _context.Chats.Add(chat);

            await _context.SaveChangesAsync();
            return _mapper.Map<WebChatViewModel>(chat);
        }
    }
}
