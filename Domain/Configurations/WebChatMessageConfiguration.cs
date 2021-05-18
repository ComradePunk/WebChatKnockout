using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class WebChatMessageConfiguration : IEntityTypeConfiguration<WebChatMessage>
    {
        public void Configure(EntityTypeBuilder<WebChatMessage> builder)
        {
            builder.HasKey(m => m.Id);
            builder.Property(m => m.ChatId).IsRequired();

            builder.Property(m => m.Text).HasMaxLength(1024).IsRequired();
            builder.Property(m => m.SentTime).HasDefaultValueSql("SYSDATETIMEOFFSET()");

            builder.HasIndex(m => m.SentTime);

            builder.HasOne(m => m.Chat).WithMany(c => c.Messages).HasForeignKey(m => m.ChatId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(m => m.Sender).WithMany(u => u.Messages).HasForeignKey(m => m.SenderId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
