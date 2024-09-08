using System.ComponentModel.DataAnnotations;

namespace HairDresserApp.Models
{
    public class LoginModel
    {
        private string? _returnurl;

        //[Required(ErrorMessage = "Name is required.")]
        //public string? Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } 

        [Required(ErrorMessage = "Password is required.")]
        public string? Password { get; set; }

        public string? ReturnUrl
        {
            get
            {
                return _returnurl ?? "/";
            }
            set
            {
                _returnurl = value;
            }
        }
    }
}