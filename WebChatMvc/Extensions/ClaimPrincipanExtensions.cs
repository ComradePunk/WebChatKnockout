using System.Linq;
using System.Security.Claims;

namespace WebChatMvc.Extensions
{
    public static class ClaimPrincipanExtensions
    {
        public static string GetUserId(this ClaimsPrincipal user)
        {
            return user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
