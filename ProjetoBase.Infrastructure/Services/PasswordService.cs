using System.Security.Cryptography;
using System.Text;

namespace ProjetoBase.Infrastructure.Services
{
    public static class PasswordService
    {
        public static string HashSenha(string senha)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(senha));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}