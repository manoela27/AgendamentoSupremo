using AgendamentoApp.Entidade;
using AgendamentoApp.Infraestrutura;
using AgendamentoApp.Interface;
using AgendamentoApp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AgendamentoApp.Controllers;

[Authorize(Roles = "Cliente")]
public class ClienteController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IAgendamentoService _agendamentoService;
    private readonly UserManager<Usuario> _userManager;
    private readonly ILogger<ClienteController> _logger;

    public ClienteController(
        ApplicationDbContext context,
        IAgendamentoService agendamentoService,
        UserManager<Usuario> userManager,
        ILogger<ClienteController> logger)  // Inject logger
    {
        _context = context;
        _agendamentoService = agendamentoService;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<IActionResult> VisualizarAgendamentos()
    {
        _logger.LogInformation("VisualizarAgendamentos action started for user {UserId}", _userManager.GetUserId(User));
        var userId = _userManager.GetUserId(User);
        var agendamentos = await _agendamentoService.GetAgendamentosByClienteAsync(userId);
        return View(agendamentos);
    }

    public async Task<IActionResult> AlterarDadosPessoais()
    {
        var userId = _userManager.GetUserId(User);
        _logger.LogInformation("AlterarDadosPessoais GET action called by user {UserId}", userId);

        var cliente = await _context.Clientes
            .Where(c => c.Id == userId)
            .SingleOrDefaultAsync(); // <- substituindo FindAsync
        if (cliente == null)
        {
            _logger.LogWarning("Cliente not found for user {UserId} in AlterarDadosPessoais GET", userId);
            return NotFound();
        }
        return View(cliente);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlterarDadosPessoais(Cliente cliente)
    {
        var userId = _userManager.GetUserId(User);
        _logger.LogInformation("AlterarDadosPessoais POST action called by user {UserId}", userId);

        if (userId != cliente.Id)
        {
            _logger.LogWarning("UserId {UserId} does not match Cliente.Id {ClienteId} in AlterarDadosPessoais POST", userId, cliente.Id);
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState is invalid in AlterarDadosPessoais POST for user {UserId}: {ModelStateErrors}", userId, ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return View(cliente);
        }

        try
        {
            
            var clienteDb = await _context.Clientes.FindAsync(userId);
            if (clienteDb == null)
            {
                _logger.LogWarning("Cliente not found for user {UserId} in AlterarDadosPessoais POST", userId);
                return NotFound();
            }

            
            clienteDb.Nome = cliente.Nome;
            clienteDb.Email = cliente.Email;
            clienteDb.Telefone = cliente.Telefone;
            clienteDb.Endereco = cliente.Endereco;
            

            await _context.SaveChangesAsync();

            _logger.LogInformation("Cliente data updated successfully for user {UserId}", userId);
            TempData["StatusMessage"] = "Seus dados foram atualizados com sucesso.";
            return RedirectToAction(nameof(AlterarDadosPessoais));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency error when updating Cliente data for user {UserId}", userId);
            if (!await ClienteExists(cliente.Id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error when updating Cliente data for user {UserId}", userId);
            throw;
        }
    }


    public async Task<IActionResult> NovoAgendamento()
    {
        ViewBag.Servicos = await _context.Servicos.Where(s => s.Ativo).ToListAsync();
        ViewBag.Funcionarios = await _context.Funcionarios
            .Include(f => f.Cargo)
            .Where(f => f.Ativo)
            .ToListAsync();
        return View(new AgendamentoViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NovoAgendamento(AgendamentoViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Servicos = await _context.Servicos.Where(s => s.Ativo).ToListAsync();
            ViewBag.Funcionarios = await _context.Funcionarios
                .Include(f => f.Cargo)
                .Where(f => f.Ativo)
                .ToListAsync();
            return View(model);
        }

        var userId = _userManager.GetUserId(User);
        var cliente = await _context.Clientes.FindAsync(userId);
        if (cliente == null)
        {
            ModelState.AddModelError("", "Cliente não encontrado");
            return View(model);
        }

        var servico = await _context.Servicos.FindAsync(model.ServicoId);
        var funcionario = await _context.Funcionarios.FindAsync(model.FuncionarioId);

        if (servico == null)
        {
            ModelState.AddModelError("ServicoId", "Serviço não encontrado");
            return View(model);
        }

        if (funcionario == null)
        {
            ModelState.AddModelError("FuncionarioId", "Funcionário não encontrado");
            return View(model);
        }

        try
        {
            var agendamento = model.ToEntity(userId, cliente, funcionario, servico);
            await _agendamentoService.CreateAgendamentoAsync(agendamento);
            TempData["StatusMessage"] = "Agendamento realizado com sucesso!";
            return RedirectToAction(nameof(VisualizarAgendamentos));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.Servicos = await _context.Servicos.Where(s => s.Ativo).ToListAsync();
            ViewBag.Funcionarios = await _context.Funcionarios
                .Include(f => f.Cargo)
                .Where(f => f.Ativo)
                .ToListAsync();
            return View(model);
        }
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExcluirConta()
    {
        var userId = _userManager.GetUserId(User);
        var usuario = await _userManager.FindByIdAsync(userId);

        if (usuario == null)
        {
            _logger.LogWarning("Usuário não encontrado para exclusão: {UserId}", userId);
            return NotFound();
        }


        var cliente = await _context.Clientes.FindAsync(userId);
        if (cliente != null)
        {
            _context.Clientes.Remove(cliente);
        }

        var result = await _userManager.DeleteAsync(usuario);

        if (result.Succeeded)
        {
            await HttpContext.SignOutAsync();
            TempData["ContaExcluida"] = "true";
            return RedirectToAction("Index", "Home");
        }
        else
        {
            _logger.LogError("Erro ao excluir conta do usuário {UserId}: {Errors}", userId, string.Join(", ", result.Errors.Select(e => e.Description)));
            TempData["ErrorMessage"] = "Erro ao excluir sua conta.";
            return RedirectToAction("MeuPerfil", "Cliente");
        }
    }
    [HttpGet]
    public async Task<IActionResult> GetFuncionariosPorServico(int servicoId)
    {
        var servico = await _context.Servicos.FindAsync(servicoId);
        if (servico == null)
        {
            return NotFound();
        }

        var funcionarios = await _context.Funcionarios
            .Where(f => f.CargoId == servico.CargoId && f.Ativo)
            .Select(f => new {
                f.Id,
                f.Nome
            })
            .ToListAsync();

        return Json(funcionarios);
    }


}

