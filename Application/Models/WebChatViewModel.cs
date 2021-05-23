using System;
using System.Collections.Generic;

namespace Application.Models
{
    public class WebChatViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<WebChatUserViewModel> Users { get; set; }
        public int UnreadMessagesCount { get; set; }
        public bool IsOnline { get; set; }
    }
}
