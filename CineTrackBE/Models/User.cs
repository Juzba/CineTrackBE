using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace CineTrackBE.Models;

public class User : IdentityUser
{
    public ICollection<Comment> Comments { get; set; } = [];

    [Required(ErrorMessage = "Email je povinný!")]
    [MinLength(6, ErrorMessage ="Email musí mít minimálně 6 znaků!")]
    public override string Email { get => base.Email; set => base.Email = value; }


    [Required(ErrorMessage = "Password je povinný!")]
    [MinLength(6, ErrorMessage = "Password musí mít minimálně 6 znaků!")]
    public override string PasswordHash { get => base.PasswordHash; set => base.PasswordHash = value; }

}
