using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MealPlanner.ViewModels.Recipes
{
    public class RecipeCreateViewModel
    {
        [Display(Name = "Recept neve")]
        [Required(ErrorMessage = "Add meg a recept nevét.")]
        [StringLength(
            200,
            ErrorMessage = "A recept neve legfeljebb 200 karakter lehet.")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Kategória")]
        [Required(ErrorMessage = "Válassz kategóriát.")]
        [Range(1, int.MaxValue, ErrorMessage = "Érvénytelen kategória.")]
        public int? CategoryId { get; set; }

        [Display(Name = "Adagszám")]
        [Required(ErrorMessage = "Add meg az adagszámot.")]
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Az adagszám pozitív egész szám legyen.")]
        public int? Servings { get; set; } = 1;

        [Display(Name = "Elkészítési idő percben")]
        [Required(ErrorMessage = "Add meg az elkészítési időt.")]
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Az elkészítési idő legalább 1 perc legyen.")]
        public int? PreparationTimeMinutes { get; set; }

        [Display(Name = "Elkészítés")]
        [Required(ErrorMessage = "Írd le az elkészítés lépéseit.")]
        [StringLength(
            10000,
            ErrorMessage = "A leírás legfeljebb 10 000 karakter lehet.")]
        public string Instructions { get; set; } = string.Empty;

        [Display(Name = "Nyilvános recept")]
        public bool IsPublic { get; set; }

        [MinLength(1, ErrorMessage = "Legalább egy hozzávaló szükséges.")]
        [MaxLength(100, ErrorMessage = "Legfeljebb 100 hozzávaló adható meg.")]
        public List<RecipeIngredientFormViewModel> Ingredients { get; set; }
            = new();

        [BindNever]
        [ValidateNever]
        public List<SelectListItem> CategoryOptions { get; set; } = new();

        [BindNever]
        [ValidateNever]
        public List<SelectListItem> IngredientOptions { get; set; } = new();

        [BindNever]
        [ValidateNever]
        public List<SelectListItem> UnitOptions { get; set; } = new();
    }
}