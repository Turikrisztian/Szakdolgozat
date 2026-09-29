using System.ComponentModel.DataAnnotations;

namespace MealPlanner.ViewModels.Categories
{
    public class CategoryFormViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Kategória neve")]
        [Required(ErrorMessage = "A kategória nevét kötelező megadni.")]
        [StringLength(
            100,
            ErrorMessage = "A kategória neve legfeljebb 100 karakter lehet.")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Aktív kategória")]
        public bool IsActive { get; set; } = true;
    }
}