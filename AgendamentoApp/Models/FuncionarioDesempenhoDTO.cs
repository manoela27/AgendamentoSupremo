namespace AgendamentoApp.Models;

public class FuncionarioDesempenhoDTO
{
    public Entidade.Funcionario Funcionario { get; set; }
    public int TotalAgendamentos { get; set; }
    public int AgendamentosConcluidos { get; set; }
    public decimal Faturamento { get; set; }
}
