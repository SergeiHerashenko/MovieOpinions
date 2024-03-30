using Microsoft.AspNetCore.Mvc;

namespace MovieOpinions.Controllers
{
    public class LoginPageController : Controller
    {
        [HttpGet]
        public IActionResult LoginPage()
        {
            return View();
        }
    }
}
