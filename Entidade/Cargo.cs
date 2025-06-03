using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
namespace AgendamentoApp.Entidade   
{
    public class Cargo
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }

        [ValidateNever]
        public ICollection<Funcionario> Funcionarios { get; set; }
    }
}