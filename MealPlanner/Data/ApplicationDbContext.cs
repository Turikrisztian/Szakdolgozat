using MealPlanner.Data.Configurations;
using MealPlanner.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Data
{
    public class ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : IdentityDbContext(options)
    {
        public DbSet<Category> Categories => Set<Category>();

        public DbSet<Unit> Units => Set<Unit>();

        public DbSet<Ingredient> Ingredients => Set<Ingredient>();

        public DbSet<Recipe> Recipes => Set<Recipe>();

        public DbSet<RecipeIngredient> RecipeIngredients
            => Set<RecipeIngredient>();

        public DbSet<PantryItem> PantryItems => Set<PantryItem>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfiguration(new CategoryConfiguration());
            builder.ApplyConfiguration(new UnitConfiguration());
            builder.ApplyConfiguration(new IngredientConfiguration());
            builder.ApplyConfiguration(new RecipeConfiguration());
            builder.ApplyConfiguration(new RecipeIngredientConfiguration());
            builder.ApplyConfiguration(new PantryItemConfiguration());
        }
    }
}