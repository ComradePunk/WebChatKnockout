using Application.Models;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WebChatMvc.Extensions;

namespace WebChatMvc.Controllers
{
    [Authorize]
    [Route("api/users"), Produces("application/json")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet("currentUser")]
        public IActionResult LoggedInUser()
        {
            var idStr = HttpContext.User.GetUserId();
            if (!string.IsNullOrWhiteSpace(idStr) && Guid.TryParse(idStr, out var id))
                return Ok(new WebChatUserViewModel { Id = id, UserName = HttpContext.User.GetUserName() });

            return Unauthorized();
        }

        [HttpGet("online")]
        public async Task<IActionResult> OnlineUsers()
        {
            var idStr = HttpContext.User.GetUserId();
            if (!string.IsNullOrWhiteSpace(idStr) && Guid.TryParse(idStr, out var id))
                return Ok(await _userService.GetOnlineUsers(id));

            return Unauthorized();
        }
    }
}
