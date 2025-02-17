using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LojaRemastered.Data;
using LojaRemastered.Models;
using Microsoft.Build.Framework;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace LojaRemastered.Controllers
{
    [Authorize]
    public class TransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TransactionsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager) 
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Transactions
        public async Task<IActionResult> Index()
        {
            return View(await _context.Transactions.ToListAsync());
        }

        // GET: Transactions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // GET: Transactions/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Transactions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,RelatedUserId,Amount,TransactionType,TransactionDate,BalanceAfterTransaction")] Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                _context.Add(transaction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(transaction);
        }

        // GET: Transactions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }
            return View(transaction);
        }

        // POST: Transactions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,RelatedUserId,Amount,TransactionType,TransactionDate,BalanceAfterTransaction")] Transaction transaction)
        {
            if (id != transaction.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(transaction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransactionExists(transaction.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(transaction);
        }

        // GET: Transactions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // POST: Transactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TransactionExists(int id)
        {
            return _context.Transactions.Any(e => e.Id == id);
        }



        [HttpGet]
        public IActionResult Deposit()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Deposit(decimal amount)
        {
            // Recupera o usuário logado
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Unauthorized("Usuário não encontrado.");
            }

            // Atualiza o saldo do usuário
            user.Balance += amount;

            // Cria a transação de depósito
            var transaction = new Transaction
            {
                UserId = userId,
                RelatedUserId = userId, // Em depósito, o usuário é ele mesmo
                RelatedUserName = user.UserName,
                Amount = amount,
                Quantity = 1,
                ProductName = "Deposit",
                TransactionType = TransactionType.Deposit,
                TransactionDate = DateTime.UtcNow,
                BalanceAfterTransaction = user.Balance
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Depósito realizado com sucesso!";
            return RedirectToAction("History", "Transactions");
        }

        // ---------------------------------------------------
        // Ação para efetuar a compra de um produto
        // ---------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> BuyProduct(int productId)
        {
            // Recupera o produto a ser comprado
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return NotFound("Produto não encontrado.");
            }

            // Verifica se há unidades disponíveis
            if (product.Stocks < 1)
            {
                TempData["Error"] = "Produto esgotado.";
                return RedirectToAction("Store", "Products");
            }

            // Recupera o comprador (usuário logado)
            var buyerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var buyer = await _userManager.FindByIdAsync(buyerId);
            if (buyer == null)
            {
                return Unauthorized("Comprador não identificado.");
            }

            // Recupera o vendedor (proprietário do produto)
            var seller = await _userManager.FindByIdAsync(product.SellerId);
            if (seller == null)
            {
                return NotFound("Vendedor não encontrado.");
            }

            // Verifica se o comprador possui saldo suficiente para a compra
            if (buyer.Balance < product.Price)
            {
                TempData["Error"] = "Saldo insuficiente para realizar a compra.";
                return RedirectToAction("Store", "Products");
            }

            // Atualiza os saldos: deduz do comprador e credita no vendedor
            buyer.Balance -= product.Price;
            seller.Balance += product.Price;

            // Cria a transação para o comprador (Purchase)
            var purchaseTransaction = new Transaction
            {
                UserId = buyer.Id,
                RelatedUserId = seller.Id,
                RelatedUserName = seller.UserName, // para o comprador, o usuário relacionado é o vendedor
                Amount = product.Price,
                TransactionType = TransactionType.Purchase,
                TransactionDate = DateTime.UtcNow,
                BalanceAfterTransaction = buyer.Balance,
                ProductName = product.Name,
                Quantity = 1
            };

            // Cria a transação para o vendedor (Sale)
            var saleTransaction = new Transaction
            {
                UserId = seller.Id,
                RelatedUserId = buyer.Id,
                RelatedUserName = buyer.UserName, // para o vendedor, o usuário relacionado é o comprador
                Amount = product.Price,
                TransactionType = TransactionType.Sale,
                TransactionDate = DateTime.UtcNow,
                BalanceAfterTransaction = seller.Balance,
                ProductName = product.Name,
                Quantity = 1
            };

            _context.Transactions.Add(purchaseTransaction);
            _context.Transactions.Add(saleTransaction);

            // Atualiza o estoque:
            if (product.Stocks > 1)
            {
                product.Stocks -= 1;
                _context.Products.Update(product);
            }
            else
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Compra realizada com sucesso!";
            return RedirectToAction("Store", "Products");
        }






        [HttpPost]
        public async Task<IActionResult> BuyProducts()
        {
            // Recupera o usuário logado
            var buyerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var buyer = await _userManager.FindByIdAsync(buyerId);
            if (buyer == null)
            {
                return Unauthorized("Comprador não identificado.");
            }

            // Recupera o carrinho do usuário
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == buyerId);

            if (cart == null || !cart.Items.Any())
            {
                TempData["Error"] = "Seu carrinho está vazio.";
                return RedirectToAction("Store", "Products");
            }

            // Processa cada item do carrinho
            foreach (var item in cart.Items.ToList())
            {
                // Recupera o produto (garante que o produto exista)
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null)
                {
                    _context.CartItems.Remove(item);
                    continue;
                }

                // Verifica se a quantidade desejada não excede o estoque disponível
                if (item.Quantity > product.Stocks)
                {
                    TempData["Error"] = $"A quantidade solicitada para {product.Name} excede o estoque disponível.";
                    return RedirectToAction("Checkout", "Cart");
                }

                // Calcula o total do item
                var totalItemPrice = product.Price * item.Quantity;

                // Verifica se o comprador possui saldo suficiente para esse item
                if (buyer.Balance < totalItemPrice)
                {
                    TempData["Error"] = $"Saldo insuficiente para comprar {product.Name}.";
                    return RedirectToAction("Checkout", "Cart");
                }

                // Recupera o vendedor do produto
                var seller = await _userManager.FindByIdAsync(product.SellerId);
                if (seller == null)
                {
                    TempData["Error"] = $"Vendedor não encontrado para {product.Name}.";
                    return RedirectToAction("Checkout", "Cart");
                }

                // Atualiza os saldos: deduz do comprador e credita no vendedor
                buyer.Balance -= totalItemPrice;
                seller.Balance += totalItemPrice;

                // Cria a transação para o comprador (Purchase)
                var purchaseTransaction = new Transaction
                {
                    UserId = buyer.Id,
                    RelatedUserId = seller.Id,
                    RelatedUserName = buyer.UserName,
                    Amount = totalItemPrice,
                    TransactionType = TransactionType.Purchase,
                    TransactionDate = DateTime.UtcNow,
                    BalanceAfterTransaction = buyer.Balance,
                    Quantity = item.Quantity,
                    ProductName = product.Name
                };

                // Cria a transação para o vendedor (Sale)
                var saleTransaction = new Transaction
                {
                    UserId = seller.Id,
                    RelatedUserId = buyer.Id,
                    RelatedUserName = seller.UserName,
                    Amount = totalItemPrice,
                    TransactionType = TransactionType.Sale,
                    TransactionDate = DateTime.UtcNow,
                    BalanceAfterTransaction = seller.Balance,
                    Quantity = item.Quantity,
                    ProductName = product.Name
                };

                _context.Transactions.Add(purchaseTransaction);
                _context.Transactions.Add(saleTransaction);

                // Atualiza a lógica de estoque: use apenas um bloco para isso
                if (product.Stocks - item.Quantity <= 0)
                {
                    _context.Products.Remove(product);
                }
                else
                {
                    product.Stocks -= item.Quantity;
                    _context.Products.Update(product);
                }

                // Remove o item do carrinho
                _context.CartItems.Remove(item);
            }

            // Salva as alterações no banco
            await _context.SaveChangesAsync();

            TempData["Success"] = "Compra realizada com sucesso!";
            return RedirectToAction("Store", "Products");
        }












        [Authorize]
        public async Task<IActionResult> History()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            // Passa o saldo atual para a view
            ViewBag.CurrentBalance = user.Balance;

            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();

            return View(transactions);
        }














    }
}
