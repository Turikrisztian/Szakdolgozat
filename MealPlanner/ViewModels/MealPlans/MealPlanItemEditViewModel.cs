using System.ComponentModel.DataAnnotations;
using MealPlanner.Models.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MealPlanner.ViewModels.MealPlans
{
    public class MealPlanItemEditViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Tervezett adagszám")]
        [Required(ErrorMessage = "Add meg az adagszámot.")]
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Az adagszám pozitív egész szám legyen.")]
        public int? Servings { get; set; }

        [BindNever]
        [ValidateNever]
        public string RecipeTitle { get; set; } = string.Empty;

        [BindNever]
        [ValidateNever]
        public DateOnly Date { get; set; }

        [BindNever]
        [ValidateNever]
        public DateOnly WeekStart { get; set; }

        [BindNever]
        [ValidateNever]
        public MealType MealType { get; set; }
    }
}