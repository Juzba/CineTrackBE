using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CineTrackBE.Models.ViewModel;

#nullable disable

public class UserWithRoles
{
    public string Id { get; set; }

    [Required(ErrorMessage = "UserName je povinný!")]
    [MinLength(6, ErrorMessage = "UserName musí mít minimálně 6 znaků!")]
    public string UserName { get; set; }
    public string Email { get; set; }

    public bool EmailConfirmed { get; set; }

    public string PhoneNumber { get; set; }

    [Required(ErrorMessage = "Password je povinný!")]
    [MinLength(6, ErrorMessage = "Password musí mít minimálně 6 znaků!")]
    public string PasswordHash { get; set; }


    public ICollection<string> Roles { get; set; } = [];
}
