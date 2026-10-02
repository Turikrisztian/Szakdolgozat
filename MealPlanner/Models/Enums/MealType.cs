using System.ComponentModel.DataAnnotations;

namespace MealPlanner.Models.Enums
{
    public enum MealType
    {
        [Display(Name = "Reggeli")]
        Breakfast = 1,

        [Display(Name = "Ebéd")]
        Lunch = 2,

        [Display(Name = "Vacsora")]
        Dinner = 3,

        // A korábbi adatbázisban a 4-es érték is megengedett.
        [Display(Name = "Egyéb étkezés")]
        Other = 4
    }
}
