using Microsoft.AspNetCore.Identity;

namespace MealPlanner.Models
{
    public class Recipe
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Instructions { get; set; } = string.Empty;

        public int Servings { get; set; } = 1;

        public int PreparationTimeMinutes { get; set; }

        public int CategoryId { get; set; }

        public Category Category { get; set; } = null!;

        public string UserId { get; set; } = string.Empty;

        public IdentityUser User { get; set; } = null!;

        public bool IsPublic { get; set; } = false;

        public bool IsArchived { get; set; } = false;

        public DateTimeOffset CreatedAtUtc { get; set; }
            = DateTimeOffset.UtcNow;

        public DateTimeOffset? UpdatedAtUtc { get; set; }
        public ICollection<RecipeIngredient> RecipeIngredients { get; set; }
    = new List<RecipeIngredient>();
    }
}