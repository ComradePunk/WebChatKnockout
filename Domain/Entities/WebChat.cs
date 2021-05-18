using System.Collections.Generic;

namespace Domain.Entities
{
    public class WebChat
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<WebChatToUser> Users { get; set; }
        public List<WebChatMessage> Messages { get; set; }
    }
}
