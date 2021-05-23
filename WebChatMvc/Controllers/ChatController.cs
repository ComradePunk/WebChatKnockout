using Application.Models;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using WebChatMvc.Extensions;
using WebChatMvc.Hubs;

namespace WebChatMvc.Controllers
{
    [Authorize]
    [Route("api/chat"), Produces("application/json")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ChatService _service;

        public ChatController(ChatService service)
        {
            _service = service;
        }

        [HttpGet("usersOnline")]
        public async Task<IActionResult> OnlineUsers()
        {
            var idStr = HttpContext.User.GetUserId();
            if (!string.IsNullOrWhiteSpace(idStr) && Guid.TryParse(idStr, out var id))
                return Ok(await _service.GetOnlineUsers(id));

            return Unauthorized();
        }

        [HttpGet]
        public async Task<IActionResult> Chats()
        {
            var idStr = HttpContext.User.GetUserId();
            if (!string.IsNullOrWhiteSpace(idStr) && Guid.TryParse(idStr, out var id))
                return Ok(await _service.GetWebChats(id));

            return Unauthorized();
        }

        [HttpGet("messages/{chatId}")]
        public async Task<IActionResult> Messages(int chatId, [FromQuery] int? pageNumber, [FromQuery] int? pageSize)
        {
            var idStr = HttpContext.User.GetUserId();
            if (!string.IsNullOrWhiteSpace(idStr) && Guid.TryParse(idStr, out var id))
            {
                return Ok(await _service.GetMessages(chatId, id, pageNumber, pageSize));
            }

            return Unauthorized();
        }

        [HttpPost("{userId}")]
        public async Task<IActionResult> CreateChat(Guid userId)
        {
            var idStr = HttpContext.User.GetUserId();
            if (!string.IsNullOrWhiteSpace(idStr) && Guid.TryParse(idStr, out var id))
            {
                return Ok(await _service.CreateChat(id, userId));
            }

            return Unauthorized();
        }
    }
}
