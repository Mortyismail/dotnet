using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using WebApplication1.Data;
using WebApplication1.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public class MyListingsViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public MyListingsViewComponent(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        // Get the current user
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user == null)
        {
            Console.WriteLine("User not found."); // Debugging message
            return Content("User not found.");
        }

        // Fetch the user's listings
        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Where(p => p.SellerId == user.Id)
            .OrderByDescending(p => p.ListedDate)
            .ToListAsync();

        // Debugging: Print the number of listings and user email
        Console.WriteLine($"Found {products.Count} listings for user {user.Email}");

        if (!products.Any())
        {
            Console.WriteLine("No listings found for the user."); // Debugging message
            return Content("No listings found.");
        }

        return View(products);
    }
}