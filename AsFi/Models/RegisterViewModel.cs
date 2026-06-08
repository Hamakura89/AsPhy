using System.ComponentModel.DataAnnotations;

namespace AsFi.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Введите логин (6 цифр)")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Логин должен состоять из 6 цифр")]
        public string Login { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите фамилию")]
        [StringLength(50, ErrorMessage = "Фамилия должна содержать не более 50 символов")]
        [RegularExpression(@"^[а-яА-ЯёЁ\s-]+$", ErrorMessage = "Фамилия должна содержать только русские буквы")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите имя")]
        [StringLength(50, ErrorMessage = "Имя должно содержать не более 50 символов")]
        [RegularExpression(@"^[а-яА-ЯёЁ\s-]+$", ErrorMessage = "Имя должно содержать только русские буквы")]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Отчество должно содержать не более 50 символов")]
        public string? Patronymic { get; set; }

        [Required(ErrorMessage = "Введите email")]
        [EmailAddress(ErrorMessage = "Введите корректный email адрес")]
        public string Email { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Введите пароль")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "Пароль должен содержать от 6 до 20 символов")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}