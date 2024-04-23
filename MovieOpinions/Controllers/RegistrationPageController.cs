using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using MovieOpinions.Domain.ViewModels.RegisterModel;
using MovieOpinions.Service.Interfaces;
using System.Security.Claims;

namespace MovieOpinions.Controllers
{
    public class RegistrationPageController : Controller
    {
        private readonly IAccountService _accountService;

        public RegistrationPageController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public IActionResult RegistrationPage()
        {
            RegisterModel registerModel = new RegisterModel();
            return View(registerModel);
        }

        [HttpPost]
        public async Task<IActionResult> RegistrationUser([FromBody] RegisterModel formData)
        {
            if (string.IsNullOrWhiteSpace(formData.Login) || string.IsNullOrWhiteSpace(formData.Password) || string.IsNullOrWhiteSpace(formData.PasswordConfirm))
            {
                return Json(new { description = "У вас не заповнені всі поля.\n Будь-ласка перевірте інформацію" });
            }

            if(formData.Password != formData.PasswordConfirm)
            {
                return Json(new { description = "Паролі не співпадають!\n Будь-ласка перевірте інформацію" });
            }

            var response = await _accountService.Register(formData);

            if(response.StatusCode == Domain.Enum.StatusCode.OK)
            {
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(response.Data));

                return Json(new { response.Description, isRedirectNeeded = true });
            }
            else
            {
                return Json(new { response.Description, isRedirectNeeded = false });
            }
        }
    }
}
