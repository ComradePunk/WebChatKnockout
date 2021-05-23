using System;

namespace Application.Models
{
    public class SendMessageModel
    {
        public int ChatId { get; set; }
        public string Text { get; set; }
        public Guid SenderId { get; set; }
        public string SenderName { get; set; }
    }
}
