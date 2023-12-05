using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace DeliveryMonitoring.Models
{
    public class Login
    {
        [Required(ErrorMessage = "Username is required!")]
        [DataType(DataType.Text)]
        [DisplayName("Username")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Password is required!")]
        [DataType(DataType.Password)]
        //[NoTrim]
        [DisplayName("Password")]
        public string? Password { get; set; }

        [DisplayName("Remember Me")]
        public bool RememberMe { get; set; }
    }
    public class cookieValidation
    {
        public bool isValid { get; set; }
    }
}
