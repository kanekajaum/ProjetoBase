using Microsoft.AspNetCore.Mvc;
using ProjetoBase.Application.ViewModels;
using ProjetoBase.Domain.Entities;
using ProjetoBase.Infrastructure.Context;
using ProjetoBase.Infrastructure.Services;
using System.Security.Cryptography;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class UsuarioController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly AppDbContext _context;

    public UsuarioController(IConfiguration config, AppDbContext context)
    {
        _config = config;
        _context = context;
    }

    [HttpPost("Cadastrar")]
    public async Task<IActionResult> Cadastrar(UsuarioCadastroViewModel model)
    {
        if (_context.Usuarios.Any(u => u.Email == model.Email))
            return BadRequest("Email já está em uso.");

        var usuario = new Usuario
        {
            Nome = model.Nome,
            Email = model.Email,
            SenhaHash = model.Senha
        };

        usuario.SenhaHash = PasswordService.HashSenha(usuario.SenhaHash);

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        return Ok("Usuário cadastrado com sucesso.");
    }

    [HttpPost("Login")]
    public IActionResult Login([FromBody] UsuarioLoginViewModel model)
    {
        string senhaTela = HashSenhaSHA256(model.Senha);

        var usuario = _context.Usuarios.FirstOrDefault(u =>
            u.Email == model.Email && u.SenhaHash == senhaTela);

        if (usuario == null)
            return Unauthorized("Usuário ou senha inválidos.");

        var token = TokenService.GerarToken(usuario, _config);
        return Ok(new { token });
    }

    public static string HashSenhaSHA256(string senha)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(senha));
            return Convert.ToBase64String(bytes);
        }
    }
}