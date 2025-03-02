﻿using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WebChatMvc.Extensions;

namespace WebChatMvc.Controllers
{
    [Authorize]
    [Route("api/chat"), Produces("application/json")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ChatService _chatService;

        public ChatController(ChatService service)
        {
            _chatService = service;
        }

        [HttpGet]
        public async Task<IActionResult> Chats()
        {
            var idStr = HttpContext.User.GetUserId();
            if (!string.IsNullOrWhiteSpace(idStr) && Guid.TryParse(idStr, out var id))
                return Ok(await _chatService.GetWebChats(id));

            return Unauthorized();
        }
    }
}
