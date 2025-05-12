namespace AgendamentoApp.Entidade   
{
    public class Cargo
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public ICollection<Funcionario> Funcionarios { get; set; }
    }
}