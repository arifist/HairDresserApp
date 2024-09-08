using System.ComponentModel.DataAnnotations;

namespace Entities.Dtos
{
    public record RegisterDto
    {
        public String? UserName { get; init; }

        public String? FullName { get; set; }


        [Required(ErrorMessage = "Email is required")]
        public String? Email { get; init; }

        [Required]
        [Phone]
        public String? PhoneNumber { get; set; }

        public String? VerificationCode { get; set; }
        public String? TwoFactAuthProviderName { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public String? Password { get; init; }
    }
}