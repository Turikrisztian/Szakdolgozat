using MealPlanner.Data;
using MealPlanner.Models;
using MealPlanner.Security;
using MealPlanner.ViewModels.Categories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Controllers
{
    [Authorize(Roles = AppRoles.Admin)]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .AsNoTracking()
                .OrderBy(category => category.Name)
                .ToListAsync();

            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CategoryFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CategoryFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var name = model.Name.Trim();

            var nameExists = await _context.Categories
                .AnyAsync(category => category.Name == name);

            if (nameExists)
            {
                ModelState.AddModelError(
                    nameof(model.Name),
                    "Már létezik ilyen nevű kategória.");

                return View(model);
            }

            var category = new Category
            {
                Name = name,
                IsActive = model.IsActive
            };

            _context.Categories.Add(category);

            if (!await TrySaveChangesAsync())
            {
                return View(model);
            }

            TempData["SuccessMessage"] = "A kategória létrejött.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit([FromRoute] int id)
        {
            var category = await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(category => category.Id == id);

            if (category is null)
            {
                return NotFound();
            }

            var model = new CategoryFormViewModel
            {
                Id = category.Id,
                Name = category.Name,
                IsActive = category.IsActive
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            [FromRoute] int id,
            CategoryFormViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            var category = await _context.Categories.FindAsync(id);

            if (category is null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var name = model.Name.Trim();

            var nameExists = await _context.Categories
                .AnyAsync(other =>
                    other.Id != id && other.Name == name);

            if (nameExists)
            {
                ModelState.AddModelError(
                    nameof(model.Name),
                    "Már létezik ilyen nevű kategória.");

                return View(model);
            }

            category.Name = name;
            category.IsActive = model.IsActive;

            if (!await TrySaveChangesAsync())
            {
                return View(model);
            }

            TempData["SuccessMessage"] = "A kategória módosításait mentettük.";

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
                    "A rekord időközben megváltozott vagy megszűnt. "
                    + "Térj vissza a listához, és nyisd meg újra.");

                return false;
            }
            catch (DbUpdateException exception)
                when (exception.InnerException is SqlException sqlException
                    && (sqlException.Number == 2601
                        || sqlException.Number == 2627))
            {
                ModelState.AddModelError(
                    nameof(CategoryFormViewModel.Name),
                    "Már létezik ilyen nevű kategória.");

                return false;
            }
        }
    }
}