using LojaRemastered.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Build.ObjectModelRemoting;
using Microsoft.EntityFrameworkCore;

namespace LojaRemastered.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
               
        public DbSet<Product> Products { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; } 
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Sua_Connection_String", options =>
                    options.EnableRetryOnFailure(
                        maxRetryCount: 5,      // Número máximo de tentativas
                        maxRetryDelay: TimeSpan.FromSeconds(10), // Tempo máximo entre tentativas
                        errorNumbersToAdd: null)); // Erros adicionais que devem acionar uma nova tentativa
            }
        }
    }
}
