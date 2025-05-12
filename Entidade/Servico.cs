using System.ComponentModel.DataAnnotations.Schema;
using AgendamentoApp.Entidade;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AgendamentoApp.Entidade;

public class Servico
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Descricao { get; set; }
    public decimal Preco { get; set; }
    public int DuracaoMinutos { get; set; }
    public bool Ativo { get; set; }

    [BindNever]
    [NotMapped]
    public ICollection<Agendamento> Agendamentos { get; set; }
}
