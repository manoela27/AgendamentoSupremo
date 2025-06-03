using AgendamentoApp.Entidade;
using AgendamentoApp.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AgendamentoApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly IUserService _userService;
        private readonly SignInManager<Usuario> _signInManager;

        public AuthController(
            IUserService userService,
            SignInManager<Usuario> signInManager)
        {
            _userService = userService;
            _signInManager = signInManager;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var result = await _userService.ValidateUserCredentialsAsync(email, password);
                if (result)
                {
                    var user = await _userService.GetUserByEmailAsync(email);
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    if (!string.IsNullOrEmpty(returnUrl))
                        return LocalRedirect(returnUrl);

                    return RedirectToAction("Index", "Profile");
                }

                ModelState.AddModelError(string.Empty, "Tentativa de login inválida.");
            }

            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string nome, string email, string cpf, string telefone, string endereco, string password, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(nome))
                ModelState.AddModelError("nome", "Nome é obrigatório.");

            if (string.IsNullOrWhiteSpace(email))
                ModelState.AddModelError("email", "Email é obrigatório.");

            if (string.IsNullOrWhiteSpace(cpf))
                ModelState.AddModelError("cpf", "CPF é obrigatório.");

            if (string.IsNullOrWhiteSpace(telefone))
                ModelState.AddModelError("telefone", "Telefone é obrigatório.");

            if (string.IsNullOrWhiteSpace(endereco))
                ModelState.AddModelError("endereco", "Endereço é obrigatório.");

            if (string.IsNullOrWhiteSpace(password))
                ModelState.AddModelError("password", "Senha é obrigatória.");

            if (password != confirmPassword)
                ModelState.AddModelError("confirmPassword", "As senhas não coincidem.");

            // Verifica se já existe usuário com o mesmo e-mail
            var usuarioExistente = await _userService.GetUserByEmailAsync(email);
            if (usuarioExistente != null)
                ModelState.AddModelError("email", "Este e-mail já está em uso.");

            if (!ModelState.IsValid)
                return View();

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
                await _signInManager.SignInAsync(cliente, isPersistent: false);
                return RedirectToAction("Index", "Profile");
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
                    case "InvalidEmail":
                        mensagem = "O email informado é inválido.";
                        break;
                    default:
                        mensagem = error.Description;
                        break;
                }

                ModelState.AddModelError("password", mensagem);
            }

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
