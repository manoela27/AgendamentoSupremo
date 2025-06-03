using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace AgendamentoApp.Entidade
{
    public class Servico
    {
        public int Id { get; set; }

        public string Nome { get; set; }
        public string Descricao { get; set; }
        public decimal Preco { get; set; }
        public int DuracaoMinutos { get; set; }
        public bool Ativo { get; set; }

        // NOVO: Relacionamento com Cargo
        public int CargoId { get; set; }

        [ValidateNever]
        public Cargo Cargo { get; set; }

        [BindNever]
        [NotMapped]
        [ValidateNever]
        public ICollection<Agendamento> Agendamentos { get; set; }
    }
}
