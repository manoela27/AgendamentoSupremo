using AgendamentoApp.Entidade;
using System.Collections.Generic;
using System.Reflection.Emit;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AgendamentoApp.Infraestrutura;

public class ApplicationDbContext : IdentityDbContext<Usuario>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : base(options)
    {
    }

    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Funcionario> Funcionarios { get; set; }
    public DbSet<Cargo> Cargos { get; set; }
    public DbSet<Servico> Servicos { get; set; }
    public DbSet<Agendamento> Agendamentos { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Agendamento>()
            .HasOne(a => a.Cliente)
            .WithMany(c => c.Agendamentos)
            .HasForeignKey(a => a.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Agendamento>()
            .HasOne(a => a.Funcionario)
            .WithMany(f => f.Agendamentos)
            .HasForeignKey(a => a.FuncionarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Agendamento>()
            .HasOne(a => a.Servico)
            .WithMany(s => s.Agendamentos)
            .HasForeignKey(a => a.ServicoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Funcionario>()
            .HasOne(f => f.Cargo)
            .WithMany(c => c.Funcionarios)
            .HasForeignKey(f => f.CargoId)
            .OnDelete(DeleteBehavior.Restrict);

     
        builder.Entity<Servico>()
            .Property(s => s.Preco)
            .HasPrecision(10, 2);

        builder.Entity<Servico>()
            .HasOne(s => s.Cargo)
            .WithMany()
            .HasForeignKey(s => s.CargoId)
            .OnDelete(DeleteBehavior.Restrict);
    }

}
