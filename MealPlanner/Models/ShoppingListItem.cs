namespace MealPlanner.Models
{
    public class ShoppingListItem
    {
        public int Id { get; set; }
        public int ShoppingListId { get; set; }
        public ShoppingList ShoppingList { get; set; } = null!;
        public int IngredientId { get; set; }
        public Ingredient Ingredient { get; set; } = null!;
        public decimal RequiredQuantity { get; set; }
        public decimal AvailableQuantity { get; set; }
        public decimal Quantity { get; set; }
        public int UnitId { get; set; }
        public Unit Unit { get; set; } = null!;
        public bool IsPurchased { get; set; }
    }
}
