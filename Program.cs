using LojaRemastered.Data;
using LojaRemastered.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configuração do banco de dados
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()));
// Configuração do Identity com ApplicationUser
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>() // Suporte a roles (ADM, Vendedor, Comprador)
.AddEntityFrameworkStores<ApplicationDbContext>();

// Adiciona suporte ao MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var dbContext = services.GetRequiredService<ApplicationDbContext>();

    await SeedAdminAccount(userManager);
    await SeedLojaAccount(userManager, dbContext);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); // Necessário para o Identity funcionar

app.Run();








async Task SeedAdminAccount(UserManager<ApplicationUser> userManager)
{
    
    string adminEmail = "admin@admin.com";
    string adminPassword = "Admin123!"; // Senha padrão de demonstração
    
    // Verificar se o usuário Admin já existe
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            Balance = 9999
        };

        await userManager.CreateAsync(adminUser, adminPassword);
    }
}



async Task SeedLojaAccount(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext)
{
    string lojaEmail = "loja@loja.com";
    string lojaPassword = "Loja123!";

    // Verifica se o usuário Loja já existe
    var lojaUser = await userManager.FindByEmailAsync(lojaEmail);
    if (lojaUser == null)
    {
        lojaUser = new ApplicationUser
        {
            UserName = lojaEmail,
            Email = lojaEmail,
            EmailConfirmed = true,
            Balance = 1000000m
        };

        await userManager.CreateAsync(lojaUser, lojaPassword);
    }

    // Se os produtos da Loja ainda não foram criados, adicionamos 10 produtos
    if (!dbContext.Products.Any(p => p.SellerId == lojaUser.Id))
    {
        var produtos = new List<Product>
        {
            new Product { Name = "Notebook Gamer", Price = 4500m, Stocks = 5, SellerId = lojaUser.Id, SellerName = lojaEmail },
            new Product { Name = "Smartphone X", Price = 2800m, Stocks = 10, SellerId = lojaUser.Id , SellerName = lojaEmail },
            new Product { Name = "Headset Pro",   Price = 600m,     Stocks = 20, SellerId = lojaUser.Id , SellerName = lojaEmail},
            new Product { Name = "Teclado Mecânico", Price = 350m,  Stocks = 15, SellerId = lojaUser.Id, SellerName = lojaEmail },
            new Product { Name = "Mouse Gamer", Price = 250m, Stocks = 30, SellerId = lojaUser.Id, SellerName = lojaEmail },
            new Product { Name = "Monitor Full HD", Price = 1200m, Stocks = 8, SellerId = lojaUser.Id, SellerName = lojaEmail },
            new Product { Name = "Cadeira Gamer", Price = 900m, Stocks = 6, SellerId = lojaUser.Id, SellerName = lojaEmail },
            new Product { Name = "Placa de Vídeo RTX", Price = 5500m, Stocks = 3, SellerId = lojaUser.Id, SellerName = lojaEmail },
            new Product { Name = "SSD 1TB", Price = 700m, Stocks = 12, SellerId = lojaUser.Id, SellerName = lojaEmail },
            new Product { Name = "Fonte 750W", Price = 400m, Stocks = 10, SellerId = lojaUser.Id, SellerName = lojaEmail }
        };

        dbContext.Products.AddRange(produtos);
        await dbContext.SaveChangesAsync();
    }
}