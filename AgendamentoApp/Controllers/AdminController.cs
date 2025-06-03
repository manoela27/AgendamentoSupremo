using AgendamentoApp.Entidade;
using AgendamentoApp.Infraestrutura;
using AgendamentoApp.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AgendamentoApp.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IUserService _userService;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<Usuario> _userManager;

    public AdminController(
        IUserService userService,
        ApplicationDbContext context,
        UserManager<Usuario> userManager)
    {
        _userService = userService;
        _context = context;
        _userManager = userManager;
    }

    // ========================= FUNCIONÁRIOS =========================
    [HttpGet]
    public async Task<IActionResult> CadastrarFuncionario()
    {
        ViewBag.Cargos = await _context.Cargos.ToListAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CadastrarFuncionario(
        string nome,
        string email,
        string cpf,
        string telefone,
        string endereco,
        int cargoId,
        string password)
    {
        if (ModelState.IsValid)
        {
            var funcionario = new Funcionario
            {
                Nome = nome,
                Email = email,
                UserName = email,
                CPF = cpf,
                Telefone = telefone,
                Endereco = endereco,
                CargoId = cargoId,
                DataCadastro = DateTime.Now,
                Ativo = true
            };

            var result = await _userService.CreateUserAsync(funcionario, password, "Funcionario");
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ListarFuncionarios));
            }

            foreach (var error in result.Errors)
            {
                var mensagem = error.Description;
                switch (error.Code)
                {
                    case "PasswordRequiresNonAlphanumeric":
                        mensagem = "A senha deve conter pelo menos um caractere especial.";
                        break;
                    case "PasswordRequiresDigit":
                        mensagem = "A senha deve conter pelo menos um número.";
                        break;
                    case "PasswordRequiresUpper":
                        mensagem = "A senha deve conter pelo menos uma letra maiúscula.";
                        break;
                    case "PasswordRequiresLower":
                        mensagem = "A senha deve conter pelo menos uma letra minúscula.";
                        break;
                    case "PasswordTooShort":
                        mensagem = "A senha deve conter pelo menos 6 caracteres.";
                        break;
                    case "DuplicateEmail":
                        mensagem = "Este email já está em uso.";
                        break;
                    case "InvalidEmail":
                        mensagem = "O email informado é inválido.";
                        break;
                }
                ModelState.AddModelError(string.Empty, mensagem);
            }
        }

        ViewBag.Cargos = await _context.Cargos.ToListAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExcluirFuncionario(string id)
    {
        var funcionario = await _context.Funcionarios.FindAsync(id);
        if (funcionario == null)
            return NotFound();

        _context.Funcionarios.Remove(funcionario);
        await _context.SaveChangesAsync();

        TempData["StatusMessage"] = "Funcionário excluído com sucesso!";
        return RedirectToAction(nameof(ListarFuncionarios));
    }

    public async Task<IActionResult> ListarFuncionarios()
    {
        var funcionarios = await _context.Funcionarios
            .Include(f => f.Cargo)
            .ToListAsync();
        return View(funcionarios);
    }

    public async Task<IActionResult> EditarFuncionario(string id)
    {
        if (id == null)
            return NotFound();

        var funcionario = await _context.Funcionarios
            .Include(f => f.Cargo)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (funcionario == null)
            return NotFound();

        ViewBag.Cargos = await _context.Cargos.ToListAsync();
        return View(funcionario);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarFuncionario(string id, Funcionario funcionario, string NovaSenha)
    {
        if (id != funcionario.Id)
            return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                var usuarioExistente = await _context.Funcionarios.FirstOrDefaultAsync(f => f.Id == funcionario.Id);
                if (usuarioExistente == null)
                    return NotFound();

                // Atualiza os dados principais
                usuarioExistente.Nome = funcionario.Nome;
                usuarioExistente.Email = funcionario.Email;
                usuarioExistente.UserName = funcionario.Email;
                usuarioExistente.Telefone = funcionario.Telefone;
                usuarioExistente.Endereco = funcionario.Endereco;
                usuarioExistente.CargoId = funcionario.CargoId;
                usuarioExistente.Ativo = funcionario.Ativo;

                // Se nova senha foi preenchida, atualiza
                if (!string.IsNullOrWhiteSpace(NovaSenha))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(usuarioExistente);
                    var result = await _userManager.ResetPasswordAsync(usuarioExistente, token, NovaSenha);

                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }

                        ViewBag.Cargos = await _context.Cargos.ToListAsync();
                        return View(funcionario);
                    }
                }

                await _context.SaveChangesAsync();
                TempData["StatusMessage"] = "Funcionário atualizado com sucesso!";
                return RedirectToAction(nameof(ListarFuncionarios));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FuncionarioExists(funcionario.Id))
                    return NotFound();
                else
                    throw;
            }
        }

        ViewBag.Cargos = await _context.Cargos.ToListAsync();
        return View(funcionario);
    }


    // ========================= CLIENTES =========================
    public IActionResult CadastrarCliente()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CadastrarCliente(
    string nome,
    string email,
    string cpf,
    string telefone,
    string endereco,
    string password)
    {
        if (ModelState.IsValid)
        {
            var cliente = new Cliente
            {
                Nome = nome,
                Email = email,
                UserName = email,
                CPF = cpf,
                Telefone = telefone,
                Endereco = endereco,
                DataCadastro = DateTime.Now,
                Ativo = true
            };

            var result = await _userService.CreateUserAsync(cliente, password, "Cliente");
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ListarClientes));
            }

            foreach (var error in result.Errors)
            {
                var mensagem = error.Description;
                switch (error.Code)
                {
                    case "PasswordRequiresNonAlphanumeric":
                        mensagem = "A senha deve conter pelo menos um caractere especial.";
                        break;
                    case "PasswordRequiresDigit":
                        mensagem = "A senha deve conter pelo menos um número.";
                        break;
                    case "PasswordRequiresUpper":
                        mensagem = "A senha deve conter pelo menos uma letra maiúscula.";
                        break;
                    case "PasswordRequiresLower":
                        mensagem = "A senha deve conter pelo menos uma letra minúscula.";
                        break;
                    case "PasswordTooShort":
                        mensagem = "A senha deve conter pelo menos 6 caracteres.";
                        break;
                    case "DuplicateEmail":
                        mensagem = "Este email já está em uso.";
                        break;
                    case "InvalidEmail":
                        mensagem = "O email informado é inválido.";
                        break;
                }
                ModelState.AddModelError(string.Empty, mensagem);
            }
        }

        return View();
    }


    public async Task<IActionResult> ListarClientes()
    {
        var clientes = await _context.Clientes.ToListAsync();
        return View(clientes);
    }

    [HttpGet]
    public async Task<IActionResult> EditarCliente(string id)
    {
        if (string.IsNullOrEmpty(id))
            return NotFound();

        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null)
            return NotFound();

        return View(cliente);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarCliente(string id, string nome, string email, string cpf, string telefone, string endereco, bool ativo, string novaSenha)
    {
        if (id == null)
            return NotFound();

        var cliente = await _userService.GetUserByIdAsync(id);
        if (cliente == null)
            return NotFound();

        if (ModelState.IsValid)
        {
            cliente.Nome = nome;
            cliente.Email = email;
            cliente.UserName = email;
            cliente.CPF = cpf;
            cliente.Telefone = telefone;
            cliente.Endereco = endereco;
            cliente.Ativo = ativo;

            try
            {
                if (!string.IsNullOrEmpty(novaSenha))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(cliente);
                    var resultSenha = await _userManager.ResetPasswordAsync(cliente, token, novaSenha);

                    if (!resultSenha.Succeeded)
                    {
                        foreach (var error in resultSenha.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }

                        return View(cliente);
                    }
                }

                var updateResult = await _userService.UpdateUserAsync(cliente);
                if (!updateResult.Succeeded)
                {
                    foreach (var error in updateResult.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);

                    return View(cliente);
                }

                TempData["StatusMessage"] = "Cliente atualizado com sucesso!";
                return RedirectToAction(nameof(ListarClientes));
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Erro ao atualizar cliente.");
            }
        }

        return View(cliente);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExcluirCliente(string id)
    {
        var cliente = await _context.Clientes
            .Include(c => c.Agendamentos)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cliente == null)
            return NotFound();

        if (cliente.Agendamentos != null && cliente.Agendamentos.Any())
        {
            TempData["StatusMessage"] = "Este cliente possui agendamentos e não pode ser excluído.";
            return RedirectToAction(nameof(ListarClientes));
        }

        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();

        TempData["StatusMessage"] = "Cliente excluído com sucesso!";
        return RedirectToAction(nameof(ListarClientes));
    }



    // ========================= CARGOS =========================
    public IActionResult CadastrarCargo()
    {
        ViewBag.Funcionarios = _context.Funcionarios
            .Select(f => new SelectListItem
            {
                Value = f.Id,
                Text = f.Nome
            }).ToList();

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CadastrarCargo(Cargo cargo, List<string> FuncionariosIds)
    {
        if (ModelState.IsValid)
        {
            if (FuncionariosIds != null && FuncionariosIds.Any())
            {
                var funcionariosSelecionados = await _context.Funcionarios
                    .Where(f => FuncionariosIds.Contains(f.Id))
                    .ToListAsync();

                cargo.Funcionarios = funcionariosSelecionados;
            }

            _context.Add(cargo);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ListarCargos));
        }

        // PREVENÇÃO DO ERRO DE VALIDAÇÃO INVÁLIDA
        ModelState.Remove("Funcionarios");

        ViewBag.Funcionarios = _context.Funcionarios
            .Select(f => new SelectListItem
            {
                Value = f.Id,
                Text = f.Nome
            }).ToList();

        return View(cargo);
    }

    public async Task<IActionResult> ListarCargos()
    {
        var cargos = await _context.Cargos
            .Include(c => c.Funcionarios)
            .ToListAsync();
        return View(cargos);
    }

    [HttpGet]
    public IActionResult EditarCargo(int id)
    {
        var cargo = _context.Cargos.Find(id);

        if (cargo == null)
            return NotFound();

        var funcionarios = _context.Funcionarios.ToList();

        ViewBag.Funcionarios = funcionarios.Select(f => new SelectListItem
        {
            Value = f.Id.ToString(),
            Text = f.Nome
        }).ToList();

        return View(cargo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarCargo(int id, Cargo cargo, List<string> funcionariosIds)
    {
        if (id != cargo.Id)
            return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                var cargoExistente = await _context.Cargos
                    .Include(c => c.Funcionarios)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (cargoExistente == null)
                    return NotFound();

                cargoExistente.Nome = cargo.Nome;
                cargoExistente.Descricao = cargo.Descricao;

                cargoExistente.Funcionarios.Clear();

                if (funcionariosIds != null && funcionariosIds.Any())
                {
                    var funcionariosSelecionados = await _context.Funcionarios
                        .Where(f => funcionariosIds.Contains(f.Id))
                        .ToListAsync();

                    foreach (var funcionario in funcionariosSelecionados)
                    {
                        cargoExistente.Funcionarios.Add(funcionario);
                    }
                }

                await _context.SaveChangesAsync();
                TempData["StatusMessage"] = "Cargo atualizado com sucesso!";
                return RedirectToAction(nameof(ListarCargos));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CargoExists(cargo.Id))
                    return NotFound();
                else
                    throw;
            }
        }

        ViewBag.Funcionarios = await _context.Funcionarios.ToListAsync();
        return View(cargo);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExcluirCargo(int id)
    {
        var cargo = await _context.Cargos
            .Include(c => c.Funcionarios)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cargo == null)
        {
            return NotFound();
        }

        // Verifica se algum funcionário está vinculado
        if (cargo.Funcionarios != null && cargo.Funcionarios.Any())
        {
            TempData["Erro"] = "Não é possível excluir um cargo vinculado a funcionários.";
            return RedirectToAction("ListarCargos");
        }

        _context.Cargos.Remove(cargo);
        await _context.SaveChangesAsync();

        TempData["Sucesso"] = "Cargo excluído com sucesso.";
        return RedirectToAction("ListarCargos");
    }


    // ========================= ATIVAÇÕES =========================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AtivarFuncionario(string id)
    {
        var funcionario = await _context.Funcionarios.FindAsync(id);
        if (funcionario == null)
            return NotFound();

        funcionario.Ativo = true;
        await _context.SaveChangesAsync();
        TempData["StatusMessage"] = "Funcionário ativado com sucesso!";
        return RedirectToAction(nameof(ListarFuncionarios));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DesativarFuncionario(string id)
    {
        var funcionario = await _context.Funcionarios.FindAsync(id);
        if (funcionario == null)
            return NotFound();

        funcionario.Ativo = false;
        await _context.SaveChangesAsync();
        TempData["StatusMessage"] = "Funcionário desativado com sucesso!";
        return RedirectToAction(nameof(ListarFuncionarios));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AtivarCliente(string id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null)
            return NotFound();

        cliente.Ativo = true;
        await _context.SaveChangesAsync();
        TempData["StatusMessage"] = "Cliente ativado com sucesso!";
        return RedirectToAction(nameof(ListarClientes));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DesativarCliente(string id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null)
            return NotFound();

        cliente.Ativo = false;
        await _context.SaveChangesAsync();
        TempData["StatusMessage"] = "Cliente desativado com sucesso!";
        return RedirectToAction(nameof(ListarClientes));
    }

    // ========================= SERVIÇOS =========================
    public async Task<IActionResult> CadastrarServico()
    {
        var cargos = await _context.Cargos.ToListAsync();

        ViewBag.Cargos = cargos.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = c.Nome
        }).ToList();

        return View();
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CadastrarServico(Servico servico)
    {
        if (ModelState.IsValid)
        {
            servico.Ativo = true;
            _context.Add(servico);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ListarServicos));
        }

        var cargos = await _context.Cargos.ToListAsync();
        ViewBag.Cargos = cargos.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = c.Nome
        }).ToList();

        return View(servico);
    }


    public async Task<IActionResult> ListarServicos()
    {
        var servicos = await _context.Servicos.ToListAsync();
        return View(servicos);
    }

    public async Task<IActionResult> EditarServico(int? id)
    {
        if (id == null)
            return NotFound();

        var servico = await _context.Servicos.FindAsync(id);
        if (servico == null)
            return NotFound();

        var cargos = await _context.Cargos.ToListAsync();
        ViewBag.Cargos = cargos.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = c.Nome
        }).ToList();

        return View(servico);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarServico(int id, Servico servico)
    {
        if (id != servico.Id)
            return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(servico);
                await _context.SaveChangesAsync();
                TempData["StatusMessage"] = "Serviço atualizado com sucesso!";
                return RedirectToAction(nameof(ListarServicos));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServicoExists(servico.Id))
                    return NotFound();
                else
                    throw;
            }
        }

        var cargos = await _context.Cargos.ToListAsync();
        ViewBag.Cargos = cargos.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = c.Nome
        }).ToList();

        return View(servico);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AtivarServico(int id)
    {
        var servico = await _context.Servicos.FindAsync(id);
        if (servico == null)
            return NotFound();

        servico.Ativo = true;
        await _context.SaveChangesAsync();
        TempData["StatusMessage"] = "Serviço ativado com sucesso!";
        return RedirectToAction(nameof(ListarServicos));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DesativarServico(int id)
    {
        var servico = await _context.Servicos.FindAsync(id);
        if (servico == null)
            return NotFound();

        servico.Ativo = false;
        await _context.SaveChangesAsync();
        TempData["StatusMessage"] = "Serviço desativado com sucesso!";
        return RedirectToAction(nameof(ListarServicos));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExcluirServico(int id)
    {
        var servico = await _context.Servicos.FindAsync(id);
        if (servico == null)
            return NotFound();

        _context.Servicos.Remove(servico);
        await _context.SaveChangesAsync();

        TempData["StatusMessage"] = "Serviço excluído com sucesso!";
        return RedirectToAction(nameof(ListarServicos));
    }

    // ========================= EXISTE =========================
    private bool CargoExists(int id) => _context.Cargos.Any(e => e.Id == id);
    private bool ClienteExists(string id) => _context.Clientes.Any(e => e.Id == id);
    private bool FuncionarioExists(string id) => _context.Funcionarios.Any(e => e.Id == id);
    private bool ServicoExists(int id) => _context.Servicos.Any(e => e.Id == id);
}
