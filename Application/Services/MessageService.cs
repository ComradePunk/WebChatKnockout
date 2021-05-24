using Application.Contracts;
using Application.Enums;
using Application.Extensions;
using Application.Filters;
using Application.Models;
using AutoMapper;
using AutoMapper.QueryableExtensions;
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

        public async Task<MessageViewModel> GetMessageById(int messageId)
        {
            return await _context.Messages.ProjectTo<MessageViewModel>(_mapper.ConfigurationProvider).FirstOrDefaultAsync(m => m.Id == messageId);
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
            {
                var recepient = messageDictionary[message.Id].Recepients.FirstOrDefault(r => message.SenderId == userId || r.RecepientId == userId);
                message.Status = recepient == null ? MessageStatus.Sent : (recepient.ReadTime.HasValue ? MessageStatus.Read : MessageStatus.Received);
            }

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

        private async Task UpdateMessageStatus(Guid userId, int messageId, MessageStatus toStatus)
        {
            if (toStatus == MessageStatus.Sent || !await _context.Messages.AnyAsync(m => m.Id == messageId && m.SenderId != userId))
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

            if (toStatus == MessageStatus.Read)
                messageStatus.ReadTime = now;
            await _context.SaveChangesAsync();
        }

        public async Task ReceiveMessage(Guid userId, int messageId)
        {
            await UpdateMessageStatus(userId, messageId, MessageStatus.Received);
        }

        public async Task ReadMessage(Guid userId, int messageId)
        {
            await UpdateMessageStatus(userId, messageId, MessageStatus.Read);
        }
    }
}
