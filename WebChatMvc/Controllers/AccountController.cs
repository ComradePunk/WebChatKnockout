using Application.Models;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using WebChatMvc.Extensions;
using WebChatMvc.Hubs;

namespace WebChatMvc.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<WebChatUser> _userManager;
        private readonly SignInManager<WebChatUser> _signInManager;
        private readonly IHubContext<WebChatHub> _hubContext;
        private readonly IMapper _mapper;

        public AccountController(UserManager<WebChatUser> userManager
                , SignInManager<WebChatUser> signInManager
                , IHubContext<WebChatHub> hubContext
                , IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _hubContext = hubContext;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        private async Task SendUserEvent(string eventName, string userId, string userName)
        {
            if (!string.IsNullOrWhiteSpace(userId) && Guid.TryParse(userId, out var id))
                await _hubContext.Clients.All.SendAsync(eventName, new WebChatUserViewModel { Id = id, UserName = userName });
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _mapper.Map<WebChatUser>(model);

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, false);
                    await SendUserEvent("UserLoggedIn", user.Id.ToString("D"), user.UserName);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result =
                    await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByNameAsync(model.UserName);
                    await SendUserEvent("UserLoggedIn", user.Id.ToString("D"), user.UserName);
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Неправильный логин и (или) пароль");
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await SendUserEvent("UserLoggedOut", HttpContext.User.GetUserId(), HttpContext.User.GetUserName());
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
