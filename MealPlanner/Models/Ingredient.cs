using MealPlanner.Models.Enums;

namespace MealPlanner.Models
{
    public class Ingredient
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public MeasurementType MeasurementType { get; set; }

        public bool IsActive { get; set; } = true;
    }
}