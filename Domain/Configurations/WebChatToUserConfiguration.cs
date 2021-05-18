using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class WebChatToUserConfiguration : IEntityTypeConfiguration<WebChatToUser>
    {
        public void Configure(EntityTypeBuilder<WebChatToUser> builder)
        {
            builder.HasKey(ctu => new { ctu.UserId, ctu.ChatId });

            builder.HasOne(ctu => ctu.User)
                .WithMany(u => u.Chats)
                .HasForeignKey(ctu => ctu.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ctu => ctu.Chat)
                .WithMany(u => u.Users)
                .HasForeignKey(ctu => ctu.ChatId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
