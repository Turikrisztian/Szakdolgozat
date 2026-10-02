using MealPlanner.Models.Enums;

namespace MealPlanner.Models
{
    public class MealPlanItem
    {
        public int Id { get; set; }
        public int MealPlanId { get; set; }
        public MealPlan MealPlan { get; set; } = null!;
        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; } = null!;

        // Az adatbázisban már létező, konkrét naptári dátum.
        public DateOnly Date { get; set; }
        public MealType MealType { get; set; }
        public int Servings { get; set; }
    }
}
