using System.Collections.Generic;

namespace AgendamentoApp.Entidade
{
    public class Funcionario : Usuario
    {
        public Cargo Cargo { get; set; }
        public int CargoId { get; set; }
        public ICollection<Agendamento> Agendamentos { get; set; }
    }
}