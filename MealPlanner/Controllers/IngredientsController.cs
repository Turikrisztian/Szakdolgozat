using MealPlanner.Data;
using MealPlanner.Models;
using MealPlanner.Security;
using MealPlanner.ViewModels.Ingredients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Controllers
{
    [Authorize(Roles = AppRoles.Admin)]
    public class IngredientsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public IngredientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var ingredients = await _context.Ingredients
                .AsNoTracking()
                .OrderBy(ingredient => ingredient.Name)
                .ToListAsync();

            return View(ingredients);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new IngredientCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            IngredientCreateViewModel model)
        {
            if (!ModelState.IsValid || model.MeasurementType is null)
            {
                return View(model);
            }

            var name = model.Name.Trim();

            var nameExists = await _context.Ingredients
                .AnyAsync(ingredient => ingredient.Name == name);

            if (nameExists)
            {
                ModelState.AddModelError(
                    nameof(model.Name),
                    "Már létezik ilyen nevű alapanyag.");

                return View(model);
            }

            var ingredient = new Ingredient
            {
                Name = name,
                MeasurementType = model.MeasurementType.Value,
                IsActive = model.IsActive
            };

            _context.Ingredients.Add(ingredient);

            if (!await TrySaveChangesAsync())
            {
                return View(model);
            }

            TempData["SuccessMessage"] =
                "Az alapanyag létrehozása sikerült.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit([FromRoute] int id)
        {
            var ingredient = await _context.Ingredients
                .AsNoTracking()
                .FirstOrDefaultAsync(ingredient => ingredient.Id == id);

            if (ingredient is null)
            {
                return NotFound();
            }

            var model = new IngredientEditViewModel
            {
                Id = ingredient.Id,
                Name = ingredient.Name,
                MeasurementType = ingredient.MeasurementType,
                IsActive = ingredient.IsActive
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            [FromRoute] int id,
            IngredientEditViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            var ingredient = await _context.Ingredients.FindAsync(id);

            if (ingredient is null)
            {
                return NotFound();
            }

            // A megjelenített mértéktípust az adatbázisból vesszük.
            model.MeasurementType = ingredient.MeasurementType;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var name = model.Name.Trim();

            var nameExists = await _context.Ingredients
                .AnyAsync(otherIngredient =>
                    otherIngredient.Id != id &&
                    otherIngredient.Name == name);

            if (nameExists)
            {
                ModelState.AddModelError(
                    nameof(model.Name),
                    "Már létezik ilyen nevű alapanyag.");

                return View(model);
            }

            // Csak ez a két adat szerkeszthető.
            ingredient.Name = name;
            ingredient.IsActive = model.IsActive;

            if (!await TrySaveChangesAsync())
            {
                return View(model);
            }

            TempData["SuccessMessage"] =
                "Az alapanyag módosítása sikerült.";

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> TrySaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "A rekord időközben megváltozott vagy megszűnt. " +
                    "Térj vissza a listához, és nyisd meg újra.");

                return false;
            }
            catch (DbUpdateException exception)
                when (exception.InnerException is SqlException sqlException &&
                      (sqlException.Number == 2601 ||
                       sqlException.Number == 2627))
            {
                ModelState.AddModelError(
                    "Name",
                    "Már létezik ilyen nevű alapanyag.");

                return false;
            }
        }
    }
}