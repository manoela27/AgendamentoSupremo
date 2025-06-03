using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace AgendamentoApp.Entidade
{

    public class Cliente : Usuario
    {
        [ValidateNever]
        public ICollection<Agendamento> Agendamentos { get; set; }
    }

}