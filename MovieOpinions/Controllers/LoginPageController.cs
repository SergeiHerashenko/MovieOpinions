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

        [HttpPost]
        public async Task<IActionResult> LoginUser([FromBody] LoginModel formData)
        {
            if(string.IsNullOrWhiteSpace(formData.LoginUser) || string.IsNullOrWhiteSpace(formData.PasswordUser))
            {
                return Json(new { description = "Поле логіну або паролю пусте.\nБудь-ласка перевірте інформацію" });
            }

            var response = await _accountService.Login(formData);

            if(response.StatusCode == Domain.Enum.StatusCode.OK)
            {
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(response.Data));

                return Json(new { redirectUrl = Url.Action("PrivateOfficesPage", "PrivateOfficesPage") });
            }
            else
            {
                return Json(new { response.Description });
            }
        }
    }
}
