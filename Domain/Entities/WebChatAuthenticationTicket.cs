using System;

namespace Domain.Entities
{
    public class WebChatAuthenticationTicket
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public WebChatUser User { get; set; }

        public byte[] Value { get; set; }

        public DateTimeOffset LastActivity { get; set; }

        public DateTimeOffset? Expires { get; set; }
    }
}
