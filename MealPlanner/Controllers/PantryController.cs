using System.Globalization;
using MealPlanner.Data;
using MealPlanner.Models;
using MealPlanner.ViewModels.Pantry;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Controllers
{
    [Authorize]
    public class PantryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PantryController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var items = await _context.PantryItems
                .AsNoTracking()
                .Where(item => item.UserId == userId)
                .Include(item => item.Ingredient)
                .Include(item => item.Unit)
                .OrderBy(item => item.Ingredient.Name)
                .ThenBy(item => item.Id)
                .ToListAsync();

            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new PantryCreateViewModel();

            await LoadOptionsAsync(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            PantryCreateViewModel model)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            await LoadOptionsAsync(model);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var ingredientId = model.IngredientId!.Value;
            var unitId = model.UnitId!.Value;

            var ingredient = await _context.Ingredients
                .AsNoTracking()
                .FirstOrDefaultAsync(ingredient =>
                    ingredient.Id == ingredientId &&
                    ingredient.IsActive);

            if (ingredient is null)
            {
                ModelState.AddModelError(
                    nameof(model.IngredientId),
                    "Válassz létező, aktív alapanyagot.");
            }

            var unit = await _context.Units
                .AsNoTracking()
                .FirstOrDefaultAsync(unit => unit.Id == unitId);

            if (unit is null)
            {
                ModelState.AddModelError(
                    nameof(model.UnitId),
                    "Válassz létező mértékegységet.");
            }
            else if (ingredient is not null &&
                     ingredient.MeasurementType != unit.MeasurementType)
            {
                ModelState.AddModelError(
                    nameof(model.UnitId),
                    "Ez a mértékegység nem használható ehhez az alapanyaghoz.");
            }

            var alreadyExists = await _context.PantryItems
                .AnyAsync(item =>
                    item.UserId == userId &&
                    item.IngredientId == ingredientId);

            if (alreadyExists)
            {
                ModelState.AddModelError(
                    nameof(model.IngredientId),
                    "Ez az alapanyag már szerepel a készletedben.");
            }

            var normalizedQuantity = model.Quantity
                .Trim()
                .Replace(',', '.');

            if (!decimal.TryParse(
                    normalizedQuantity,
                    NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture,
                    out var quantity) ||
                quantity <= 0)
            {
                ModelState.AddModelError(
                    nameof(model.Quantity),
                    "A mennyiség nullánál nagyobb szám legyen.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var pantryItem = new PantryItem
            {
                UserId = userId,
                IngredientId = ingredientId,
                Quantity = quantity,
                UnitId = unitId,
                UpdatedAtUtc = DateTimeOffset.UtcNow
            };

            _context.PantryItems.Add(pantryItem);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                var existsNow = await _context.PantryItems
                    .AsNoTracking()
                    .AnyAsync(item =>
                        item.UserId == userId &&
                        item.IngredientId == ingredientId);

                if (!existsNow)
                {
                    throw;
                }

                ModelState.AddModelError(
                    nameof(model.IngredientId),
                    "Ez az alapanyag időközben már bekerült a készletedbe.");

                return View(model);
            }

            TempData["SuccessMessage"] =
                "Az alapanyag bekerült a készletedbe.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit([FromRoute] int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            // Csak a bejelentkezett felhasználó saját tétele kérhető le.
            var pantryItem = await _context.PantryItems
                .AsNoTracking()
                .Include(item => item.Ingredient)
                .FirstOrDefaultAsync(item =>
                    item.Id == id &&
                    item.UserId == userId);

            if (pantryItem is null)
            {
                return NotFound();
            }

            var model = new PantryEditViewModel
            {
                Id = pantryItem.Id,
                Quantity = pantryItem.Quantity.ToString(
                    "0.###",
                    CultureInfo.InvariantCulture),
                UnitId = pantryItem.UnitId
            };

            await LoadEditOptionsAsync(model, pantryItem);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            [FromRoute] int id,
            PantryEditViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var pantryItem = await _context.PantryItems
                .Include(item => item.Ingredient)
                .FirstOrDefaultAsync(item =>
                    item.Id == id &&
                    item.UserId == userId);

            if (pantryItem is null)
            {
                return NotFound();
            }

            // A nevet és a választható egységeket az adatbázisból töltjük.
            await LoadEditOptionsAsync(model, pantryItem);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var unitId = model.UnitId!.Value;

            var unitAllowed = await _context.Units
                .AnyAsync(unit =>
                    unit.Id == unitId &&
                    unit.MeasurementType ==
                        pantryItem.Ingredient.MeasurementType);

            if (!unitAllowed)
            {
                ModelState.AddModelError(
                    nameof(model.UnitId),
                    "Válassz az alapanyaghoz megfelelő mértékegységet.");
            }

            var normalizedQuantity = model.Quantity
                .Trim()
                .Replace(',', '.');

            if (!decimal.TryParse(
                    normalizedQuantity,
                    NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture,
                    out var quantity) ||
                quantity < 0)
            {
                ModelState.AddModelError(
                    nameof(model.Quantity),
                    "A mennyiség nulla vagy annál nagyobb szám legyen.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            pantryItem.Quantity = quantity;
            pantryItem.UnitId = unitId;
            pantryItem.UpdatedAtUtc = DateTimeOffset.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "A készlettétel mentése nem sikerült. " +
                    "Lehetséges, hogy időközben törölték. " +
                    "Nyisd meg újra a készletlistát.");

                return View(model);
            }

            TempData["SuccessMessage"] =
                "A készlettétel módosítása sikerült.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var pantryItem = await _context.PantryItems
                .AsNoTracking()
                .Include(item => item.Ingredient)
                .Include(item => item.Unit)
                .FirstOrDefaultAsync(item =>
                    item.Id == id &&
                    item.UserId == userId);

            if (pantryItem is null)
            {
                return NotFound();
            }

            return View(pantryItem);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed([FromRoute] int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            // A jogosultságot a törlés pillanatában is ellenőrizzük.
            var pantryItem = await _context.PantryItems
                .FirstOrDefaultAsync(item =>
                    item.Id == id &&
                    item.UserId == userId);

            if (pantryItem is null)
            {
                return NotFound();
            }

            _context.PantryItems.Remove(pantryItem);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Ha egy másik böngészőlapon már törölték,
                // a kívánt állapotot elértük.
                var stillExists = await _context.PantryItems
                    .AsNoTracking()
                    .AnyAsync(item =>
                        item.Id == id &&
                        item.UserId == userId);

                if (stillExists)
                {
                    throw;
                }
            }

            TempData["SuccessMessage"] =
                "Az alapanyagot eltávolítottuk a készletedből.";

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadOptionsAsync(
            PantryCreateViewModel model)
        {
            model.IngredientOptions = await _context.Ingredients
                .AsNoTracking()
                .Where(ingredient => ingredient.IsActive)
                .OrderBy(ingredient => ingredient.Name)
                .Select(ingredient => new SelectListItem
                {
                    Value = ingredient.Id.ToString(),
                    Text = ingredient.Name
                })
                .ToListAsync();

            model.UnitOptions = await _context.Units
                .AsNoTracking()
                .OrderBy(unit => unit.MeasurementType)
                .ThenBy(unit => unit.ConversionFactor)
                .Select(unit => new SelectListItem
                {
                    Value = unit.Id.ToString(),
                    Text = unit.Name + " (" + unit.Symbol + ")"
                })
                .ToListAsync();
        }

        private async Task LoadEditOptionsAsync(
            PantryEditViewModel model,
            PantryItem pantryItem)
        {
            model.IngredientName = pantryItem.Ingredient.Name;

            var measurementType = pantryItem.Ingredient.MeasurementType;

            model.UnitOptions = await _context.Units
                .AsNoTracking()
                .Where(unit => unit.MeasurementType == measurementType)
                .OrderBy(unit => unit.ConversionFactor)
                .ThenBy(unit => unit.Name)
                .Select(unit => new SelectListItem
                {
                    Value = unit.Id.ToString(),
                    Text = unit.Name + " (" + unit.Symbol + ")"
                })
                .ToListAsync();
        }
    }
}