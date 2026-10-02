namespace MealPlanner.Models
{
    public class ShoppingList
    {
        public int Id { get; set; }
        public int MealPlanId { get; set; }
        public MealPlan MealPlan { get; set; } = null!;
        public DateTimeOffset GeneratedAtUtc { get; set; } = DateTimeOffset.UtcNow;
        public ICollection<ShoppingListItem> Items { get; set; } = new List<ShoppingListItem>();
    }
}
