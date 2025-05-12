using AgendamentoApp.Entidade;
using AgendamentoApp.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AgendamentoApp.Infraestrutura;
using Microsoft.EntityFrameworkCore;

namespace AgendamentoApp.Controllers;

[Authorize]
public class AgendamentoController : Controller
{
    private readonly IAgendamentoService _agendamentoService;
    private readonly IFuncionarioService _funcionarioService;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<Usuario> _userManager;

    public AgendamentoController(
        IAgendamentoService agendamentoService,
        IFuncionarioService funcionarioService,
        ApplicationDbContext context,
        UserManager<Usuario> userManager)
    {
        _agendamentoService = agendamentoService;
        _funcionarioService = funcionarioService;
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> CadastrarAgendamento()
    {
        ViewBag.Servicos = await _context.Servicos.Where(s => s.Ativo).ToListAsync();
        ViewBag.Funcionarios = await _context.Funcionarios
            .Include(f => f.Cargo)
            .Where(f => f.Ativo)
            .ToListAsync();
        ViewBag.Clientes = await _context.Clientes.Where(c => c.Ativo).ToListAsync();

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CadastrarAgendamento(Agendamento agendamento)
    {
        if (ModelState.IsValid)
        {
            try
            {
                agendamento.Status = "Agendado";
                await _agendamentoService.CreateAgendamentoAsync(agendamento);
                TempData["StatusMessage"] = "Agendamento realizado com sucesso!";
                return RedirectToAction(nameof(ListarAgendamentos));
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
        ViewBag.Clientes = await _context.Clientes.Where(c => c.Ativo).ToListAsync();

        return View(agendamento);
    }

    public async Task<IActionResult> ListarAgendamentos(DateTime? data)
    {
        if (!data.HasValue)
        {
            data = DateTime.Today;
        }

        var agendamentos = await _agendamentoService.GetAgendamentosByDateAsync(data.Value);
        ViewBag.DataSelecionada = data.Value;
        return View(agendamentos);
    }

    [HttpGet]
    public async Task<IActionResult> EditarAgendamento(int id)
    {
        var agendamento = await _agendamentoService.GetAgendamentoByIdAsync(id);
        if (agendamento == null)
        {
            return NotFound();
        }

        ViewBag.Servicos = await _context.Servicos.Where(s => s.Ativo).ToListAsync();
        ViewBag.Funcionarios = await _context.Funcionarios
            .Include(f => f.Cargo)
            .Where(f => f.Ativo)
            .ToListAsync();
        ViewBag.Clientes = await _context.Clientes.Where(c => c.Ativo).ToListAsync();

        return View(agendamento);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarAgendamento(int id, Agendamento agendamento)
    {
        if (id != agendamento.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                await _agendamentoService.UpdateAgendamentoAsync(agendamento);
                TempData["StatusMessage"] = "Agendamento atualizado com sucesso!";
                return RedirectToAction(nameof(ListarAgendamentos));
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
        ViewBag.Clientes = await _context.Clientes.Where(c => c.Ativo).ToListAsync();

        return View(agendamento);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelarAgendamento(int id)
    {
        var agendamento = await _agendamentoService.GetAgendamentoByIdAsync(id);
        if (agendamento == null)
        {
            return NotFound();
        }

        agendamento.Status = "Cancelado";
        await _agendamentoService.UpdateAgendamentoAsync(agendamento);
        TempData["StatusMessage"] = "Agendamento cancelado com sucesso.";

        return RedirectToAction(nameof(ListarAgendamentos));
    }

    [HttpGet]
    public async Task<JsonResult> VerificarDisponibilidade(string funcionarioId, DateTime dataHora)
    {
        var disponivel = await _funcionarioService.IsFuncionarioDisponivelAsync(funcionarioId, dataHora);
        return Json(new { disponivel });
    }

    [HttpGet]
    public async Task<JsonResult> GetHorariosDisponiveis(string funcionarioId, DateTime data)
    {
        var horarios = await _funcionarioService.GetHorariosDisponiveisAsync(funcionarioId, data);
        return Json(horarios);
    }
}
