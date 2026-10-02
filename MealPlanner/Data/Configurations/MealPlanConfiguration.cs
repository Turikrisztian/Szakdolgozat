using MealPlanner.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MealPlanner.Data.Configurations
{
    public class MealPlanConfiguration : IEntityTypeConfiguration<MealPlan>
    {
        public void Configure(EntityTypeBuilder<MealPlan> builder)
        {
            builder.ToTable("MealPlans");
            builder.HasKey(plan => plan.Id);
            builder.Property(plan => plan.UserId).IsRequired().HasMaxLength(450);
            builder.Property(plan => plan.WeekStartDate).HasColumnType("date").IsRequired();
            builder.Property(plan => plan.CreatedAtUtc).IsRequired();
            builder.Property(plan => plan.UpdatedAtUtc).IsRequired(false);

            builder.HasIndex(plan => new { plan.UserId, plan.WeekStartDate }).IsUnique();

            builder.HasOne(plan => plan.User)
                .WithMany()
                .HasForeignKey(plan => plan.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
