using AgendamentoApp.Entidade;
using AgendamentoApp.Infraestrutura;
using AgendamentoApp.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgendamentoApp.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly UserManager<Usuario> _userManager;
    private readonly ApplicationDbContext _context;

    public ProfileController(
        UserManager<Usuario> userManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        Usuario usuario;

        if (User.IsInRole("Funcionario"))
        {
            usuario = await _context.Funcionarios
                .Include(f => f.Cargo)
                .FirstOrDefaultAsync(f => f.Id == userId);
        }
        else
        {
            usuario = await _userManager.FindByIdAsync(userId);
        }

        if (usuario == null)
            return NotFound();

        return View(usuario);
    }
}
