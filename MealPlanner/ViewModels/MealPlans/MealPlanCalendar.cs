using System.Globalization;
using MealPlanner.Models.Enums;

namespace MealPlanner.ViewModels.MealPlans
{
    public static class MealPlanCalendar
    {
        public static bool TryGetWeekStart(
            DateOnly date,
            out DateOnly weekStart)
        {
            var daysSinceMonday = ((int)date.DayOfWeek + 6) % 7;

            weekStart = date.AddDays(-daysSinceMonday);

            return weekStart.DayNumber
                <= DateOnly.MaxValue.DayNumber - 6;
        }

        public static string Iso(DateOnly date)
        {
            return date.ToString(
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture);
        }

        public static string MealName(MealType type)
        {
            return type switch
            {
                MealType.Breakfast => "Reggeli",
                MealType.Lunch => "Ebéd",
                MealType.Dinner => "Vacsora",
                MealType.Other => "Egyéb étkezés",
                _ => "Ismeretlen étkezés"
            };
        }
    }
}