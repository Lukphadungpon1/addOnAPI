using System.ComponentModel.DataAnnotations;

namespace AddOn_API.DTOs.Account
{
    public class LoginRequest
    {
        [Required]
        public string? EmpUsername { get; set; }
        [Required]
        public string? EmpPassword { get; set; }
        [Required]
        public string? Program { get; set; }
    }
}