using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MealPlanner.ViewModels.Pantry
{
    public class PantryEditViewModel
    {
        public int Id { get; set; }

        [BindNever]
        [ValidateNever]
        public string IngredientName { get; set; } = string.Empty;

        [Display(Name = "Mennyiség")]
        [Required(ErrorMessage = "Add meg a mennyiséget.")]
        [RegularExpression(
            @"[0-9]{1,15}([.,][0-9]{1,3})?",
            ErrorMessage =
                "Nemnegatív számot adj meg, legfeljebb három tizedesjeggyel.")]
        public string Quantity { get; set; } = string.Empty;

        [Display(Name = "Mértékegység")]
        [Required(ErrorMessage = "Válassz mértékegységet.")]
        [Range(1, int.MaxValue, ErrorMessage = "Érvénytelen mértékegység.")]
        public int? UnitId { get; set; }

        [BindNever]
        [ValidateNever]
        public List<SelectListItem> UnitOptions { get; set; } = new();
    }
}