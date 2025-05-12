using AgendamentoApp.Entidade;
using Microsoft.AspNetCore.Identity;

namespace AgendamentoApp.Infraestrutura;

public static class DbInitializer
{
    public static async Task Initialize(
        ApplicationDbContext context,
        UserManager<Usuario> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        context.Database.EnsureCreated();

        string[] roles = { "Admin", "Funcionario", "Cliente" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Create Admin
        var adminEmail = "admin@sistema.com";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var admin = new Usuario
            {
                UserName = adminEmail,
                Email = adminEmail,
                Nome = "Administrador do Sistema",
                CPF = "000.000.000-00",
                Telefone = "(11) 99999-9999",
                Endereco = "Rua do Admin, 123",
                DataCadastro = DateTime.Now,
                Ativo = true,
                EmailConfirmed = true
            };

            await userManager.CreateAsync(admin, "Admin@123");
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        // Create Cargo and Funcionario
        if (!context.Cargos.Any())
        {
            var cargo = new Cargo
            {
                Nome = "Cabeleireiro(a)",
                Descricao = "Profissional especializado em cortes e tratamentos capilares"
            };
            context.Cargos.Add(cargo);
            await context.SaveChangesAsync();

            var funcionarioEmail = "funcionario@sistema.com";
            if (await userManager.FindByEmailAsync(funcionarioEmail) == null)
            {
                var funcionario = new Funcionario
                {
                    UserName = funcionarioEmail,
                    Email = funcionarioEmail,
                    Nome = "João Silva",
                    CPF = "111.111.111-11",
                    Telefone = "(11) 98888-8888",
                    Endereco = "Rua do Funcionário, 456",
                    DataCadastro = DateTime.Now,
                    Ativo = true,
                    EmailConfirmed = true,
                    CargoId = cargo.Id
                };

                await userManager.CreateAsync(funcionario, "Funcionario@123");
                await userManager.AddToRoleAsync(funcionario, "Funcionario");
            }
        }

        // Create Cliente
        var clienteEmail = "cliente@sistema.com";
        if (await userManager.FindByEmailAsync(clienteEmail) == null)
        {
            var cliente = new Cliente
            {
                UserName = clienteEmail,
                Email = clienteEmail,
                Nome = "Maria Santos",
                CPF = "222.222.222-22",
                Telefone = "(11) 97777-7777",
                Endereco = "Rua do Cliente, 789",
                DataCadastro = DateTime.Now,
                Ativo = true,
                EmailConfirmed = true
            };

            await userManager.CreateAsync(cliente, "Cliente@123");
            await userManager.AddToRoleAsync(cliente, "Cliente");
        }

        if (!context.Servicos.Any())
        {
            var servico = new Servico
            {
                Nome = "Corte de Cabelo",
                Descricao = "Corte de cabelo masculino ou feminino",
                Preco = 50.00M,
                DuracaoMinutos = 30,
                Ativo = true
            };
            context.Servicos.Add(servico);
            await context.SaveChangesAsync();
        }
    }
}
