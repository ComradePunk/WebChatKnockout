using Microsoft.AspNetCore.Authentication;

namespace WebChatMvc.Extensions
{
    public static class AuthenticationTicketExtensions
    {
        public static byte[] SerializeToBytes(this AuthenticationTicket source)
            => TicketSerializer.Default.Serialize(source);

        public static AuthenticationTicket DeserializeAuthenticationTicket(this byte[] source)
            => source == null ? null : TicketSerializer.Default.Deserialize(source);
    }
}
