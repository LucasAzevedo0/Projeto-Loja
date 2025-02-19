using LojaRemastered.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LojaRemastered.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public async Task<IActionResult> LoginAsAdmin()
        {
            var adminUser = await _userManager.FindByEmailAsync("admin@admin.com");

            if (adminUser == null)
            {
                return NotFound("Conta de administrador não encontrada.");
            }

            await _signInManager.SignInAsync(adminUser, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }

    }
}
