using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Models.ViewModels;
using WebApplication1.Services;
using Microsoft.AspNetCore.Authorization;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IFileStorageService _fileStorage;

        public ProductsController(
            ApplicationDbContext context,
            UserManager<User> userManager,
            IFileStorageService fileStorage)
        {
            _context = context;
            _userManager = userManager;
            _fileStorage = fileStorage;
        }

        public async Task<IActionResult> Index(string searchString, int? categoryId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .Include(p => p.Images)
                .AsQueryable();

            // Exclude current user's products
            if (currentUser != null)
            {
                query = query.Where(p => p.SellerId != currentUser.Id);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p =>
                    p.Title.Contains(searchString) ||
                    p.Description.Contains(searchString));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            var products = await query.ToListAsync();
            ViewData["Categories"] = new SelectList(_context.Categories, "Id", "Name");
            return View(products);
        }

        public IActionResult Create()
        {
            ViewData["Categories"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Challenge();

                var product = new Product
                {
                    Title = model.Title,
                    Description = model.Description,
                    Price = model.Price,
                    CategoryId = model.CategoryId,
                    SellerId = user.Id,
                    ListedDate = DateTime.UtcNow,
                    Images = new List<ProductImage>()
                };

                if (model.Images != null)
                {
                    foreach (var image in model.Images)
                    {
                        var imageUrl = await _fileStorage.SaveImageAsync(image);
                        product.Images.Add(new ProductImage
                        {
                            ImageUrl = imageUrl,
                            UploadedAt = DateTime.UtcNow
                        });
                    }
                }

                Console.WriteLine($"Adding product: {product.Title} for user: {user.Email}");

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                Console.WriteLine($"Product saved with ID: {product.Id}");

                return RedirectToAction(nameof(Index));
            }

            foreach (var modelState in ModelState.Values)
            {
                foreach (var error in modelState.Errors)
                {
                    Console.WriteLine($"Model error: {error.ErrorMessage}");
                }
            }

            ViewData["Categories"] = new SelectList(_context.Categories, "Id", "Name");
            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (product.SellerId != user.Id && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    

    [Authorize]

        
        public async Task<IActionResult> MyListings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Where(p => p.SellerId == user.Id)
                .OrderByDescending(p => p.ListedDate)
                .ToListAsync();

            return View(products);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id && p.SellerId == user.Id);

            if (product == null)
                return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyListings));
        }
    }
}