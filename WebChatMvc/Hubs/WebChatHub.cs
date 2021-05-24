using Application.Models;
using Application.Services;
using AutoMapper;
using Domain;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebChatMvc.Extensions;

namespace WebChatMvc.Hubs
{
    [Authorize]
    public class WebChatHub : Hub
    {
        private readonly IServiceProvider _serviceProvider;

        public WebChatHub(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private async Task RenewAuthenticationTicket(IServiceScope scope)
        {
            var context = scope.ServiceProvider.GetRequiredService<WebChatContext>();
            var now = DateTimeOffset.Now;
            var idStr = Context.User.GetUserId();
            if (string.IsNullOrWhiteSpace(idStr) || !Guid.TryParse(idStr, out var userId))
                return;

            var authenticationTickets = await context.AuthenticationTickets.Where(t => t.UserId == userId && t.Expires >= now).ToListAsync();

            var ticketStore = scope.ServiceProvider.GetService<ITicketStore>();

            foreach (var authenticationTicket in authenticationTickets)
            {
                var ticket = authenticationTicket.Value.DeserializeAuthenticationTicket();
                if (ticket == null)
                    continue;

                if (ticket.Properties.ExpiresUtc.HasValue)
                    ticket.Properties.ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(5);
                await ticketStore.RenewAsync(authenticationTicket.Id.ToString("D"), ticket);
            }
        }

        public async Task SendMessage(SendMessageModel messageModel)
        {
            var senderIdStr = Context.User.GetUserId();
            if (string.IsNullOrWhiteSpace(senderIdStr) || !Guid.TryParse(senderIdStr, out var senderId))
                return;

            messageModel.SenderId = senderId;
            messageModel.SenderName = Context.User.GetUserName();

            using (var scope = _serviceProvider.CreateScope())
            {
                await RenewAuthenticationTicket(scope);
                var chatService = scope.ServiceProvider.GetService<ChatService>();
                var messageService = scope.ServiceProvider.GetService<MessageService>();

                var mapper = scope.ServiceProvider.GetService<IMapper>();
                var message = await messageService.SendMessage(messageModel);
                if (message == null)
                    return;

                var ids = await chatService.GetChatUsers(messageModel.ChatId);

                var tasks = new List<Task>(ids.Count);
                foreach(var id in ids)
                {
                    tasks.Add(Clients.User(id.ToString("D")).SendAsync("ReceiveMessage", message));
                }
                await Task.WhenAll(tasks);
            }
        }

        public async Task MessageRecepientGotMessage(int messageId)
        {
            var idStr = Context.User.GetUserId();
            if (string.IsNullOrWhiteSpace(idStr) || !Guid.TryParse(idStr, out var id))
                return;

            using (var scope = _serviceProvider.CreateScope())
            {
                await RenewAuthenticationTicket(scope);
                var messageService = scope.ServiceProvider.GetService<MessageService>();
                await messageService.ReceiveMessage(id, messageId);

                var message = await messageService.GetMessageById(messageId);
                await Clients.User(message.SenderId.ToString("D")).SendAsync("MessageReceived", messageId);
                await Clients.User(idStr).SendAsync("MessageReceived", messageId);
            }
        }

        public async Task ReadMessage(int messageId)
        {
            var idStr = Context.User.GetUserId();
            if (string.IsNullOrWhiteSpace(idStr) || !Guid.TryParse(idStr, out var id))
                return;

            using (var scope = _serviceProvider.CreateScope())
            {
                await RenewAuthenticationTicket(scope);
                var messageService = scope.ServiceProvider.GetService<MessageService>();
                await messageService.ReadMessage(id, messageId);

                var message = await messageService.GetMessageById(messageId);
                await Clients.User(message.SenderId.ToString("D")).SendAsync("MessageRead", messageId);
                await Clients.User(idStr).SendAsync("MessageRead", messageId);
            }
        }

        public async Task CreateChat(WebChatUserViewModel user)
        {
            var idStr = Context.User.GetUserId();
            if (string.IsNullOrWhiteSpace(idStr) || !Guid.TryParse(idStr, out var id))
                return;

            using (var scope = _serviceProvider.CreateScope())
            {
                await RenewAuthenticationTicket(scope);
                var chatService = scope.ServiceProvider.GetService<ChatService>();

                var me = new WebChatUserViewModel { Id = id, UserName = Context.User.GetUserName() };
                var chat = await chatService.CreateChat(me, user);

                chat.Name = user.UserName;
                await Clients.User(idStr).SendAsync("ChatCreated", chat);

                chat.Name = me.UserName;
                await Clients.User(user.Id.ToString("D")).SendAsync("ChatCreated", chat);
            }
        }
    }
}
