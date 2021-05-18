using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class WebChatAuthenticationTicketConfiguration : IEntityTypeConfiguration<WebChatAuthenticationTicket>
    {
        public void Configure(EntityTypeBuilder<WebChatAuthenticationTicket> builder)
        {
            builder.HasKey(a => a.Id);

            builder.HasIndex(a => a.Expires);

            builder.HasOne(a => a.User)
                .WithMany(u => u.Tickets)
                .HasForeignKey(a => a.UserId)
                .IsRequired();
        }
    }
}
