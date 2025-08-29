using System.ComponentModel.DataAnnotations;

namespace CineTrackBE.Models.DTO;
#nullable disable
public class LoginDto
{
    [Required(ErrorMessage = "UserName is required!")]
    [MinLength(6, ErrorMessage = "UserName must have minimum 6 char!")]
    public string UserName { get; set; }

    [Required(ErrorMessage = "Password is required!")]
    [MinLength(6, ErrorMessage = "Password must have minimum 6 char!")]
    public string Password { get; set; }

}




