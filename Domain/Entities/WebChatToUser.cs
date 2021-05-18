using System;

namespace Domain.Entities
{
    public class WebChatToUser
    {
        public int ChatId { get; set; }
        public Guid UserId { get; set; }

        public WebChat Chat { get; set; }
        public WebChatUser User { get; set; }
    }
}
