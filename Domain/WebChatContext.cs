using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace Domain
{
    public class WebChatContext : IdentityDbContext<WebChatUser, IdentityRole<Guid>, Guid>
    {
        public WebChatContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<WebChatAuthenticationTicket> AuthenticationTickets { get; set; }
        public DbSet<WebChat> Chats { get; set; }
        public DbSet<WebChatToUser> ChatsToUsers { get; set; }
        public DbSet<WebChatMessage> Messages { get; set; }
        public DbSet<WebMessageToRecepient> MessageRecepients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(WebChatContext).Assembly);
        }
    }
}
