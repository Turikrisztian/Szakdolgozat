using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace MealPlanner.ViewModels.Recipes
{
    public class RecipeEditViewModel
    {
        public int Id { get; set; }



        [Display(Name = "Recept neve")]
        [Required(ErrorMessage = "Add meg a recept nevét.")]
        [StringLength(
            200,
            ErrorMessage = "A recept neve legfeljebb 200 karakter lehet.")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Elkészítés")]
        [Required(ErrorMessage = "Írd le az elkészítés lépéseit.")]
        [StringLength(
            10000,
            ErrorMessage = "A leírás legfeljebb 10 000 karakter lehet.")]
        public string Instructions { get; set; } = string.Empty;

        [Display(Name = "Adagszám")]
        [Required(ErrorMessage = "Add meg az adagszámot.")]
        [Range(
    1,
    int.MaxValue,
    ErrorMessage = "Az adagszám pozitív egész szám legyen.")]
        public int? Servings { get; set; }

        [Display(Name = "Elkészítési idő percben")]
        [Required(ErrorMessage = "Add meg az elkészítési időt.")]
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Az elkészítési idő legalább 1 perc legyen.")]
        public int? PreparationTimeMinutes { get; set; }

        [Display(Name = "Nyilvános recept")]
        public bool IsPublic { get; set; }

        [Display(Name = "Kategória")]
        [Required(ErrorMessage = "Válassz kategóriát.")]
        [Range(1, int.MaxValue, ErrorMessage = "Érvénytelen kategória.")]
        public int? CategoryId { get; set; }

        [BindNever]
        [ValidateNever]
        public List<SelectListItem> CategoryOptions { get; set; } = new();

        public List<RecipeIngredientFormViewModel> Ingredients { get; set; } = new();

        [BindNever]
        [ValidateNever]
        public List<SelectListItem> IngredientOptions { get; set; } = new();

        [BindNever]
        [ValidateNever]
        public List<SelectListItem> UnitOptions { get; set; } = new();
    }
}