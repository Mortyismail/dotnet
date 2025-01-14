using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public MessagesController(
            ApplicationDbContext context,
            UserManager<User> userManager)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Challenge();

            var conversations = await _context.Messages
                .Where(m => m.SenderId == currentUser.Id || m.ReceiverId == currentUser.Id)
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .GroupBy(m => m.SenderId == currentUser.Id ? m.ReceiverId : m.SenderId)
                .Select(g => new ConversationViewModel
                {
                    OtherUserId = g.Key,
                    OtherUserEmail = g.First().SenderId == currentUser.Id
                        ? g.First().Receiver!.Email
                        : g.First().Sender!.Email,
                    LastMessage = g.OrderByDescending(m => m.SentAt).First().Content,
                    LastMessageDate = g.Max(m => m.SentAt),
                    UnreadCount = g.Count(m => !m.IsRead && m.ReceiverId == currentUser.Id)
                })
                .ToListAsync();

            return View(conversations);
        }

        public async Task<IActionResult> Conversation(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Challenge();

            var otherUser = await _userManager.FindByIdAsync(userId);
            if (otherUser == null)
                return NotFound();

            var messages = await _context.Messages
                .Where(m => (m.SenderId == currentUser.Id && m.ReceiverId == userId) ||
                           (m.SenderId == userId && m.ReceiverId == currentUser.Id))
                .Include(m => m.Sender)
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            // Mark messages as read
            var unreadMessages = messages.Where(m => !m.IsRead && m.ReceiverId == currentUser.Id);
            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
            }
            await _context.SaveChangesAsync();

            ViewBag.OtherUser = otherUser;
            return View(messages);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(string receiverId, string content)
        {
            if (string.IsNullOrEmpty(content))
                return BadRequest();

            var sender = await _userManager.GetUserAsync(User);
            if (sender == null)
                return Challenge();

            var receiver = await _userManager.FindByIdAsync(receiverId);
            if (receiver == null)
                return NotFound();

            var message = new Message
            {
                SenderId = sender.Id,
                ReceiverId = receiverId,
                Content = content,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Conversation), new { userId = receiverId });
        }
    }
}