using AgendamentoApp.Entidade;
using AgendamentoApp.Infraestrutura;
using AgendamentoApp.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgendamentoApp.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IUserService _userService;
    private readonly ApplicationDbContext _context;

    public AdminController(
        IUserService userService,
        ApplicationDbContext context)
    {
        _userService = userService;
        _context = context;
    }

    // Funcionários
    public async Task<IActionResult> CadastrarFuncionario()
    {
        ViewBag.Cargos = await _context.Cargos.ToListAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CadastrarFuncionario(Funcionario funcionario, string password)
    {
        if (ModelState.IsValid)
        {
            funcionario.DataCadastro = DateTime.Now;
            funcionario.Ativo = true;

            var result = await _userService.CreateUserAsync(funcionario, password, "Funcionario");
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ListarFuncionarios));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        ViewBag.Cargos = await _context.Cargos.ToListAsync();
        return View(funcionario);
    }

    public async Task<IActionResult> ListarFuncionarios()
    {
        var funcionarios = await _context.Funcionarios
            .Include(f => f.Cargo)
            .ToListAsync();
        return View(funcionarios);
    }

    // Clientes
    public IActionResult CadastrarCliente()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CadastrarCliente(Cliente cliente, string password)
    {
        if (ModelState.IsValid)
        {
            cliente.DataCadastro = DateTime.Now;
            cliente.Ativo = true;

            var result = await _userService.CreateUserAsync(cliente, password, "Cliente");
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ListarClientes));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(cliente);
    }

    public async Task<IActionResult> ListarClientes()
    {
        var clientes = await _context.Clientes.ToListAsync();
        return View(clientes);
    }

    // Cargos
    public IActionResult CadastrarCargo()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CadastrarCargo(Cargo cargo)
    {
        if (ModelState.IsValid)
        {
            _context.Add(cargo);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ListarCargos));
        }
        return View(cargo);
    }

    public async Task<IActionResult> ListarCargos()
    {
        var cargos = await _context.Cargos
            .Include(c => c.Funcionarios)
            .ToListAsync();
        return View(cargos);
    }

    public async Task<IActionResult> EditarCargo(int? id)
    {
        if (id == null)
            return NotFound();

        var cargo = await _context.Cargos.FindAsync(id);
        if (cargo == null)
            return NotFound();

        return View(cargo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarCargo(int id, Cargo cargo)
    {
        if (id != cargo.Id)
            return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(cargo);
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
        return View(cargo);
    }

    public async Task<IActionResult> EditarCliente(string id)
    {
        if (id == null)
            return NotFound();

        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null)
            return NotFound();

        return View(cliente);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarCliente(string id, Cliente cliente)
    {
        if (id != cliente.Id)
            return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(cliente);
                await _context.SaveChangesAsync();
                TempData["StatusMessage"] = "Cliente atualizado com sucesso!";
                return RedirectToAction(nameof(ListarClientes));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClienteExists(cliente.Id))
                    return NotFound();
                else
                    throw;
            }
        }
        return View(cliente);
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
    public async Task<IActionResult> EditarFuncionario(string id, Funcionario funcionario)
    {
        if (id != funcionario.Id)
            return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(funcionario);
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

    private bool CargoExists(int id)
    {
        return _context.Cargos.Any(e => e.Id == id);
    }

    private bool ClienteExists(string id)
    {
        return _context.Clientes.Any(e => e.Id == id);
    }

    private bool FuncionarioExists(string id)
    {
        return _context.Funcionarios.Any(e => e.Id == id);
    }

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

    // Serviços
    public IActionResult CadastrarServico()
    {
        return View();
    }

    public async Task<IActionResult> EditarServico(int? id)
    {
        if (id == null)
            return NotFound();

        var servico = await _context.Servicos.FindAsync(id);
        if (servico == null)
            return NotFound();

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
    public async Task<IActionResult> CadastrarServico(Servico servico)
    {
        if (ModelState.IsValid)
        {
            servico.Ativo = true;
            _context.Add(servico);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ListarServicos));
        }
        return View(servico);
    }

    public async Task<IActionResult> ListarServicos()
    {
        var servicos = await _context.Servicos.ToListAsync();
        return View(servicos);
    }

 
    private bool ServicoExists(int id)
    {
        return _context.Servicos.Any(e => e.Id == id);
    }
}
