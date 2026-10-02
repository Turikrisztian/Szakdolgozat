using MealPlanner.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MealPlanner.Data.Configurations
{
    public class MealPlanItemConfiguration : IEntityTypeConfiguration<MealPlanItem>
    {
        public void Configure(EntityTypeBuilder<MealPlanItem> builder)
        {
            builder.ToTable("MealPlanItems", table =>
            {
                table.HasCheckConstraint("CK_MealPlanItems_MealType_Valid", "[MealType] IN (1, 2, 3, 4)");
                table.HasCheckConstraint("CK_MealPlanItems_Servings_Positive", "[Servings] > 0");
            });

            builder.HasKey(item => item.Id);
            builder.Property(item => item.Date).HasColumnType("date").IsRequired();
            builder.Property(item => item.MealType).HasConversion<int>().IsRequired();
            builder.Property(item => item.Servings).IsRequired();

            // A korábban létrehozott index nem egyedi.
            builder.HasIndex(item => new { item.MealPlanId, item.Date, item.MealType });

            builder.HasOne(item => item.MealPlan)
                .WithMany(plan => plan.Items)
                .HasForeignKey(item => item.MealPlanId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(item => item.Recipe)
                .WithMany()
                .HasForeignKey(item => item.RecipeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
