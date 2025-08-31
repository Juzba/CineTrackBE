using System.ComponentModel.DataAnnotations;

namespace CineTrackBE.Models.DTO;

#nullable disable
public class UserDto
{
    public string Id { get; set; }

    [Required(ErrorMessage = "UserName is required!") ]
    public string UserName { get; set; } 

    [Required(ErrorMessage = "Email is required!")]
    public string Email { get; set; }
    public string NewPassword { get; set; }
    public string PhoneNumber { get; set; }
    public bool EmailConfirmed { get; set; }
    public ICollection<string> Roles { get; set; }


}
