using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Configurations
{
    public class WebMessageToRecepientConfiguration : IEntityTypeConfiguration<WebMessageToRecepient>
    {
        public void Configure(EntityTypeBuilder<WebMessageToRecepient> builder)
        {
            builder.HasKey(mtr => new { mtr.MessageId, mtr.RecepientId });
            builder.Property(mtr => mtr.ReceivedTime).HasDefaultValueSql("SYSDATETIMEOFFSET()");

            builder.HasOne(mtr => mtr.Message).WithMany(m => m.Recepients).HasForeignKey(mtr => mtr.MessageId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(mtr => mtr.Recepient).WithMany().HasForeignKey(mtr => mtr.RecepientId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
