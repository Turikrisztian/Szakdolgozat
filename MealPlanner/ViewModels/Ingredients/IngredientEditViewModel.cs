using System.ComponentModel.DataAnnotations;
using MealPlanner.Models.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MealPlanner.ViewModels.Ingredients
{
    public class IngredientEditViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Alapanyag neve")]
        [Required(ErrorMessage = "Az alapanyag nevét kötelező megadni.")]
        [StringLength(
            150,
            ErrorMessage = "Az alapanyag neve legfeljebb 150 karakter lehet.")]
        public string Name { get; set; } = string.Empty;

        [BindNever]
        [Display(Name = "Mértéktípus")]
        public MeasurementType MeasurementType { get; set; }

        [Display(Name = "Aktív alapanyag")]
        public bool IsActive { get; set; } = true;
    }
}