using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AgendamentoApp.Infraestrutura;
using Microsoft.EntityFrameworkCore;

namespace AgendamentoApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        if (User.Identity.IsAuthenticated)
        {
            if (User.IsInRole("Admin"))
            {
                ViewBag.TotalClientes = await _context.Clientes.CountAsync();
                ViewBag.TotalFuncionarios = await _context.Funcionarios.CountAsync();
                ViewBag.AgendamentosHoje = await _context.Agendamentos
                    .Where(a => a.DataHora.Date == DateTime.Today)
                    .CountAsync();
            }
            else if (User.IsInRole("Funcionario"))
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                ViewBag.MeusAgendamentosHoje = await _context.Agendamentos
                    .Where(a => a.FuncionarioId == userId && a.DataHora.Date == DateTime.Today)
                    .CountAsync();
            }
            else if (User.IsInRole("Cliente"))
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                ViewBag.MeusProximosAgendamentos = await _context.Agendamentos
                    .Include(a => a.Servico)
                    .Include(a => a.Funcionario)
                    .Where(a => a.ClienteId == userId && a.DataHora >= DateTime.Now)
                    .OrderBy(a => a.DataHora)
                    .Take(5)
                    .ToListAsync();
            }
        }

        ViewBag.ServicosDisponiveis = await _context.Servicos
            .Where(s => s.Ativo)
            .OrderBy(s => s.Nome)
            .ToListAsync();

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
