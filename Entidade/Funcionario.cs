using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace AgendamentoApp.Entidade
{
    public class Funcionario : Usuario
    {
        [ValidateNever]

        public Cargo Cargo { get; set; }
        [ValidateNever]

        public int? CargoId { get; set; }

        [ValidateNever]
        public ICollection<Agendamento> Agendamentos { get; set; }
    }
}