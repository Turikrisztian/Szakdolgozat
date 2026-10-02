using System.ComponentModel.DataAnnotations;
using MealPlanner.Models.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MealPlanner.ViewModels.MealPlans
{
    public class MealPlanCreateViewModel
    {
        [Display(Name = "Nap")]
        [Required(ErrorMessage = "Válassz napot.")]
        [DataType(DataType.Date)]
        public DateOnly? Date { get; set; }

        [Display(Name = "Étkezés")]
        [Required(ErrorMessage = "Válassz étkezést.")]
        [EnumDataType(
            typeof(MealType),
            ErrorMessage = "Érvénytelen étkezéstípus.")]
        public MealType? MealType { get; set; }

        [Display(Name = "Recept")]
        [Required(ErrorMessage = "Válassz receptet.")]
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Érvénytelen recept.")]
        public int? RecipeId { get; set; }

        [Display(Name = "Tervezett adagszám")]
        [Required(ErrorMessage = "Add meg az adagszámot.")]
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Az adagszám pozitív egész szám legyen.")]
        public int? Servings { get; set; } = 1;

        [BindNever]
        [ValidateNever]
        public List<SelectListItem> RecipeOptions { get; set; }
            = new();

        [BindNever]
        [ValidateNever]
        public DateOnly WeekStart { get; set; }
    }
}