using System.ComponentModel.DataAnnotations;
using MealPlanner.Models.Enums;

namespace MealPlanner.ViewModels.Ingredients
{
    public class IngredientCreateViewModel
    {
        [Display(Name = "Alapanyag neve")]
        [Required(ErrorMessage = "Az alapanyag nevét kötelező megadni.")]
        [StringLength(
            150,
            ErrorMessage = "Az alapanyag neve legfeljebb 150 karakter lehet.")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Mértéktípus")]
        [Required(ErrorMessage = "Válassz mértéktípust.")]
        [EnumDataType(
            typeof(MeasurementType),
            ErrorMessage = "Érvénytelen mértéktípus.")]
        public MeasurementType? MeasurementType { get; set; }

        [Display(Name = "Aktív alapanyag")]
        public bool IsActive { get; set; } = true;
    }
}