using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class WebChatMessage
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public Guid SenderId { get; set; }
        public string Text { get; set; }
        public DateTimeOffset SentTime { get; set; }

        public WebChat Chat { get; set; }
        public WebChatUser Sender { get; set; }

        public List<WebMessageToRecepient> Recepients { get; set; }
    }
}
