using System;

namespace Application.Models
{
    public class MessageViewModel
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public Guid SenderId { get; set; }
        public string SenderName { get; set; }
        public string Text { get; set; }
        public DateTimeOffset SentTime { get; set; }
        public bool IsRead { get; set; }
    }
}
