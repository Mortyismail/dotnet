using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Message
    {
        public int Id { get; set; }
        [Required]
        public string SenderId { get; set; } = string.Empty;
        public User? Sender { get; set; }
        [Required]
        public string ReceiverId { get; set; } = string.Empty;
        public User? Receiver { get; set; }
        [Required]
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
    }
}