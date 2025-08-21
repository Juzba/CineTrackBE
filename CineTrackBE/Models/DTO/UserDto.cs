using Microsoft.AspNetCore.Identity;

namespace CineTrackBE.Models.DTO
{
    public class UserDto
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public IList<string> Roles { get; set; } = [];



    }
}
