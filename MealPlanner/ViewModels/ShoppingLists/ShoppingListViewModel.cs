namespace MealPlanner.ViewModels.ShoppingLists
{
    public class ShoppingListViewModel
    {
        public int? ShoppingListId { get; set; }

        public int? MealPlanId { get; set; }

        public DateOnly WeekStart { get; set; }

        public DateOnly WeekEnd => WeekStart.AddDays(6);

        public DateTimeOffset? GeneratedAtUtc { get; set; }

        public bool HasPlannedMeals { get; set; }

        public bool IsOutdated { get; set; }

        public List<ShoppingListItemViewModel> Items { get; set; }
            = new();
    }

    public class ShoppingListItemViewModel
    {
        public int Id { get; set; }

        public int IngredientId { get; set; }

        public string IngredientName { get; set; } = string.Empty;

        public decimal RequiredQuantity { get; set; }

        public decimal AvailableQuantity { get; set; }

        public decimal Quantity { get; set; }

        public string UnitName { get; set; } = string.Empty;

        public string UnitSymbol { get; set; } = string.Empty;

        public bool IsPurchased { get; set; }
    }
}