using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LojaRemastered.Data;
using LojaRemastered.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace LojaRemastered.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            return View(await _context.Products.ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(Product product)
        {
            var user = await _userManager.GetUserAsync(User); // Obtém o usuário logado
            if (user == null)
            {
                return Unauthorized();
            }

            // Atribui automaticamente o SellerId e o Nome do usuario
            product.SellerId = user.Id;
            product.SellerName = user.UserName;
            // Adiciona ao banco de dados
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return RedirectToAction("Store");
        }

        // GET: Products/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, Product model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }
            

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            if (product.SellerId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return Unauthorized("Impossivel Editar Produtos de outros Usuarios");
            }



            if (ModelState.IsValid)
            {
                try
                {
                    // 🔹 Verifica se Name ou Price mudaram
                    if (product.Name != model.Name || product.Price != model.Price)
                    {
                        product.DataAnuncio = DateTime.Now;
                    }

                    product.Name = model.Name;
                    product.Price = model.Price;
                    product.Stocks = model.Stocks;

                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("MyProducts");
            }

            // Se ModelState não for válido, retorna a View com o modelo para exibir os erros de validação
            return View(model);
        }


        // GET: Products/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            if (product.SellerId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return Unauthorized("Impossivel deletar produtos de outros Usuarios");
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            if (product.SellerId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return Unauthorized("Impossível deletar produtos de outros Usuários");
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }



        [Authorize]
        public async Task<IActionResult> MyProducts()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return Unauthorized("Usuario não autenticado");
            }


            var userid = User.FindFirstValue(ClaimTypes.NameIdentifier);


            var userProducts = await _context.Products
                .Where(p => p.SellerId == userid)
                .ToListAsync();
                
            return View(userProducts);

        }

        public async Task<IActionResult> Store()
        {
            // Opcional: Filtrar somente produtos disponíveis, se necessário.
            var products = await _context.Products.ToListAsync();
            return View(products);
        }






    }
}
