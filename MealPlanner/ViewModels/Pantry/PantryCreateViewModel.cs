using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MealPlanner.ViewModels.Pantry
{
    public class PantryCreateViewModel
    {
        [Display(Name = "Alapanyag")]
        [Required(ErrorMessage = "Válassz alapanyagot.")]
        [Range(1, int.MaxValue, ErrorMessage = "Érvénytelen alapanyag.")]
        public int? IngredientId { get; set; }

        [Display(Name = "Mennyiség")]
        [Required(ErrorMessage = "Add meg a mennyiséget.")]
        [RegularExpression(
            @"[0-9]{1,15}([.,][0-9]{1,3})?",
            ErrorMessage =
                "Számot adj meg, legfeljebb három tizedesjeggyel. Például: 500 vagy 0,5.")]
        public string Quantity { get; set; } = string.Empty;

        [Display(Name = "Mértékegység")]
        [Required(ErrorMessage = "Válassz mértékegységet.")]
        [Range(1, int.MaxValue, ErrorMessage = "Érvénytelen mértékegység.")]
        public int? UnitId { get; set; }

        [BindNever]
        [ValidateNever]
        public List<SelectListItem> IngredientOptions { get; set; } = new();

        [BindNever]
        [ValidateNever]
        public List<SelectListItem> UnitOptions { get; set; } = new();
    }
}