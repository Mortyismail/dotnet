namespace WebApplication1.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalProducts { get; set; }
        public List<User> RecentUsers { get; set; } = new();
        public List<Product> RecentProducts { get; set; } = new();
    }
}