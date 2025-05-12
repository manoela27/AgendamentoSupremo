using AgendamentoApp.Entidade;
using Microsoft.AspNetCore.Identity;

namespace AgendamentoApp.Interface;

public interface IUserService
{
    Task<IdentityResult> CreateUserAsync(Usuario user, string password, string role);
    Task<Usuario> GetUserByIdAsync(string id);
    Task<Usuario> GetUserByEmailAsync(string email);
    Task<bool> ValidateUserCredentialsAsync(string email, string password);
    Task<IEnumerable<Usuario>> GetAllUsersAsync();
    Task<IdentityResult> UpdateUserAsync(Usuario user);
    Task<IdentityResult> DeleteUserAsync(string id);
}
