using MealPlanner.Models.Enums;

namespace MealPlanner.ViewModels.MealPlans
{
    public class MealPlanWeekViewModel
    {
        public DateOnly WeekStart { get; set; }

        public DateOnly WeekEnd => WeekStart.AddDays(6);

        public DateOnly? PreviousWeek =>
            WeekStart.DayNumber >= 7
                ? WeekStart.AddDays(-7)
                : null;

        public DateOnly? NextWeek =>
            WeekStart.DayNumber <= DateOnly.MaxValue.DayNumber - 13
                ? WeekStart.AddDays(7)
                : null;

        public List<MealPlanEntryViewModel> Items { get; set; }
            = new();
    }

    public class MealPlanEntryViewModel
    {
        public int Id { get; set; }

        public int RecipeId { get; set; }

        public string RecipeTitle { get; set; } = string.Empty;

        public DateOnly Date { get; set; }

        public MealType MealType { get; set; }

        public int Servings { get; set; }

        public bool CanViewRecipe { get; set; }

        public bool IsArchived { get; set; }
    }
}