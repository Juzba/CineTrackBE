using System.ComponentModel.DataAnnotations;

namespace CineTrackBE.Models.DTO;
#nullable disable
public class LoginDto
{
    [Required(ErrorMessage = "Email is required!")]
    [MinLength(6, ErrorMessage = "Email must have minimum 6 char!")]
    [EmailAddress(ErrorMessage = "Invalid Email Address!")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Password is required!")]
    [MinLength(6, ErrorMessage = "Password must have minimum 6 char!")]
    public string Password { get; set; }
    public bool RememberMe { get; set; } = false;

}




