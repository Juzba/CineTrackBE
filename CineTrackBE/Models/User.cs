using Microsoft.AspNetCore.Identity;

#nullable disable

namespace CineTrackBE.Models;

public class User: IdentityUser
{
    public ICollection<Comment> Comments { get; set; } = [];

}
