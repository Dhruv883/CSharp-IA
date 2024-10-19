using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
