using Microsoft.AspNetCore.Identity;

namespace AgendamentoApp.Entidade;

public class Usuario : IdentityUser
{
    public string Nome { get; set; }
    public string CPF { get; set; }
    public string Endereco { get; set; }
    public string Telefone { get; set; }
    public DateTime DataCadastro { get; set; }
    public bool Ativo { get; set; }
}