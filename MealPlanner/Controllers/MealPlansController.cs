using System.Data;
using System.Globalization;
using MealPlanner.Data;
using MealPlanner.Models;
using MealPlanner.Models.Enums;
using MealPlanner.ViewModels.MealPlans;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Controllers
{
    [Authorize]
    public class MealPlansController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public MealPlansController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            [FromQuery] DateOnly? weekStart)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            if (!ModelState.IsValid ||
                !MealPlanCalendar.TryGetWeekStart(
                    weekStart ?? DateOnly.FromDateTime(DateTime.Today),
                    out var monday))
            {
                return BadRequest(
                    "Érvénytelen dátum. Válassz egy teljes naptári hetet.");
            }

            var items = await _context.MealPlanItems
                .AsNoTracking()
                .Where(item =>
                    item.MealPlan.UserId == userId &&
                    item.MealPlan.WeekStartDate == monday)
                .OrderBy(item => item.Date)
                .ThenBy(item => item.MealType)
                .ThenBy(item => item.Id)
                .Select(item => new MealPlanEntryViewModel
                {
                    Id = item.Id,
                    RecipeId = item.RecipeId,
                    Date = item.Date,
                    MealType = item.MealType,
                    Servings = item.Servings,

                    CanViewRecipe =
                        item.Recipe.UserId == userId ||
                        (item.Recipe.IsPublic &&
                         !item.Recipe.IsArchived),

                    RecipeTitle =
                        item.Recipe.UserId == userId ||
                        (item.Recipe.IsPublic &&
                         !item.Recipe.IsArchived)
                            ? item.Recipe.Title
                            : "Már nem elérhető recept",

                    IsArchived =
                        item.Recipe.UserId == userId &&
                        item.Recipe.IsArchived
                })
                .ToListAsync();

            var model = new MealPlanWeekViewModel
            {
                WeekStart = monday,
                Items = items
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create(
            [FromQuery] DateOnly? date,
            [FromQuery] MealType? mealType)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var selectedDate =
                date ?? DateOnly.FromDateTime(DateTime.Today);

            if (!ModelState.IsValid ||
                !MealPlanCalendar.TryGetWeekStart(selectedDate, out _) ||
                (mealType.HasValue &&
                 !Enum.IsDefined(mealType.Value)))
            {
                return BadRequest(
                    "Érvénytelen nap vagy étkezéstípus.");
            }

            var model = new MealPlanCreateViewModel
            {
                Date = selectedDate,
                MealType = mealType,
                Servings = 1
            };

            await LoadCreateOptionsAsync(model, userId);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            MealPlanCreateViewModel model)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            if (model.Date.HasValue &&
                !MealPlanCalendar.TryGetWeekStart(
                    model.Date.Value,
                    out _))
            {
                ModelState.AddModelError(
                    nameof(model.Date),
                    "Ehhez a dátumhoz nem tartozik teljes naptári hét.");
            }

            if (!ModelState.IsValid)
            {
                await LoadCreateOptionsAsync(model, userId);

                return View(model);
            }

            var date = model.Date!.Value;
            var mealType = model.MealType!.Value;
            var recipeId = model.RecipeId!.Value;

            MealPlanCalendar.TryGetWeekStart(date, out var monday);

            try
            {
                var strategy =
                    _context.Database.CreateExecutionStrategy();

                var result = await strategy.ExecuteAsync(async () =>
                {
                    _context.ChangeTracker.Clear();

                    // A heti terv és az étkezési tétel együtt mentődik.
                    await using var transaction =
                        await _context.Database.BeginTransactionAsync(
                            IsolationLevel.Serializable);

                    var recipeAllowed = await AvailableRecipes(userId)
                        .AnyAsync(recipe => recipe.Id == recipeId);

                    if (!recipeAllowed)
                    {
                        return CreateResult.RecipeUnavailable;
                    }

                    var plan = await _context.MealPlans
                        .SingleOrDefaultAsync(plan =>
                            plan.UserId == userId &&
                            plan.WeekStartDate == monday);

                    if (plan is not null)
                    {
                        var alreadyExists =
                            await _context.MealPlanItems.AnyAsync(item =>
                                item.MealPlanId == plan.Id &&
                                item.Date == date &&
                                item.MealType == mealType &&
                                item.RecipeId == recipeId);

                        if (alreadyExists)
                        {
                            return CreateResult.Duplicate;
                        }
                    }

                    var now = DateTimeOffset.UtcNow;

                    if (plan is null)
                    {
                        plan = new MealPlan
                        {
                            UserId = userId,
                            WeekStartDate = monday,
                            CreatedAtUtc = now
                        };

                        _context.MealPlans.Add(plan);
                    }

                    plan.Items.Add(new MealPlanItem
                    {
                        Date = date,
                        MealType = mealType,
                        RecipeId = recipeId,
                        Servings = model.Servings!.Value
                    });

                    plan.UpdatedAtUtc = now;

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return CreateResult.Saved;
                });

                if (result == CreateResult.Saved)
                {
                    TempData["SuccessMessage"] =
                        "A recept bekerült a heti tervedbe.";

                    return RedirectToWeek(monday);
                }

                ModelState.AddModelError(
                    nameof(model.RecipeId),
                    result == CreateResult.Duplicate
                        ? "Ez a recept ezen a napon, ennél az étkezésnél " +
                          "már szerepel. Az adagszámát a Szerkesztés " +
                          "gombbal módosíthatod."
                        : "Ez a recept már nem választható. Válassz saját " +
                          "vagy nyilvános, nem archivált receptet.");
            }
            catch (Exception exception) when (
                exception.GetBaseException() is SqlException sql &&
                sql.Number is 1205 or 2601 or 2627)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "A heti tervet közben másik kérés is módosította. " +
                    "Próbáld újra a hozzáadást.");
            }

            _context.ChangeTracker.Clear();

            await LoadCreateOptionsAsync(model, userId);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(
            [FromRoute] int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var item = await OwnedItems(userId)
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == id);

            if (item is null)
            {
                return NotFound();
            }

            return View(ToEditModel(item, userId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            [FromRoute] int id,
            MealPlanItemEditViewModel model)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            if (id != model.Id)
            {
                return BadRequest();
            }

            var item = await OwnedItems(userId)
                .FirstOrDefaultAsync(item => item.Id == id);

            if (item is null)
            {
                return NotFound();
            }

            FillDisplayFields(model, item, userId);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            item.Servings = model.Servings!.Value;
            item.MealPlan.UpdatedAtUtc = DateTimeOffset.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "A tételt időközben módosították vagy törölték. " +
                    "Nyisd meg újra a heti tervet.");

                return View(model);
            }

            TempData["SuccessMessage"] =
                "A tervezett adagszám módosítása sikerült.";

            return RedirectToWeek(item.MealPlan.WeekStartDate);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(
            [FromRoute] int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var item = await OwnedItems(userId)
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == id);

            if (item is null)
            {
                return NotFound();
            }

            return View(ToEditModel(item, userId));
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            [FromRoute] int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var item = await OwnedItems(userId)
                .FirstOrDefaultAsync(item => item.Id == id);

            if (item is null)
            {
                return NotFound();
            }

            var weekStart = item.MealPlan.WeekStartDate;

            item.MealPlan.UpdatedAtUtc = DateTimeOffset.UtcNow;

            _context.MealPlanItems.Remove(item);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["ErrorMessage"] =
                    "A tételt időközben módosították vagy törölték. " +
                    "Ellenőrizd a heti tervet.";

                return RedirectToWeek(weekStart);
            }

            TempData["SuccessMessage"] =
                "A tételt eltávolítottuk a heti tervből.";

            return RedirectToWeek(weekStart);
        }

        private IQueryable<Recipe> AvailableRecipes(string userId)
        {
            return _context.Recipes
                .AsNoTracking()
                .Where(recipe =>
                    !recipe.IsArchived &&
                    (recipe.UserId == userId || recipe.IsPublic));
        }

        private IQueryable<MealPlanItem> OwnedItems(string userId)
        {
            return _context.MealPlanItems
                .Include(item => item.MealPlan)
                .Include(item => item.Recipe)
                .Where(item => item.MealPlan.UserId == userId);
        }

        private async Task LoadCreateOptionsAsync(
            MealPlanCreateViewModel model,
            string userId)
        {
            var recipes = await AvailableRecipes(userId)
                .OrderBy(recipe => recipe.Title)
                .ThenBy(recipe => recipe.Id)
                .Select(recipe => new
                {
                    recipe.Id,
                    recipe.Title,
                    recipe.Servings
                })
                .ToListAsync();

            model.RecipeOptions = recipes
                .Select(recipe => new SelectListItem
                {
                    Value = recipe.Id.ToString(
                        CultureInfo.InvariantCulture),

                    Text = $"{recipe.Title} " +
                           $"(alaprecept: {recipe.Servings} adag)"
                })
                .ToList();

            if (!MealPlanCalendar.TryGetWeekStart(
                model.Date ?? DateOnly.FromDateTime(DateTime.Today),
                out var week))
            {
                MealPlanCalendar.TryGetWeekStart(
                    DateOnly.FromDateTime(DateTime.Today),
                    out week);
            }

            model.WeekStart = week;
        }

        private RedirectToActionResult RedirectToWeek(
            DateOnly weekStart)
        {
            return RedirectToAction(
                nameof(Index),
                new
                {
                    weekStart = MealPlanCalendar.Iso(weekStart)
                });
        }

        private static MealPlanItemEditViewModel ToEditModel(
            MealPlanItem item,
            string userId)
        {
            var model = new MealPlanItemEditViewModel
            {
                Id = item.Id,
                Servings = item.Servings
            };

            FillDisplayFields(model, item, userId);

            return model;
        }

        private static void FillDisplayFields(
            MealPlanItemEditViewModel model,
            MealPlanItem item,
            string userId)
        {
            model.Date = item.Date;
            model.WeekStart = item.MealPlan.WeekStartDate;
            model.MealType = item.MealType;

            model.RecipeTitle =
                item.Recipe.UserId == userId ||
                (item.Recipe.IsPublic && !item.Recipe.IsArchived)
                    ? item.Recipe.Title
                    : "Már nem elérhető recept";
        }

        private enum CreateResult
        {
            Saved,
            Duplicate,
            RecipeUnavailable
        }
    }
}