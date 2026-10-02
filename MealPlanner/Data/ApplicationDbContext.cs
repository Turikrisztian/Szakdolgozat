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

        public DbSet<MealPlan> MealPlans => Set<MealPlan>();

        public DbSet<MealPlanItem> MealPlanItems => Set<MealPlanItem>();

        public DbSet<ShoppingList> ShoppingLists => Set<ShoppingList>();

        public DbSet<ShoppingListItem> ShoppingListItems => Set<ShoppingListItem>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfiguration(new CategoryConfiguration());
            builder.ApplyConfiguration(new UnitConfiguration());
            builder.ApplyConfiguration(new IngredientConfiguration());
            builder.ApplyConfiguration(new RecipeConfiguration());
            builder.ApplyConfiguration(new RecipeIngredientConfiguration());
            builder.ApplyConfiguration(new PantryItemConfiguration());

            builder.ApplyConfiguration(new MealPlanConfiguration());
            builder.ApplyConfiguration(new MealPlanItemConfiguration());
            builder.ApplyConfiguration(new ShoppingListConfiguration());
            builder.ApplyConfiguration(new ShoppingListItemConfiguration());
        }
    }
}
