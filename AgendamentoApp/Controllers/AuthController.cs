using AgendamentoApp.Entidade;
using AgendamentoApp.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AgendamentoApp.Controllers;

public class AuthController : Controller
{
    private readonly IUserService _userService;
    private readonly SignInManager<Usuario> _signInManager;

    public AuthController(
        IUserService userService,
        SignInManager<Usuario> signInManager)
    {
        _userService = userService;
        _signInManager = signInManager;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password, string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (ModelState.IsValid)
        {
            var result = await _userService.ValidateUserCredentialsAsync(email, password);
            if (result)
            {
                var user = await _userService.GetUserByEmailAsync(email);
                await _signInManager.SignInAsync(user, isPersistent: false);

                if (returnUrl != null)
                    return LocalRedirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Tentativa de login inválida.");
        }

        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(Usuario user, string password)
    {
        if (ModelState.IsValid)
        {
            user.DataCadastro = DateTime.Now;
            user.Ativo = true;

            var result = await _userService.CreateUserAsync(user, password, "Cliente");
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}
