using Microsoft.AspNetCore.Mvc;

namespace MovieOpinions.Controllers
{
    public class AuthorizationController : Controller
    {
        [HttpPost("login")]
        public async Task<IActionResult> LoginUser([FromBody] )
        {
            return View();
        }
    }
}
