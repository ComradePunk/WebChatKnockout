using System;

namespace Domain.Entities
{
    public class WebMessageToRecepient
    {
        public int MessageId { get; set; }
        public Guid RecepientId { get; set; }
        public DateTimeOffset ReceivedTime { get; set; }
        public DateTimeOffset? ReadTime { get; set; }

        public WebChatMessage Message { get; set; }
        public WebChatUser Recepient { get; set; }
    }
}
