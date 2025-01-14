using Microsoft.AspNetCore.Identity;

namespace WebApplication1.Models
{
    public class User : IdentityUser
    {
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public List<Product> Products { get; set; } = new();
        public List<Message> SentMessages { get; set; } = new();
        public List<Message> ReceivedMessages { get; set; } = new();
    }
}