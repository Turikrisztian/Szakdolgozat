using MealPlanner.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MealPlanner.Data.Configurations
{
    public class ShoppingListItemConfiguration : IEntityTypeConfiguration<ShoppingListItem>
    {
        public void Configure(EntityTypeBuilder<ShoppingListItem> builder)
        {
            builder.ToTable("ShoppingListItems", table =>
            {
                table.HasCheckConstraint("CK_ShoppingListItems_RequiredQuantity_Positive", "[RequiredQuantity] > 0");
                table.HasCheckConstraint("CK_ShoppingListItems_AvailableQuantity_NonNegative", "[AvailableQuantity] >= 0");
                table.HasCheckConstraint("CK_ShoppingListItems_Quantity_Positive", "[Quantity] > 0");
            });

            builder.HasKey(item => item.Id);
            builder.Property(item => item.RequiredQuantity).HasPrecision(18, 3).IsRequired();
            builder.Property(item => item.AvailableQuantity).HasPrecision(18, 3).IsRequired();
            builder.Property(item => item.Quantity).HasPrecision(18, 3).IsRequired();
            builder.Property(item => item.IsPurchased).IsRequired();
            builder.HasIndex(item => new { item.ShoppingListId, item.IngredientId }).IsUnique();

            builder.HasOne(item => item.ShoppingList)
                .WithMany(list => list.Items)
                .HasForeignKey(item => item.ShoppingListId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(item => item.Ingredient)
                .WithMany()
                .HasForeignKey(item => item.IngredientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(item => item.Unit)
                .WithMany()
                .HasForeignKey(item => item.UnitId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
