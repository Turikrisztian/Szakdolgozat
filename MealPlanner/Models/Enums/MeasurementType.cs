using System.ComponentModel.DataAnnotations;

namespace MealPlanner.Models.Enums
{
    public enum MeasurementType
    {
        [Display(Name = "Tömeg")]
        Mass = 1,

        [Display(Name = "Térfogat")]
        Volume = 2,

        [Display(Name = "Darabszám")]
        Count = 3
    }
}