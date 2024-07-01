using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using MovieOpinions.Domain.ViewModels.LoginModel;
using MovieOpinions.Service.Interfaces;
using System.Security.Claims;

namespace MovieOpinions.Controllers
{
    public class LoginPageController : Controller
    {
        private readonly IAccountService _accountService;

        public LoginPageController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet]
        public IActionResult LoginPage()
        {
            LoginModel loginModel = new LoginModel(); 
            return View(loginModel);
        }

        [HttpGet]
        public IActionResult RedirectToRegistrationPage()
        {
            return RedirectToAction("RegistrationPage", "RegistrationPage");
        }

        [HttpPost]
        public async Task<IActionResult> LoginUser([FromBody] LoginModel FormData)
        {
            if(string.IsNullOrWhiteSpace(FormData.LoginUser) || string.IsNullOrWhiteSpace(FormData.PasswordUser))
            {
                return Json(new { description = "Поле логіну або паролю пусте.\nБудь-ласка перевірте інформацію" });
            }

            var Response = await _accountService.Login(FormData);

            if(Response.StatusCode == Domain.Enum.StatusCode.OK)
            {
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(Response.Data));

                return Json(new { RedirectUrl = Url.Action("FilmPage", "FilmPage") });
            }
            else
            {
                return Json(new { Response.Description });
            }
        }
    }
}
