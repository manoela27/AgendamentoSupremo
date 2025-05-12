using AgendamentoApp.Entidade;
using AgendamentoApp.Infraestrutura;
using AgendamentoApp.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgendamentoApp.Controllers;

[Authorize(Roles = "Cliente")]
public class ClienteController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IAgendamentoService _agendamentoService;
    private readonly UserManager<Usuario> _userManager;

    public ClienteController(
        ApplicationDbContext context,
        IAgendamentoService agendamentoService,
        UserManager<Usuario> userManager)
    {
        _context = context;
        _agendamentoService = agendamentoService;
        _userManager = userManager;
    }

    public async Task<IActionResult> VisualizarAgendamentos()
    {
        var userId = _userManager.GetUserId(User);
        var agendamentos = await _agendamentoService.GetAgendamentosByClienteAsync(userId);
        return View(agendamentos);
    }

    public async Task<IActionResult> AlterarDadosPessoais()
    {
        var userId = _userManager.GetUserId(User);
        var cliente = await _context.Clientes.FindAsync(userId);
        if (cliente == null)
        {
            return NotFound();
        }
        return View(cliente);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlterarDadosPessoais(Cliente cliente)
    {
        if (ModelState.IsValid)
        {
            var userId = _userManager.GetUserId(User);
            if (userId != cliente.Id)
            {
                return NotFound();
            }

            try
            {
                _context.Update(cliente);
                await _context.SaveChangesAsync();
                TempData["StatusMessage"] = "Seus dados foram atualizados com sucesso.";
                return RedirectToAction(nameof(AlterarDadosPessoais));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ClienteExists(cliente.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }
        return View(cliente);
    }

    public async Task<IActionResult> NovoAgendamento()
    {
        ViewBag.Servicos = await _context.Servicos.Where(s => s.Ativo).ToListAsync();
        ViewBag.Funcionarios = await _context.Funcionarios
            .Include(f => f.Cargo)
            .Where(f => f.Ativo)
            .ToListAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NovoAgendamento(Agendamento agendamento)
    {
        var userId = _userManager.GetUserId(User);
        agendamento.ClienteId = userId;
        agendamento.Status = "Agendado";

        if (ModelState.IsValid)
        {
            try
            {
                await _agendamentoService.CreateAgendamentoAsync(agendamento);
                TempData["StatusMessage"] = "Agendamento realizado com sucesso!";
                return RedirectToAction(nameof(VisualizarAgendamentos));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }

        ViewBag.Servicos = await _context.Servicos.Where(s => s.Ativo).ToListAsync();
        ViewBag.Funcionarios = await _context.Funcionarios
            .Include(f => f.Cargo)
            .Where(f => f.Ativo)
            .ToListAsync();
        return View(agendamento);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelarAgendamento(int id)
    {
        var userId = _userManager.GetUserId(User);
        var agendamento = await _context.Agendamentos
            .FirstOrDefaultAsync(a => a.Id == id && a.ClienteId == userId);

        if (agendamento == null)
        {
            return NotFound();
        }

        if (agendamento.DataHora <= DateTime.Now.AddHours(24))
        {
            TempData["ErrorMessage"] = "Não é possível cancelar agendamentos com menos de 24 horas de antecedência.";
            return RedirectToAction(nameof(VisualizarAgendamentos));
        }

        agendamento.Status = "Cancelado";
        await _context.SaveChangesAsync();
        TempData["StatusMessage"] = "Agendamento cancelado com sucesso.";

        return RedirectToAction(nameof(VisualizarAgendamentos));
    }

    private async Task<bool> ClienteExists(string id)
    {
        return await _context.Clientes.AnyAsync(c => c.Id == id);
    }
}
