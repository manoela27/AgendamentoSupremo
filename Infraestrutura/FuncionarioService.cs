using AgendamentoApp.Entidade;
using AgendamentoApp.Infraestrutura;
using AgendamentoApp.Interface;
using Microsoft.EntityFrameworkCore;

namespace AgendamentoApp.Infraestrutura;

public class FuncionarioService : IFuncionarioService
{
    private readonly ApplicationDbContext _context;

    public FuncionarioService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Funcionario>> GetAllFuncionariosAsync()
    {
        return await _context.Funcionarios
            .Include(f => f.Cargo)
            .OrderBy(f => f.Nome)
            .ToListAsync();
    }

    public async Task<Funcionario> GetFuncionarioByIdAsync(string id)
    {
        return await _context.Funcionarios
            .Include(f => f.Cargo)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<IEnumerable<Funcionario>> GetFuncionariosByCargoAsync(int cargoId)
    {
        return await _context.Funcionarios
            .Include(f => f.Cargo)
            .Where(f => f.CargoId == cargoId)
            .OrderBy(f => f.Nome)
            .ToListAsync();
    }

    public async Task<IEnumerable<Agendamento>> GetAgendamentosFuncionarioAsync(string funcionarioId)
    {
        return await _context.Agendamentos
            .Include(a => a.Cliente)
            .Include(a => a.Servico)
            .Where(a => a.FuncionarioId == funcionarioId)
            .OrderBy(a => a.DataHora)
            .ToListAsync();
    }

    public async Task<bool> IsFuncionarioDisponivelAsync(string funcionarioId, DateTime dataHora)
    {
        var agendamentosNoHorario = await _context.Agendamentos
            .Include(a => a.Servico)
            .Where(a => a.FuncionarioId == funcionarioId &&
                       a.Status != "Cancelado")
            .Where(a => a.DataHora <= dataHora &&
                       a.DataHora.AddMinutes(a.Servico.DuracaoMinutos) > dataHora)
            .AnyAsync();

        return !agendamentosNoHorario;
    }

    public async Task<IEnumerable<DateTime>> GetHorariosDisponiveisAsync(string funcionarioId, DateTime data)
    {
        var horarios = new List<DateTime>();
        var horaInicio = new DateTime(data.Year, data.Month, data.Day, 8, 0, 0);
        var horaFim = new DateTime(data.Year, data.Month, data.Day, 18, 0, 0);

        // Buscar todos os agendamentos do funcionário para o dia
        var agendamentosDoFuncionario = await _context.Agendamentos
            .Include(a => a.Servico)
            .Where(a => a.FuncionarioId == funcionarioId &&
                       a.DataHora.Date == data.Date &&
                       a.Status != "Cancelado")
            .OrderBy(a => a.DataHora)
            .ToListAsync();

        var currentTime = horaInicio;
        while (currentTime < horaFim)
        {
            // Não mostrar horários passados
            if (currentTime > DateTime.Now)
            {
                var horarioOcupado = false;
                foreach (var agendamento in agendamentosDoFuncionario)
                {
                    var fimAgendamento = agendamento.DataHora.AddMinutes(agendamento.Servico.DuracaoMinutos);
                    if (currentTime >= agendamento.DataHora && currentTime < fimAgendamento)
                    {
                        horarioOcupado = true;
                        break;
                    }
                }

                if (!horarioOcupado)
                {
                    horarios.Add(currentTime);
                }
            }
            currentTime = currentTime.AddMinutes(30);
        }

        return horarios;
    }

    public async Task<Funcionario> UpdateFuncionarioAsync(Funcionario funcionario)
    {
        var existingFuncionario = await _context.Funcionarios.FindAsync(funcionario.Id);
        if (existingFuncionario == null)
        {
            throw new InvalidOperationException("Funcionário não encontrado.");
        }

        _context.Entry(existingFuncionario).CurrentValues.SetValues(funcionario);
        await _context.SaveChangesAsync();
        return funcionario;
    }

    public async Task<bool> DeleteFuncionarioAsync(string id)
    {
        var funcionario = await _context.Funcionarios.FindAsync(id);
        if (funcionario == null)
        {
            return false;
        }

        _context.Funcionarios.Remove(funcionario);
        await _context.SaveChangesAsync();
        return true;
    }
}
