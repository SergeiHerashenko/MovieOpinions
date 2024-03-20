using Microsoft.AspNetCore.Mvc;
using MovieOpinions.Domain.ViewModels.HomePageModel;

namespace MovieOpinions.Controllers
{
    public class HomePageController : Controller
    {
        public IActionResult HomePage()
        {
            HomePageModel model = new HomePageModel();
            return View(model);
        }
    }
}
