using MealPlanner.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MealPlanner.Data.Configurations
{
    public class ShoppingListConfiguration : IEntityTypeConfiguration<ShoppingList>
    {
        public void Configure(EntityTypeBuilder<ShoppingList> builder)
        {
            builder.ToTable("ShoppingLists");
            builder.HasKey(list => list.Id);
            builder.Property(list => list.GeneratedAtUtc).IsRequired();
            builder.HasIndex(list => list.MealPlanId).IsUnique();

            builder.HasOne(list => list.MealPlan)
                .WithOne(plan => plan.ShoppingList)
                .HasForeignKey<ShoppingList>(list => list.MealPlanId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
