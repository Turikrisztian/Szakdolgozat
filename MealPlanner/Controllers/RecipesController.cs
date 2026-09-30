using System.Globalization;
using MealPlanner.Data;
using MealPlanner.Models;
using MealPlanner.Security;
using MealPlanner.ViewModels.Recipes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Controllers
{
    [Authorize]
    public class RecipesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public RecipesController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Index(
            [FromQuery] string? search,
            [FromQuery] int? categoryId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            search = search?.Trim();

            var query = _context.Recipes
                .AsNoTracking()
                .Where(recipe =>
                    recipe.IsPublic &&
                    !recipe.IsArchived);

            await LoadRecipeFilterOptionsAsync(query, categoryId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(recipe =>
                    recipe.Title.Contains(search));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(recipe =>
                    recipe.CategoryId == categoryId.Value);
            }

            var recipes = await query
                .Include(recipe => recipe.Category)
                .OrderBy(recipe => recipe.Title)
                .ThenBy(recipe => recipe.Id)
                .ToListAsync();

            var hasFilters =
                !string.IsNullOrWhiteSpace(search) ||
                categoryId.HasValue;

            ViewData["Title"] = "Nyilvános receptek";
            ViewData["Search"] = search;
            ViewData["FilterAction"] = nameof(Index);
            ViewData["EmptyMessage"] = hasFilters
                ? "Nincs a keresési feltételeknek megfelelő recept."
                : "Még nincs megjeleníthető nyilvános recept.";

            return View(recipes);
        }

        [HttpGet]
        public async Task<IActionResult> Mine(
            [FromQuery] string? search,
            [FromQuery] int? categoryId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            search = search?.Trim();

            var query = _context.Recipes
                .AsNoTracking()
                .Where(recipe => recipe.UserId == userId);

            await LoadRecipeFilterOptionsAsync(query, categoryId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(recipe =>
                    recipe.Title.Contains(search));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(recipe =>
                    recipe.CategoryId == categoryId.Value);
            }

            var recipes = await query
                .Include(recipe => recipe.Category)
                .OrderBy(recipe => recipe.IsArchived)
                .ThenBy(recipe => recipe.Title)
                .ThenBy(recipe => recipe.Id)
                .ToListAsync();

            var hasFilters =
                !string.IsNullOrWhiteSpace(search) ||
                categoryId.HasValue;

            ViewData["Title"] = "Saját receptek";
            ViewData["Search"] = search;
            ViewData["FilterAction"] = nameof(Mine);
            ViewData["EmptyMessage"] = hasFilters
                ? "Nincs a keresési feltételeknek megfelelő saját recept."
                : "Még nincs saját recepted.";

            return View("Index", recipes);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Details([FromRoute] int id)
        {
            var userId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole(AppRoles.Admin);

            var recipe = await _context.Recipes
                .AsNoTracking()
                .Where(recipe =>
                    recipe.Id == id &&
                    (
                        (recipe.IsPublic && !recipe.IsArchived) ||
                        (userId != null && recipe.UserId == userId) ||
                        isAdmin
                    ))
                .Include(recipe => recipe.Category)
                .Include(recipe => recipe.RecipeIngredients)
                    .ThenInclude(item => item.Ingredient)
                .Include(recipe => recipe.RecipeIngredients)
                    .ThenInclude(item => item.Unit)
                .FirstOrDefaultAsync();

            if (recipe is null)
            {
                return NotFound();
            }

            return View(recipe);
        }

        [HttpGet]
        public async Task<IActionResult> Edit([FromRoute] int id)
        {
            var userId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole(AppRoles.Admin);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var recipe = await _context.Recipes
                .AsNoTracking()
                .Include(recipe => recipe.RecipeIngredients)
                .FirstOrDefaultAsync(recipe =>
                    recipe.Id == id &&
                    (recipe.UserId == userId || isAdmin));

            if (recipe is null)
            {
                return NotFound();
            }

            var model = new RecipeEditViewModel
            {
                Id = recipe.Id,
                Title = recipe.Title,
                Instructions = recipe.Instructions,
                Servings = recipe.Servings,
                PreparationTimeMinutes = recipe.PreparationTimeMinutes,
                IsPublic = recipe.IsPublic,
                CategoryId = recipe.CategoryId,

                Ingredients = recipe.RecipeIngredients
                    .OrderBy(item => item.Id)
                    .Select(item => new RecipeIngredientFormViewModel
                    {
                        IngredientId = item.IngredientId,
                        UnitId = item.UnitId,
                        Quantity = item.Quantity.ToString(
                            "0.###",
                            CultureInfo.InvariantCulture)
                    })
                    .ToList()
            };

            if (model.Ingredients.Count == 0)
            {
                model.Ingredients.Add(
                    new RecipeIngredientFormViewModel());
            }

            await LoadEditOptionsAsync(model, recipe);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            [FromRoute] int id,
            RecipeEditViewModel model,
            string? operation)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            var userId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole(AppRoles.Admin);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var recipe = await _context.Recipes
                .Include(recipe => recipe.RecipeIngredients)
                .FirstOrDefaultAsync(recipe =>
                    recipe.Id == id &&
                    (recipe.UserId == userId || isAdmin));

            if (recipe is null)
            {
                return NotFound();
            }

            model.Ingredients ??= new List<RecipeIngredientFormViewModel>();

            if (model.Ingredients.Count > 100)
            {
                return BadRequest();
            }

            await LoadEditOptionsAsync(model, recipe);

            // Új hozzávalósor: egyelőre csak az űrlapot változtatjuk.
            if (operation == "add")
            {
                ModelState.Clear();

                if (model.Ingredients.Count >= 100)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Legfeljebb 100 hozzávaló adható meg.");
                }
                else
                {
                    model.Ingredients.Add(
                        new RecipeIngredientFormViewModel());
                }

                return View(model);
            }

            // Hozzávalósor eltávolítása az űrlapról.
            if (operation?.StartsWith(
                "remove:",
                StringComparison.Ordinal) == true)
            {
                if (!int.TryParse(operation["remove:".Length..], out var index) ||
                    index < 0 ||
                    index >= model.Ingredients.Count)
                {
                    return BadRequest();
                }

                model.Ingredients.RemoveAt(index);

                if (model.Ingredients.Count == 0)
                {
                    model.Ingredients.Add(
                        new RecipeIngredientFormViewModel());
                }

                ModelState.Clear();

                return View(model);
            }

            if (operation is not null && operation != "save")
            {
                return BadRequest();
            }

            if (model.Ingredients.Count == 0)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Legalább egy hozzávalót adj meg.");
            }

            if (model.CategoryId.HasValue)
            {
                var categoryAllowed = await _context.Categories
                    .AnyAsync(category =>
                        category.Id == model.CategoryId.Value &&
                        (
                            category.IsActive ||
                            category.Id == recipe.CategoryId
                        ));

                if (!categoryAllowed)
                {
                    ModelState.AddModelError(
                        nameof(model.CategoryId),
                        "Válassz aktív kategóriát, vagy tartsd meg a jelenlegit.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // A mentett recept eredeti alapanyagai.
            var originalIngredientIds = recipe.RecipeIngredients
                .Select(item => item.IngredientId)
                .ToHashSet();

            var ingredientIds = model.Ingredients
                .Select(item => item.IngredientId!.Value)
                .Distinct()
                .ToList();

            var unitIds = model.Ingredients
                .Select(item => item.UnitId!.Value)
                .Distinct()
                .ToList();

            var ingredients = await _context.Ingredients
                .AsNoTracking()
                .Where(ingredient => ingredientIds.Contains(ingredient.Id))
                .ToDictionaryAsync(ingredient => ingredient.Id);

            var units = await _context.Units
                .AsNoTracking()
                .Where(unit => unitIds.Contains(unit.Id))
                .ToDictionaryAsync(unit => unit.Id);

            var usedIngredientIds = new HashSet<int>();
            var quantities = new decimal[model.Ingredients.Count];

            for (var i = 0; i < model.Ingredients.Count; i++)
            {
                var item = model.Ingredients[i];
                var prefix = $"Ingredients[{i}]";

                var ingredientId = item.IngredientId!.Value;
                var unitId = item.UnitId!.Value;

                if (!usedIngredientIds.Add(ingredientId))
                {
                    ModelState.AddModelError(
                        $"{prefix}.IngredientId",
                        "Ugyanaz az alapanyag csak egyszer szerepelhet.");
                }

                ingredients.TryGetValue(ingredientId, out var ingredient);
                units.TryGetValue(unitId, out var unit);

                // Inaktív alapanyag megtartható, ha már a recept része volt.
                if (ingredient is null ||
                    (!ingredient.IsActive &&
                     !originalIngredientIds.Contains(ingredientId)))
                {
                    ModelState.AddModelError(
                        $"{prefix}.IngredientId",
                        "Válassz aktív alapanyagot, vagy tartsd meg a meglévőt.");
                }

                if (unit is null)
                {
                    ModelState.AddModelError(
                        $"{prefix}.UnitId",
                        "Válassz létező mértékegységet.");
                }
                else if (ingredient is not null &&
                         ingredient.MeasurementType != unit.MeasurementType)
                {
                    ModelState.AddModelError(
                        $"{prefix}.UnitId",
                        "Ez a mértékegység nem használható ehhez az alapanyaghoz.");
                }

                var normalizedQuantity = (item.Quantity ?? "")
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
                        $"{prefix}.Quantity",
                        "A mennyiség nullánál nagyobb szám legyen.");
                }
                else if (quantity > 999999999999999.999m)
                {
                    ModelState.AddModelError(
                        $"{prefix}.Quantity",
                        "A megadott mennyiség túl nagy.");
                }
                else if (decimal.Round(quantity, 3) != quantity)
                {
                    ModelState.AddModelError(
                        $"{prefix}.Quantity",
                        "Legfeljebb három tizedesjegyet adj meg.");
                }
                else
                {
                    quantities[i] = quantity;
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            recipe.Title = model.Title.Trim();
            recipe.Instructions = model.Instructions.Trim();
            recipe.Servings = model.Servings!.Value;
            recipe.PreparationTimeMinutes = model.PreparationTimeMinutes!.Value;
            recipe.IsPublic = model.IsPublic;
            recipe.CategoryId = model.CategoryId!.Value;
            recipe.UpdatedAtUtc = DateTimeOffset.UtcNow;

            var existingItems = recipe.RecipeIngredients
                .ToDictionary(item => item.IngredientId);

            // A szerkesztett listából hiányzó hozzávalók törlése.
            var removedItems = recipe.RecipeIngredients
                .Where(item => !usedIngredientIds.Contains(item.IngredientId))
                .ToList();

            foreach (var item in removedItems)
            {
                _context.Remove(item);
            }

            // Meglévő hozzávalók frissítése, új hozzávalók hozzáadása.
            for (var i = 0; i < model.Ingredients.Count; i++)
            {
                var item = model.Ingredients[i];
                var ingredientId = item.IngredientId!.Value;

                if (existingItems.TryGetValue(
                    ingredientId,
                    out var existingItem))
                {
                    existingItem.Quantity = quantities[i];
                    existingItem.UnitId = item.UnitId!.Value;
                }
                else
                {
                    recipe.RecipeIngredients.Add(new RecipeIngredient
                    {
                        IngredientId = ingredientId,
                        Quantity = quantities[i],
                        UnitId = item.UnitId!.Value
                    });
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "A recept mentése nem sikerült. " +
                    "Lehetséges, hogy időközben törölték a receptet " +
                    "vagy valamelyik hozzávalóját. Nyisd meg újra a receptet.");

                return View(model);
            }

            TempData["SuccessMessage"] = "A recept módosítása sikerült.";

            return RedirectToAction(
                nameof(Details),
                new { id = recipe.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Archive([FromRoute] int id)
        {
            return await SetArchiveStateAsync(id, true);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore([FromRoute] int id)
        {
            return await SetArchiveStateAsync(id, false);
        }

        private async Task<IActionResult> SetArchiveStateAsync(
            int id,
            bool isArchived)
        {
            var userId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole(AppRoles.Admin);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var recipe = await _context.Recipes
                .FirstOrDefaultAsync(recipe =>
                    recipe.Id == id &&
                    (recipe.UserId == userId || isAdmin));

            if (recipe is null)
            {
                return NotFound();
            }

            if (recipe.IsArchived != isArchived)
            {
                recipe.IsArchived = isArchived;
                recipe.UpdatedAtUtc = DateTimeOffset.UtcNow;

                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = isArchived
                ? "A recept archiválása sikerült."
                : "A recept visszaállítása sikerült.";

            return RedirectToAction(
                nameof(Details),
                new { id = recipe.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new RecipeCreateViewModel();

            model.Ingredients.Add(new RecipeIngredientFormViewModel());

            await LoadRecipeOptionsAsync(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            RecipeCreateViewModel model,
            string? operation)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            model.Ingredients ??= new List<RecipeIngredientFormViewModel>();

            if (model.Ingredients.Count > 100)
            {
                return BadRequest();
            }

            await LoadRecipeOptionsAsync(model);

            // Az űrlap bővítése még nem ment receptet.
            if (operation == "add")
            {
                ModelState.Clear();

                if (model.Ingredients.Count >= 100)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Legfeljebb 100 hozzávaló adható meg.");
                }
                else
                {
                    model.Ingredients.Add(
                        new RecipeIngredientFormViewModel());
                }

                return View(model);
            }

            // Egy hozzávalósor eltávolítása az űrlapról.
            if (operation?.StartsWith(
                "remove:",
                StringComparison.Ordinal) == true)
            {
                if (!int.TryParse(operation["remove:".Length..], out var index) ||
                    index < 0 ||
                    index >= model.Ingredients.Count)
                {
                    return BadRequest();
                }

                model.Ingredients.RemoveAt(index);

                // Legalább egy kitölthető sor maradjon.
                if (model.Ingredients.Count == 0)
                {
                    model.Ingredients.Add(
                        new RecipeIngredientFormViewModel());
                }

                ModelState.Clear();

                return View(model);
            }

            if (operation is not null && operation != "save")
            {
                return BadRequest();
            }

            if (model.Ingredients.Count == 0)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Legalább egy hozzávalót adj meg.");
            }

            // A kötelező mezők, hosszkorlátok és formátumok ellenőrzése.
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var categoryExists = await _context.Categories
                .AnyAsync(category =>
                    category.Id == model.CategoryId &&
                    category.IsActive);

            if (!categoryExists)
            {
                ModelState.AddModelError(
                    nameof(model.CategoryId),
                    "Válassz létező, aktív kategóriát.");
            }

            var ingredientIds = model.Ingredients
                .Select(item => item.IngredientId!.Value)
                .Distinct()
                .ToList();

            var unitIds = model.Ingredients
                .Select(item => item.UnitId!.Value)
                .Distinct()
                .ToList();

            var ingredients = await _context.Ingredients
                .AsNoTracking()
                .Where(ingredient => ingredientIds.Contains(ingredient.Id))
                .ToDictionaryAsync(ingredient => ingredient.Id);

            var units = await _context.Units
                .AsNoTracking()
                .Where(unit => unitIds.Contains(unit.Id))
                .ToDictionaryAsync(unit => unit.Id);

            var usedIngredientIds = new HashSet<int>();
            var quantities = new decimal[model.Ingredients.Count];

            for (var i = 0; i < model.Ingredients.Count; i++)
            {
                var item = model.Ingredients[i];
                var prefix = $"Ingredients[{i}]";

                var ingredientId = item.IngredientId!.Value;
                var unitId = item.UnitId!.Value;

                if (!usedIngredientIds.Add(ingredientId))
                {
                    ModelState.AddModelError(
                        $"{prefix}.IngredientId",
                        "Ugyanaz az alapanyag csak egyszer szerepelhet.");
                }

                ingredients.TryGetValue(ingredientId, out var ingredient);
                units.TryGetValue(unitId, out var unit);

                if (ingredient is null || !ingredient.IsActive)
                {
                    ModelState.AddModelError(
                        $"{prefix}.IngredientId",
                        "Válassz létező, aktív alapanyagot.");
                }

                if (unit is null)
                {
                    ModelState.AddModelError(
                        $"{prefix}.UnitId",
                        "Válassz létező mértékegységet.");
                }
                else if (ingredient is not null &&
                         ingredient.MeasurementType != unit.MeasurementType)
                {
                    ModelState.AddModelError(
                        $"{prefix}.UnitId",
                        "Ez a mértékegység nem használható ehhez az alapanyaghoz.");
                }

                var normalizedQuantity = item.Quantity.Replace(',', '.');

                if (!decimal.TryParse(
                        normalizedQuantity,
                        NumberStyles.AllowDecimalPoint,
                        CultureInfo.InvariantCulture,
                        out var quantity) ||
                    quantity <= 0)
                {
                    ModelState.AddModelError(
                        $"{prefix}.Quantity",
                        "A mennyiség nullánál nagyobb szám legyen.");
                }
                else
                {
                    quantities[i] = quantity;
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var recipe = new Recipe
            {
                Title = model.Title.Trim(),
                CategoryId = model.CategoryId!.Value,
                Servings = model.Servings!.Value,
                PreparationTimeMinutes = model.PreparationTimeMinutes!.Value,
                Instructions = model.Instructions.Trim(),
                IsPublic = model.IsPublic,
                IsArchived = false,
                UserId = userId,
                CreatedAtUtc = DateTimeOffset.UtcNow
            };

            for (var i = 0; i < model.Ingredients.Count; i++)
            {
                var item = model.Ingredients[i];

                recipe.RecipeIngredients.Add(new RecipeIngredient
                {
                    IngredientId = item.IngredientId!.Value,
                    Quantity = quantities[i],
                    UnitId = item.UnitId!.Value
                });
            }

            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "A recept létrehozása sikerült.";

            return RedirectToAction(
                nameof(Details),
                new { id = recipe.Id });
        }

        private async Task LoadRecipeFilterOptionsAsync(
            IQueryable<Recipe> recipes,
            int? selectedCategoryId)
        {
            var categories = await recipes
                .Select(recipe => new
                {
                    Id = recipe.CategoryId,
                    Name = recipe.Category.Name
                })
                .Distinct()
                .OrderBy(category => category.Name)
                .ToListAsync();

            var options = categories
                .Select(category => new SelectListItem
                {
                    Value = category.Id.ToString(),
                    Text = category.Name,
                    Selected = category.Id == selectedCategoryId
                })
                .ToList();

            // Korábbi keresési URL esetén is látszódjon,
            // ha a kiválasztott kategória már nem elérhető.
            if (selectedCategoryId.HasValue &&
                !categories.Any(category =>
                    category.Id == selectedCategoryId.Value))
            {
                options.Insert(0, new SelectListItem
                {
                    Value = selectedCategoryId.Value.ToString(),
                    Text = "Nem elérhető kategória",
                    Selected = true
                });
            }

            ViewData["CategoryOptions"] = options;
        }

        private async Task LoadRecipeOptionsAsync(
            RecipeCreateViewModel model)
        {
            model.CategoryOptions = await _context.Categories
                .AsNoTracking()
                .Where(category => category.IsActive)
                .OrderBy(category => category.Name)
                .Select(category => new SelectListItem
                {
                    Value = category.Id.ToString(),
                    Text = category.Name
                })
                .ToListAsync();

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

        private async Task LoadEditCategoryOptionsAsync(
            RecipeEditViewModel model,
            int currentCategoryId)
        {
            model.CategoryOptions = await _context.Categories
                .AsNoTracking()
                .Where(category =>
                    category.IsActive ||
                    category.Id == currentCategoryId)
                .OrderBy(category => category.Name)
                .Select(category => new SelectListItem
                {
                    Value = category.Id.ToString(),
                    Text = category.Name +
                        (category.IsActive ? "" : " (inaktív)")
                })
                .ToListAsync();
        }

        private async Task LoadEditOptionsAsync(
            RecipeEditViewModel model,
            Recipe recipe)
        {
            await LoadEditCategoryOptionsAsync(model, recipe.CategoryId);

            var originalIngredientIds = recipe.RecipeIngredients
                .Select(item => item.IngredientId)
                .ToList();

            model.IngredientOptions = await _context.Ingredients
                .AsNoTracking()
                .Where(ingredient =>
                    ingredient.IsActive ||
                    originalIngredientIds.Contains(ingredient.Id))
                .OrderBy(ingredient => ingredient.Name)
                .Select(ingredient => new SelectListItem
                {
                    Value = ingredient.Id.ToString(),
                    Text = ingredient.Name +
                        (ingredient.IsActive ? "" : " (inaktív)")
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
    }
}