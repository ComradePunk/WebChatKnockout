using Application.Models;
using Application.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
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

        public async Task SendMessage(SendMessageModel messageModel)
        {
            var senderIdStr = Context.User.GetUserId();
            if (string.IsNullOrWhiteSpace(senderIdStr) || !Guid.TryParse(senderIdStr, out var senderId))
                return;

            messageModel.SenderId = senderId;
            messageModel.SenderName = Context.User.GetUserName();

            using (var scope = _serviceProvider.CreateScope())
            {
                var chatService = scope.ServiceProvider.GetService<ChatService>();
                var mapper = scope.ServiceProvider.GetService<IMapper>();
                var message = await chatService.SendMessage(messageModel);
                if (message == null)
                    return;

                var ids = await chatService.GetChatUsers(messageModel.ChatId);

                var tasks = new List<Task>(ids.Count);
                foreach(var id in ids)
                {
                    message.IsRead = message.SenderId == id;
                    tasks.Add(Clients.User(id.ToString("D")).SendAsync("ReceiveMessage", mapper.Map<MessageViewModel>(message)));
                }
                await Task.WhenAll(tasks);
            }
        }

        public async Task ReadMessage(int messageId)
        {
            var idStr = Context.User.GetUserId();
            if (string.IsNullOrWhiteSpace(idStr) || !Guid.TryParse(idStr, out var id))
                return;

            using (var scope = _serviceProvider.CreateScope())
            {
                var chatService = scope.ServiceProvider.GetService<ChatService>();
                await chatService.ReadMessage(id, messageId);
                await Clients.User(idStr).SendAsync("MessageRead", messageId);
            }
        }
    }
}
