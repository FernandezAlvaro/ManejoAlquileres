using Microsoft.AspNetCore.Mvc;

namespace ManejoAlquileres.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult AccessDenied(string returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
    }
}