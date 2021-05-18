using System;

namespace Application.Models
{
    public class WebChatContactViewModel
    {
        public int? ChatId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public int UnreadMessagesCount { get; set; }
        public bool IsOnline { get; set; }
    }
}
