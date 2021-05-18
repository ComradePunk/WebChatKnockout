using Application.Models;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain;
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

        public async Task<List<WebChatUserViewModel>> GetOnlineUsers()
        {
            var now = DateTimeOffset.UtcNow;
            return await _context.AuthenticationTickets
                .Include(t => t.User)
                .Where(t => !t.Expires.HasValue || t.Expires.Value > now).Select(t => t.User)
                .ProjectTo<WebChatUserViewModel>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<List<WebChatContactViewModel>> GetWebChatContacts(Guid userId)
        {
            var now = DateTimeOffset.UtcNow;

            var contacts = await _context.Users.Where(u => u.Id != userId).Select(u => new WebChatContactViewModel { UserId = u.Id, UserName = u.UserName }).ToListAsync();
            var activeTicketIds = new HashSet<Guid>(await _context.AuthenticationTickets.Where(t => !t.Expires.HasValue || t.Expires.Value > now).Select(t => t.UserId).ToListAsync());
            var availableChats = await _context.ChatsToUsers.Where(ctu => ctu.UserId == userId).ToDictionaryAsync(ctu => ctu.UserId, ctu => ctu.ChatId);

            var unreadMessageFromChatIds = await _context.Messages.Where(m => availableChats.Values.Contains(m.ChatId) && m.Recepients.All(r => r.RecepientId != userId)).Select(m => m.ChatId).ToListAsync();
            var unreadMessagesByChatCount = unreadMessageFromChatIds.GroupBy(chatId => chatId).ToDictionary(x => x.Key, x => x.Count());

            foreach (var contact in contacts)
            {
                contact.IsOnline = activeTicketIds.Contains(contact.UserId);
                if (!availableChats.TryGetValue(contact.UserId, out var chatId))
                    continue;

                contact.ChatId = chatId;
                if (unreadMessagesByChatCount.TryGetValue(chatId, out var count))
                    contact.UnreadMessagesCount = count;
            }

            return contacts;
        }
    }
}
