using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class WebChatConfiguration : IEntityTypeConfiguration<WebChat>
    {
        public void Configure(EntityTypeBuilder<WebChat> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name).HasMaxLength(128);
        }
    }
}
