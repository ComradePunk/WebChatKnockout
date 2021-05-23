using Application.Contracts;
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
    public class UserService : IScopedService
    {
        private readonly WebChatContext _context;
        private readonly IMapper _mapper;

        public UserService(WebChatContext context, IMapper mapper)
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
                .Distinct()
                .ProjectTo<WebChatUserViewModel>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<List<WebChatUserViewModel>> GetUsersOfflineSince(DateTimeOffset time)
        {
            var now = DateTimeOffset.UtcNow;
            var usersWithExpiredTickets = await _context.AuthenticationTickets
                .Include(t => t.User)
                .Where(t => t.Expires.HasValue && t.Expires.Value >= time && t.Expires.Value <= now).Select(t => t.User)
                .Distinct()
                .ProjectTo<WebChatUserViewModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            var onlineUsers = await GetOnlineUsers(Guid.Empty);

            return usersWithExpiredTickets.Except(onlineUsers).ToList();
        }
    }
}
