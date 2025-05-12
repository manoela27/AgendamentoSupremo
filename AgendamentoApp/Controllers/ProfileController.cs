using AgendamentoApp.Entidade;
using AgendamentoApp.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AgendamentoApp.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly UserManager<Usuario> _userManager;
    private readonly IUserService _userService;

    public ProfileController(
        UserManager<Usuario> userManager,
        IUserService userService)
    {
        _userManager = userManager;
        _userService = userService;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }
}
