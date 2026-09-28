using MealPlanner.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MealPlanner.Controllers
{
    [Authorize(Roles = AppRoles.Admin)]
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}