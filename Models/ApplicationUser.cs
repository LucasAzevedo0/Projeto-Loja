using Microsoft.AspNetCore.Identity;

namespace LojaRemastered.Models
{
    public class ApplicationUser : IdentityUser
    {
        public decimal Balance { get; set; }
    }
}
