using MealPlanner.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MealPlanner.Data.Configurations
{
    public class IngredientConfiguration
        : IEntityTypeConfiguration<Ingredient>
    {
        public void Configure(EntityTypeBuilder<Ingredient> builder)
        {
            builder.ToTable("Ingredients");

            builder.HasKey(ingredient => ingredient.Id);

            builder.Property(ingredient => ingredient.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.HasIndex(ingredient => ingredient.Name)
                .IsUnique();

            builder.Property(ingredient => ingredient.MeasurementType)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(ingredient => ingredient.IsActive)
                .IsRequired();
        }
    }
}