using AgendamentoApp.Entidade;
using Microsoft.AspNetCore.Identity;
using AgendamentoApp.Interface;
using Microsoft.EntityFrameworkCore;

namespace AgendamentoApp.Infraestrutura;

public class UserService : IUserService
{
    private readonly UserManager<Usuario> _userManager;
    private readonly SignInManager<Usuario> _signInManager;
    private readonly ApplicationDbContext _context;

    public UserService(
        UserManager<Usuario> userManager,
        SignInManager<Usuario> signInManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
    }

    public async Task<IdentityResult> CreateUserAsync(Usuario user, string password, string role)
    {
        var result = await _userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, role);
        }
        return result;
    }

    public async Task<Usuario> GetUserByIdAsync(string id)
    {
        return await _userManager.FindByIdAsync(id);
    }

    public async Task<Usuario> GetUserByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<bool> ValidateUserCredentialsAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return false;
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
        return result.Succeeded;
    }

    public async Task<IEnumerable<Usuario>> GetAllUsersAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<IdentityResult> UpdateUserAsync(Usuario user)
    {
        return await _userManager.UpdateAsync(user);
    }

    public async Task<IdentityResult> DeleteUserAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return IdentityResult.Failed(new IdentityError { Description = "User not found" });
        }
        return await _userManager.DeleteAsync(user);
    }
}
