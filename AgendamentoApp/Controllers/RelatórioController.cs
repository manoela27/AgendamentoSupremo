using AgendamentoApp.Entidade;
using AgendamentoApp.Infraestrutura;
using AgendamentoApp.Interface;
using AgendamentoApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgendamentoApp.Controllers;

[Authorize(Roles = "Admin")]
public class RelatorioController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IAgendamentoService _agendamentoService;

    public RelatorioController(
        ApplicationDbContext context,
        IAgendamentoService agendamentoService)
    {
        _context = context;
        _agendamentoService = agendamentoService;
    }

    public async Task<IActionResult> RelatorioAgendamentos(DateTime? dataInicio, DateTime? dataFim)
    {
        if (!dataInicio.HasValue)
            dataInicio = DateTime.Today.AddDays(-30);
        if (!dataFim.HasValue)
            dataFim = DateTime.Today;

        // Carrega todos os agendamentos do período com suas relações
        var agendamentos = await _context.Agendamentos
            .Include(a => a.Cliente)
            .Include(a => a.Funcionario)
            .Include(a => a.Servico)
            .Where(a => a.DataHora.Date >= dataInicio.Value.Date &&
                       a.DataHora.Date <= dataFim.Value.Date)
            .ToListAsync();

        // Realiza todos os cálculos em memória
        var agendamentosOrdenados = agendamentos.OrderBy(a => a.DataHora).ToList();
        var totalAgendamentos = agendamentos.Count;
        var agendamentosConcluidos = agendamentos.Count(a => a.Status == "Concluído");
        var agendamentosCancelados = agendamentos.Count(a => a.Status == "Cancelado");
        var faturamentoTotal = agendamentos
            .Where(a => a.Status == "Concluído")
            .Sum(a => a.Servico.Preco);

        // Agrupa por funcionário em memória
        var agendamentosPorFuncionario = agendamentos
            .GroupBy(a => a.Funcionario)
            .Select(g => new
            {
                Funcionario = g.Key,
                Total = g.Count(),
                Concluidos = g.Count(a => a.Status == "Concluído"),
                Cancelados = g.Count(a => a.Status == "Cancelado"),
                Faturamento = g.Where(a => a.Status == "Concluído")
                              .Sum(a => a.Servico.Preco)
            })
            .OrderByDescending(x => x.Total)
            .ToList();

        // Agrupa por serviço em memória
        var servicosMaisAgendados = agendamentos
            .GroupBy(a => a.Servico)
            .Select(g => new
            {
                Servico = g.Key,
                Total = g.Count(),
                Faturamento = g.Where(a => a.Status == "Concluído")
                              .Sum(a => a.Servico.Preco)
            })
            .OrderByDescending(x => x.Total)
            .ToList();

        ViewBag.DataInicio = dataInicio.Value;
        ViewBag.DataFim = dataFim.Value;
        ViewBag.TotalAgendamentos = totalAgendamentos;
        ViewBag.AgendamentosConcluidos = agendamentosConcluidos;
        ViewBag.AgendamentosCancelados = agendamentosCancelados;
        ViewBag.FaturamentoTotal = faturamentoTotal;
        ViewBag.AgendamentosPorFuncionario = agendamentosPorFuncionario;
        ViewBag.ServicosMaisAgendados = servicosMaisAgendados;

        return View(agendamentosOrdenados);
    }

    [HttpGet]
    public async Task<IActionResult> RelatorioFaturamentoDiario(DateTime? data)
    {
        if (!data.HasValue)
            data = DateTime.Today;

        // Carrega todos os agendamentos do dia
        var agendamentos = await _context.Agendamentos
            .Include(a => a.Cliente)
            .Include(a => a.Funcionario)
            .Include(a => a.Servico)
            .Where(a => a.DataHora.Date == data.Value.Date)
            .ToListAsync();

        // Filtra e calcula em memória
        var agendamentosConcluidos = agendamentos
            .Where(a => a.Status == "Concluído")
            .OrderBy(a => a.DataHora)
            .ToList();

        var faturamentoDiario = agendamentosConcluidos.Sum(a => a.Servico.Preco);

        ViewBag.Data = data.Value;
        ViewBag.FaturamentoDiario = faturamentoDiario;
        ViewBag.TotalAgendamentos = agendamentosConcluidos.Count;

        return View(agendamentosConcluidos);
    }

    [HttpGet]
    public async Task<IActionResult> RelatorioDesempenhoFuncionarios(DateTime? dataInicio, DateTime? dataFim)
    {
        if (!dataInicio.HasValue)
            dataInicio = DateTime.Today.AddDays(-30);
        if (!dataFim.HasValue)
            dataFim = DateTime.Today;

        // Primeiro, carregue todos os agendamentos do período
        var agendamentosPeriodo = await _context.Agendamentos
            .Include(a => a.Funcionario)
                .ThenInclude(f => f.Cargo)
            .Include(a => a.Servico)
            .Where(a => a.DataHora.Date >= dataInicio.Value.Date &&
                       a.DataHora.Date <= dataFim.Value.Date)
            .ToListAsync();

        // Depois, agrupe os agendamentos por funcionário em memória
        var desempenho = agendamentosPeriodo
            .GroupBy(a => a.Funcionario)
            .Select(g => new FuncionarioDesempenhoDTO
            {
                Funcionario = g.Key,
                TotalAgendamentos = g.Count(),
                AgendamentosConcluidos = g.Count(a => a.Status == "Concluído"),
                Faturamento = g.Where(a => a.Status == "Concluído")
                              .Sum(a => a.Servico.Preco)
            })
            .OrderByDescending(d => d.Faturamento)
            .ToList();

        // Carregue todos os funcionários para exibir mesmo os que não têm agendamentos
        var todosFuncionarios = await _context.Funcionarios
            .Include(f => f.Cargo)
            .ToListAsync();

        ViewBag.DataInicio = dataInicio.Value;
        ViewBag.DataFim = dataFim.Value;
        ViewBag.Desempenho = desempenho;

        return View(todosFuncionarios);
    }
}
