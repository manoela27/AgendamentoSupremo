using AgendamentoApp.Entidade;

namespace AgendamentoApp.Interface;

public interface IFuncionarioService
{
    Task<IEnumerable<Funcionario>> GetAllFuncionariosAsync();
    Task<Funcionario> GetFuncionarioByIdAsync(string id);
    Task<IEnumerable<Funcionario>> GetFuncionariosByCargoAsync(int cargoId);
    Task<IEnumerable<Agendamento>> GetAgendamentosFuncionarioAsync(string funcionarioId);
    Task<bool> IsFuncionarioDisponivelAsync(string funcionarioId, DateTime dataHora);
    Task<IEnumerable<DateTime>> GetHorariosDisponiveisAsync(string funcionarioId, DateTime data);
    Task<Funcionario> UpdateFuncionarioAsync(Funcionario funcionario);
    Task<bool> DeleteFuncionarioAsync(string id);
}
