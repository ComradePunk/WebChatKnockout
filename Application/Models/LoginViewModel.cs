using System.ComponentModel.DataAnnotations;

namespace Application.Models
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Email или имя пользователя")]
        public string EmailOrUserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [Display(Name = "Запомнить?")]
        public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; }
    }
}
