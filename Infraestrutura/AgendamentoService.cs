using AgendamentoApp.Entidade;
using AgendamentoApp.Interface;
using Microsoft.EntityFrameworkCore;

namespace AgendamentoApp.Infraestrutura;

public class AgendamentoService : IAgendamentoService
{
    private readonly ApplicationDbContext _context;

    public AgendamentoService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Agendamento> CreateAgendamentoAsync(Agendamento agendamento)
    {
        if (!await IsHorarioDisponivelAsync(agendamento.DataHora, agendamento.FuncionarioId))
        {
            throw new InvalidOperationException("Horário não disponível para este funcionário.");
        }

        _context.Agendamentos.Add(agendamento);
        await _context.SaveChangesAsync();
        return agendamento;
    }

    public async Task<Agendamento> GetAgendamentoByIdAsync(int id)
    {
        return await _context.Agendamentos
            .Include(a => a.Cliente)
            .Include(a => a.Funcionario)
            .Include(a => a.Servico)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Agendamento>> GetAgendamentosByClienteAsync(string clienteId)
    {
        return await _context.Agendamentos
            .Include(a => a.Funcionario)
            .Include(a => a.Servico)
            .Where(a => a.ClienteId == clienteId)
            .OrderBy(a => a.DataHora)
            .ToListAsync();
    }

    public async Task<IEnumerable<Agendamento>> GetAgendamentosByFuncionarioAsync(string funcionarioId)
    {
        return await _context.Agendamentos
            .Include(a => a.Cliente)
            .Include(a => a.Servico)
            .Where(a => a.FuncionarioId == funcionarioId)
            .OrderBy(a => a.DataHora)
            .ToListAsync();
    }

    public async Task<IEnumerable<Agendamento>> GetAgendamentosByDateAsync(DateTime date)
    {
        return await _context.Agendamentos
            .Include(a => a.Cliente)
            .Include(a => a.Funcionario)
            .Include(a => a.Servico)
            .Where(a => a.DataHora.Date == date.Date)
            .OrderBy(a => a.DataHora)
            .ToListAsync();
    }

    public async Task<IEnumerable<Agendamento>> GetAllAgendamentosAsync()
    {
        return await _context.Agendamentos
            .Include(a => a.Cliente)
            .Include(a => a.Funcionario)
            .Include(a => a.Servico)
            .OrderBy(a => a.DataHora)
            .ToListAsync();
    }

    public async Task<Agendamento> UpdateAgendamentoAsync(Agendamento agendamento)
    {
        var existingAgendamento = await _context.Agendamentos.FindAsync(agendamento.Id);
        if (existingAgendamento == null)
        {
            throw new InvalidOperationException("Agendamento não encontrado.");
        }

        if (existingAgendamento.DataHora != agendamento.DataHora ||
            existingAgendamento.FuncionarioId != agendamento.FuncionarioId)
        {
            if (!await IsHorarioDisponivelAsync(agendamento.DataHora, agendamento.FuncionarioId))
            {
                throw new InvalidOperationException("Horário não disponível para este funcionário.");
            }
        }

        _context.Entry(existingAgendamento).CurrentValues.SetValues(agendamento);
        await _context.SaveChangesAsync();
        return agendamento;
    }

    public async Task<bool> DeleteAgendamentoAsync(int id)
    {
        var agendamento = await _context.Agendamentos.FindAsync(id);
        if (agendamento == null)
        {
            return false;
        }

        _context.Agendamentos.Remove(agendamento);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsHorarioDisponivelAsync(DateTime dataHora, string funcionarioId)
    {
        var agendamentosNoHorario = await _context.Agendamentos
            .Include(a => a.Servico)
            .Where(a => a.FuncionarioId == funcionarioId)
            .Where(a => a.DataHora <= dataHora &&
                       a.DataHora.AddMinutes(a.Servico.DuracaoMinutos) > dataHora)
            .AnyAsync();

        return !agendamentosNoHorario;
    }
}
