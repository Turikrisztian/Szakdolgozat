using Microsoft.AspNetCore.Identity;

namespace MealPlanner.Models
{
    public class PantryItem
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public IdentityUser User { get; set; } = null!;

        public int IngredientId { get; set; }

        public Ingredient Ingredient { get; set; } = null!;

        public decimal Quantity { get; set; }

        public int UnitId { get; set; }

        public Unit Unit { get; set; } = null!;

        public DateTimeOffset UpdatedAtUtc { get; set; }
            = DateTimeOffset.UtcNow;
    }
}