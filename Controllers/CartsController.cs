using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using LojaRemastered.Data;
using LojaRemastered.Models;
using Microsoft.AspNetCore.Identity;

namespace LojaRemastered.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Método privado para obter o carrinho do usuário atual.
        private async Task<Cart> GetCartAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            // Se o usuário não tiver um carrinho, cria um novo.
            if (cart == null)
            {
                cart = new Cart(userId);
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }
            return cart;
        }

        // Exibe a página com os itens do carrinho
        public async Task<IActionResult> Index()
        {
            var cart = await GetCartAsync();
            return View(cart);
        }

        // Adiciona um produto ao carrinho
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            // Verifica se o produto existe
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return NotFound("Produto não encontrado.");
            }

            // Verifica se há estoque suficiente
            if (product.Stocks < quantity)
            {
                TempData["Error"] = "Estoque insuficiente para essa quantidade.";
                return RedirectToAction("Store", "Products");
            }

            // Recupera ou cria o carrinho do usuário
            var cart = await GetCartAsync();

            // Verifica se o item já existe no carrinho e atualiza a quantidade, ou adiciona um novo item
            var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (cartItem != null)
            {
                // Adiciona a quantidade e reserva o estoque correspondente
                cartItem.Quantity += quantity;
            }
            else
            {
                // Cria um novo CartItem
                var newItem = new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = quantity,
                    CartId = cart.Id
                };
                cart.Items.Add(newItem);
            }

            // Reserva o estoque: diminui a quantidade disponível do produto
            
            _context.Products.Update(product);

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }


        // Remove um item do carrinho
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem == null)
            {
                return NotFound("Item do carrinho não encontrado.");
            }

            // Recupera o produto relacionado ao item do carrinho
            var product = await _context.Products.FindAsync(cartItem.ProductId);
            if (product != null)
            {
                // Devolve ao estoque a quantidade reservada para esse item
                product.Stocks += cartItem.Quantity;
                _context.Products.Update(product);
            }

            // Remove o item do carrinho
            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
            return RedirectToAction("Store");
        }


        // Limpa o carrinho
        [HttpPost]
        public async Task<IActionResult> ClearCart()
        {
            var cart = await GetCartAsync();

            // Itera sobre uma cópia da lista para evitar problemas ao modificar a coleção
            foreach (var item in cart.Items.ToList())
            {
                // Recupera o produto associado ao item do carrinho
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    // Libera o estoque, adicionando a quantidade reservada de volta
                    product.Stocks += item.Quantity;
                    _context.Products.Update(product);
                }

                // Remove o item do carrinho
                _context.CartItems.Remove(item);
            }

            // Limpa a lista de itens no carrinho (opcional, pois os itens já foram removidos do contexto)
            cart.Clear();

            await _context.SaveChangesAsync();

            return RedirectToAction("Store" ,"Products");
        }

        // Exibe a página de checkout (ainda a ser implementada)
        public async Task<IActionResult> Checkout()
        {
            var cart = await GetCartAsync();
            // Aqui você poderá adicionar lógica para processar a compra, criar transações, etc.
            return View(cart);
        }
    }
}
