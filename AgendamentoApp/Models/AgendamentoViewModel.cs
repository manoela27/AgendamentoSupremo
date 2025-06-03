using System.ComponentModel.DataAnnotations;
using AgendamentoApp.Entidade;

namespace AgendamentoApp.Models
{
    public class AgendamentoViewModel
    {
        [Required(ErrorMessage = "O serviço é obrigatório")]
        public int ServicoId { get; set; }

        [Required(ErrorMessage = "O profissional é obrigatório")]
        public string FuncionarioId { get; set; }

        [Required(ErrorMessage = "A data é obrigatória")]
        public string Data { get; set; }

        [Required(ErrorMessage = "O horário é obrigatório")]
        public string Horario { get; set; }

        [Required(ErrorMessage = "O campo observações é obrigatório")]
        public string Observacoes { get; set; }

        public Agendamento ToEntity(string clienteId, Cliente cliente, Funcionario funcionario, Servico servico)
        {
            DateTime.TryParse($"{Data} {Horario}", out DateTime dataHora);

            return new Agendamento
            {
                DataHora = dataHora,
                Status = "Agendado",
                Observacoes = Observacoes ?? "",
                ClienteId = clienteId,
                Cliente = cliente,
                FuncionarioId = FuncionarioId,
                Funcionario = funcionario,
                ServicoId = ServicoId,
                Servico = servico
            };
        }
    }
}
