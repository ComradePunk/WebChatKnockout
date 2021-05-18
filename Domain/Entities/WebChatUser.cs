using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class WebChatUser : IdentityUser<Guid>
    {
        public List<WebChatAuthenticationTicket> Tickets { get; set; }
        public List<WebChatToUser> Chats { get; set; }
        public List<WebChatMessage> Messages { get; set; }
    }
}
