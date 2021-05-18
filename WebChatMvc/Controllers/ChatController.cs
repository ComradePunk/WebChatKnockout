using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebChatMvc.Extensions;

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
            return Ok(await _service.GetOnlineUsers());
        }

        [HttpGet("contacts")]
        public async Task<IActionResult> Contacts()
        {
            var idStr = HttpContext.User.GetUserId();
            if (!string.IsNullOrWhiteSpace(idStr) && Guid.TryParse(idStr, out var id))
                return Ok(await _service.GetWebChatContacts(id));

            return BadRequest();
        }
    }
}
