using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace CineTrackBE.Models.Entities;

public class ApplicationUser : IdentityUser
{
    public ICollection<Comment> Comments { get; set; } = [];

    [Required(ErrorMessage = "UserName je povinný!")]
    [MinLength(6, ErrorMessage ="UserName musí mít minimálně 6 znaků!")]
    public override string UserName { get => base.UserName; set => base.UserName = value; }


    [Required(ErrorMessage = "Password je povinný!")]
    [MinLength(6, ErrorMessage = "Password musí mít minimálně 6 znaků!")]
    public override string PasswordHash { get => base.PasswordHash; set => base.PasswordHash = value; }


    public ICollection<int> FavoriteMovies { get; set; } = [];



}
