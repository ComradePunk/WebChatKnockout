using Domain;
using Domain.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using WebChatMvc.Extensions;

namespace WebChatMvc.Services
{
    public class TicketStore : ITicketStore
    {
        private readonly IServiceProvider _serviceProvider;

        public TicketStore(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private byte[] SerializeToBytes(AuthenticationTicket source)
            => TicketSerializer.Default.Serialize(source);

        private AuthenticationTicket DeserializeFromBytes(byte[] source)
            => source == null ? null : TicketSerializer.Default.Deserialize(source);

        public async Task RemoveAsync(string key)
        {
            if (!Guid.TryParse(key, out var id))
                return;

            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<WebChatContext>();

                var ticket = await context.AuthenticationTickets.SingleOrDefaultAsync(t => t.Id == id);
                if (ticket == null)
                    return;

                context.AuthenticationTickets.Remove(ticket);
                await context.SaveChangesAsync();
            }
        }

        public async Task RenewAsync(string key, AuthenticationTicket ticket)
        {
            if (!Guid.TryParse(key, out var id))
                return;


            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<WebChatContext>();
                var authenticationTicket = await context.AuthenticationTickets.FindAsync(id);
                if (authenticationTicket == null)
                    return;

                authenticationTicket.Value = SerializeToBytes(ticket);
                authenticationTicket.LastActivity = DateTimeOffset.UtcNow;
                authenticationTicket.Expires = ticket.Properties.ExpiresUtc;
                await context.SaveChangesAsync();
            }
        }

        public async Task<AuthenticationTicket> RetrieveAsync(string key)
        {
            if (!Guid.TryParse(key, out var id))
                return null;

            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<WebChatContext>();
                var authenticationTicket = await context.AuthenticationTickets.FindAsync(id);
                if (authenticationTicket == null)
                    return null;

                authenticationTicket.LastActivity = DateTimeOffset.UtcNow;
                await context.SaveChangesAsync();

                return DeserializeFromBytes(authenticationTicket.Value);
            }
        }

        public async Task<string> StoreAsync(AuthenticationTicket ticket)
        {
            var nameIdentifier = ticket.Principal.GetUserId();
            if (!Guid.TryParse(nameIdentifier, out var userId))
                return null;

            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<WebChatContext>();

                if (!await context.Users.AnyAsync(u => u.Id == userId))
                    return null;

                var authenticationTicket = new WebChatAuthenticationTicket
                {
                    UserId = userId,
                    Value = SerializeToBytes(ticket),
                    LastActivity = DateTimeOffset.UtcNow,
                    Expires = ticket.Properties.ExpiresUtc
                };

                await context.AuthenticationTickets.AddAsync(authenticationTicket);
                await context.SaveChangesAsync();

                return authenticationTicket.Id.ToString();
            }
        }
    }
}
