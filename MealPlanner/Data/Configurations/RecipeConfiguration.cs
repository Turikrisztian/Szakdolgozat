using MealPlanner.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MealPlanner.Data.Configurations
{
    public class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
    {
        public void Configure(EntityTypeBuilder<Recipe> builder)
        {
            builder.ToTable("Recipes", table =>
            {
                table.HasCheckConstraint(
                    "CK_Recipes_Servings_Positive",
                    "[Servings] > 0");

                table.HasCheckConstraint(
                    "CK_Recipes_PreparationTimeMinutes_Positive",
                    "[PreparationTimeMinutes] > 0");
            });

            builder.HasKey(recipe => recipe.Id);

            builder.Property(recipe => recipe.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(recipe => recipe.Instructions)
                .IsRequired()
                .HasMaxLength(10000);

            builder.Property(recipe => recipe.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(recipe => recipe.CreatedAtUtc)
                .IsRequired();

            builder.HasOne(recipe => recipe.Category)
                .WithMany()
                .HasForeignKey(recipe => recipe.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(recipe => recipe.User)
                .WithMany()
                .HasForeignKey(recipe => recipe.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}