using AgendamentoApp.Entidade;

namespace AgendamentoApp.Entidade;

public class Agendamento
{
    public int Id { get; set; }
    public DateTime DataHora { get; set; }
    public string Status { get; set; }
    public string Observacoes { get; set; }

    public string ClienteId { get; set; }
    public Cliente Cliente { get; set; }

    public string FuncionarioId { get; set; }
    public Funcionario Funcionario { get; set; }

    public int ServicoId { get; set; }
    public Servico Servico { get; set; }
}
