using MealPlanner.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MealPlanner.Data.Configurations
{
    public class PantryItemConfiguration
        : IEntityTypeConfiguration<PantryItem>
    {
        public void Configure(EntityTypeBuilder<PantryItem> builder)
        {
            builder.ToTable("PantryItems", table =>
            {
                table.HasCheckConstraint(
                    "CK_PantryItems_Quantity_NonNegative",
                    "[Quantity] >= 0");
            });

            builder.HasKey(pantryItem => pantryItem.Id);

            builder.Property(pantryItem => pantryItem.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(pantryItem => pantryItem.Quantity)
                .HasPrecision(18, 3)
                .IsRequired();

            builder.Property(pantryItem => pantryItem.UpdatedAtUtc)
                .IsRequired();

            builder.HasIndex(pantryItem => new
            {
                pantryItem.UserId,
                pantryItem.IngredientId
            })
                .IsUnique();

            builder.HasOne(pantryItem => pantryItem.User)
                .WithMany()
                .HasForeignKey(pantryItem => pantryItem.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(pantryItem => pantryItem.Ingredient)
                .WithMany()
                .HasForeignKey(pantryItem => pantryItem.IngredientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(pantryItem => pantryItem.Unit)
                .WithMany()
                .HasForeignKey(pantryItem => pantryItem.UnitId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}