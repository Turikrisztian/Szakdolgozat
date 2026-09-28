using MealPlanner.Models.Enums;

namespace MealPlanner.Models
{
    public class Unit
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Symbol { get; set; } = string.Empty;

        public MeasurementType MeasurementType { get; set; }

        public decimal ConversionFactor { get; set; }
    }
}