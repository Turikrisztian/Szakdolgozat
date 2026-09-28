using MealPlanner.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MealPlanner.Data.Configurations
{
    public class RecipeIngredientConfiguration
        : IEntityTypeConfiguration<RecipeIngredient>
    {
        public void Configure(EntityTypeBuilder<RecipeIngredient> builder)
        {
            builder.ToTable("RecipeIngredients", table =>
            {
                table.HasCheckConstraint(
                    "CK_RecipeIngredients_Quantity_Positive",
                    "[Quantity] > 0");
            });

            builder.HasKey(recipeIngredient => recipeIngredient.Id);

            builder.Property(recipeIngredient => recipeIngredient.Quantity)
                .HasPrecision(18, 3)
                .IsRequired();

            builder.HasIndex(recipeIngredient => new
            {
                recipeIngredient.RecipeId,
                recipeIngredient.IngredientId
            })
                .IsUnique();

            builder.HasOne(recipeIngredient => recipeIngredient.Recipe)
                .WithMany(recipe => recipe.RecipeIngredients)
                .HasForeignKey(recipeIngredient => recipeIngredient.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(recipeIngredient => recipeIngredient.Ingredient)
                .WithMany()
                .HasForeignKey(recipeIngredient => recipeIngredient.IngredientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(recipeIngredient => recipeIngredient.Unit)
                .WithMany()
                .HasForeignKey(recipeIngredient => recipeIngredient.UnitId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}