using System.ComponentModel.DataAnnotations;

namespace AsFi.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        public string Login { get; set; } = string.Empty;

        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; } = false;
    }
}