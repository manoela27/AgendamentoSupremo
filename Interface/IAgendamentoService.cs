using AgendamentoApp.Entidade;

namespace AgendamentoApp.Interface;

public interface IAgendamentoService
{
    Task<Agendamento> CreateAgendamentoAsync(Agendamento agendamento);
    Task<Agendamento> GetAgendamentoByIdAsync(int id);
    Task<IEnumerable<Agendamento>> GetAgendamentosByClienteAsync(string clienteId);
    Task<IEnumerable<Agendamento>> GetAgendamentosByFuncionarioAsync(string funcionarioId);
    Task<IEnumerable<Agendamento>> GetAgendamentosByDateAsync(DateTime date);
    Task<IEnumerable<Agendamento>> GetAllAgendamentosAsync();
    Task<Agendamento> UpdateAgendamentoAsync(Agendamento agendamento);
    Task<bool> DeleteAgendamentoAsync(int id);
    Task<bool> IsHorarioDisponivelAsync(DateTime dataHora, string funcionarioId);
}
