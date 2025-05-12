using AgendamentoApp.Entidade;
using AgendamentoApp.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AgendamentoApp.Controllers;

[Authorize(Roles = "Funcionario")]
public class FuncionarioController : Controller
{
    private readonly IFuncionarioService _funcionarioService;
    private readonly UserManager<Usuario> _userManager;

    public FuncionarioController(
        IFuncionarioService funcionarioService,
        UserManager<Usuario> userManager)
    {
        _funcionarioService = funcionarioService;
        _userManager = userManager;
    }

    public async Task<IActionResult> VisualizarAgendamentos()
    {
        var userId = _userManager.GetUserId(User);
        var agendamentos = await _funcionarioService.GetAgendamentosFuncionarioAsync(userId);
        return View(agendamentos);
    }

    public async Task<IActionResult> AlterarDadosPessoais()
    {
        var userId = _userManager.GetUserId(User);
        var funcionario = await _funcionarioService.GetFuncionarioByIdAsync(userId);
        if (funcionario == null)
        {
            return NotFound();
        }
        return View(funcionario);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlterarDadosPessoais(Funcionario funcionario)
    {
        if (ModelState.IsValid)
        {
            var userId = _userManager.GetUserId(User);
            if (userId != funcionario.Id)
            {
                return NotFound();
            }

            try
            {
                await _funcionarioService.UpdateFuncionarioAsync(funcionario);
                TempData["StatusMessage"] = "Seus dados foram atualizados com sucesso.";
                return RedirectToAction(nameof(AlterarDadosPessoais));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }
        return View(funcionario);
    }

    [HttpGet]
    public async Task<IActionResult> ConsultarHorariosDisponiveis(DateTime? data)
    {
        if (!data.HasValue)
        {
            data = DateTime.Today;
        }

        var userId = _userManager.GetUserId(User);
        var horarios = await _funcionarioService.GetHorariosDisponiveisAsync(userId, data.Value);

        ViewBag.DataSelecionada = data.Value;
        return View(horarios);
    }

    [HttpGet]
    public async Task<JsonResult> GetHorariosDisponiveisJson(DateTime data)
    {
        var userId = _userManager.GetUserId(User);
        var horarios = await _funcionarioService.GetHorariosDisponiveisAsync(userId, data);
        return Json(horarios);
    }

    [HttpGet]
    public async Task<JsonResult> VerificarDisponibilidade(DateTime dataHora)
    {
        var userId = _userManager.GetUserId(User);
        var disponivel = await _funcionarioService.IsFuncionarioDisponivelAsync(userId, dataHora);
        return Json(new { disponivel });
    }
}
