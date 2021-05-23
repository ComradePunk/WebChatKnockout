using Application.Contracts;
using Application.Extensions;
using Application.Filters;
using Application.Models;
using AutoMapper;
using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class MessageService : IScopedService
    {
        private readonly WebChatContext _context;
        private readonly IMapper _mapper;

        public MessageService(WebChatContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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
                message.IsRead = message.SenderId == userId || messageDictionary[message.Id].Recepients.Any(r => r.RecepientId == userId && r.ReadTime.HasValue);

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

        public async Task ReadMessage(Guid userId, int messageId)
        {
            if (!await _context.Messages.AnyAsync(m => m.Id == messageId && m.SenderId != userId))
                return;

            var now = DateTimeOffset.Now;
            var messageStatus = await _context.MessageRecepients.FirstOrDefaultAsync(mr => mr.MessageId == messageId && mr.RecepientId == userId);
            if (messageStatus == null)
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
