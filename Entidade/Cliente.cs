using System.Collections.Generic;

namespace AgendamentoApp.Entidade
{

    public class Cliente : Usuario
    {
        public ICollection<Agendamento> Agendamentos { get; set; }
    }

}