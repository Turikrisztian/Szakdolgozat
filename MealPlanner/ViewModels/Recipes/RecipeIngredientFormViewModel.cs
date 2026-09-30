using System.ComponentModel.DataAnnotations;

namespace MealPlanner.ViewModels.Recipes
{
    public class RecipeIngredientFormViewModel
    {
        [Display(Name = "Alapanyag")]
        [Required(ErrorMessage = "Válassz alapanyagot.")]
        [Range(1, int.MaxValue, ErrorMessage = "Érvénytelen alapanyag.")]
        public int? IngredientId { get; set; }

        [Display(Name = "Mennyiség")]
        [Required(ErrorMessage = "Add meg a mennyiséget.")]
        [RegularExpression(
            @"^[0-9]{1,15}([.,][0-9]{1,3})?$",
            ErrorMessage =
                "Legfeljebb 15 egész és 3 tizedesjegyet adj meg, " +
                "ezreselválasztó nélkül.")]
        public string Quantity { get; set; } = string.Empty;

        [Display(Name = "Mértékegység")]
        [Required(ErrorMessage = "Válassz mértékegységet.")]
        [Range(1, int.MaxValue, ErrorMessage = "Érvénytelen mértékegység.")]
        public int? UnitId { get; set; }
    }
}