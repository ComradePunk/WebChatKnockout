using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WebChatMvc.Extensions;

namespace WebChatMvc.Controllers
{
    [Authorize]
    [Route("api/messages"), Produces("application/json")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly MessageService _messageService;

        public MessagesController(MessageService messageService)
        {
            _messageService = messageService;
        }

        [HttpGet("{chatId}")]
        public async Task<IActionResult> Messages(int chatId, [FromQuery] int? pageNumber, [FromQuery] int? pageSize)
        {
            var idStr = HttpContext.User.GetUserId();
            if (!string.IsNullOrWhiteSpace(idStr) && Guid.TryParse(idStr, out var id))
            {
                return Ok(await _messageService.GetMessages(chatId, id, pageNumber, pageSize));
            }

            return Unauthorized();
        }
    }
}
