using System.ComponentModel.DataAnnotations;

namespace ProjetoBase.MVC.Models
{
    public class LoginViewModel
    {
        [Required]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Senha { get; set; }
    }
}