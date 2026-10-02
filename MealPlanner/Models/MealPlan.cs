using Microsoft.AspNetCore.Identity;

namespace MealPlanner.Models
{
    public class MealPlan
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public IdentityUser User { get; set; } = null!;

        public DateOnly WeekStartDate { get; set; }

        public DateTimeOffset CreatedAtUtc { get; set; }
            = DateTimeOffset.UtcNow;

        public DateTimeOffset? UpdatedAtUtc { get; set; }

        public ShoppingList? ShoppingList { get; set; }

        public ICollection<MealPlanItem> Items { get; set; }
            = new List<MealPlanItem>();
    }
}
