using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using QLNhaSach1.Hubs;
using UserEntity = QLNhaSach1.Models.User; // Sử dụng alias
using QLNhaSach1.Models;

namespace QLNhaSach1.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(AppDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public IActionResult Index(int? userId)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int currentUserId))
            {
                return Unauthorized("Người dùng chưa xác thực hoặc ID không hợp lệ.");
            }

            if (User.IsInRole("Admin"))
            {
                Models.User targetUser = null;

                if (userId.HasValue)
                {
                    targetUser = _context.Users.FirstOrDefault(u => u.UserId == userId.Value && u.Role != Role.Admin);
                    Console.WriteLine($"Quản trị viên trò chuyện với người dùng cụ thể: {userId.Value}, Tìm thấy: {targetUser?.UserName}");
                }

                // Nếu không có userId hoặc không tìm thấy người dùng, chọn một người dùng không phải quản trị viên mặc định
                if (targetUser == null)
                {
                    targetUser = _context.Users.FirstOrDefault(u => u.Role == Role.User); // Rõ ràng sử dụng Role.User
                    if (targetUser == null)
                    {
                        // Xử lý trường hợp không có người dùng không phải quản trị viên
                        ViewBag.TargetUserName = "Không có khách hàng nào để trò chuyện";
                        ViewBag.TargetUserId = null;
                        Console.WriteLine("Không tìm thấy người dùng không phải quản trị viên trong cơ sở dữ liệu.");
                        return View(); // Trả về view mà không đặt receiverId
                    }
                    Console.WriteLine($"Tự động chọn người dùng: {targetUser.UserName}");
                }

                ViewBag.TargetUserName = targetUser.UserName;
                ViewBag.TargetUserId = targetUser.UserId;

                Console.WriteLine($"ViewBag.TargetUserName: {ViewBag.TargetUserName}");
                Console.WriteLine($"ViewBag.TargetUserId: {ViewBag.TargetUserId}");
            }
            else
            {
                // Người dùng không phải quản trị viên trò chuyện với quản trị viên
                var adminUser = _context.Users.FirstOrDefault(u => u.Role == Role.Admin);
                if (adminUser == null)
                {
                    ViewBag.TargetUserName = "Không có quản trị viên để trò chuyện";
                    ViewBag.TargetUserId = null;
                    Console.WriteLine("Không tìm thấy người dùng quản trị viên trong cơ sở dữ liệu.");
                    return View();
                }

                ViewBag.TargetUserName = "Admin";
                ViewBag.TargetUserId = adminUser.UserId;
            }

            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult SelectUser()
        {
            var users = _context.Users
                .Where(u => u.Role != Role.Admin)
                .Select(u => new { u.UserId, u.UserName, u.Email })
                .ToList();

            return View(users);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult GetConversations()
        {
            int adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var conversations = _context.ChatMessages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => (m.SenderId == adminId || m.ReceiverId == adminId) &&
                            ((m.SenderId == adminId && m.Receiver.Role == Role.User) ||
                             (m.ReceiverId == adminId && m.Sender.Role == Role.User)))
                .AsEnumerable()
                .GroupBy(m => m.SenderId == adminId ? m.ReceiverId : m.SenderId)
                .Select(g => new
                {
                    userId = g.Key,
                    userName = g.First().SenderId == g.Key
                        ? g.First().Sender.UserName
                        : g.First().Receiver.UserName,
                    lastMsg = g.OrderByDescending(x => x.SentAt).First().Content,
                    lastTime = g.Max(x => x.SentAt)
                })
                .OrderByDescending(x => x.lastTime)
                .ToList();

            return Json(conversations);
        }

        [HttpGet]
        public IActionResult GetHistory(int otherUserId)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int currentUserId))
                return Unauthorized();

            var messages = _context.ChatMessages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => (m.SenderId == currentUserId && m.ReceiverId == otherUserId)
                         || (m.SenderId == otherUserId && m.ReceiverId == currentUserId))
                .OrderBy(m => m.SentAt)
                .Select(m => new
                {
                    senderId = m.SenderId,
                    senderName = m.Sender.UserName,
                    receiverName = m.Receiver.UserName, // THÊM NGƯỜI NHẬN
                    content = m.Content,
                    sentAt = m.SentAt
                })
                .ToList();

            return Json(messages);
        }
        [HttpPost]
        public async Task<IActionResult> Send(int receiverId, string content)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int senderId))
            {
                return Unauthorized("Người dùng chưa xác thực hoặc ID không hợp lệ.");
            }


            var receiver = await _context.Users.FindAsync(receiverId);
            if (receiver == null)
            {
                return BadRequest("Người nhận không tồn tại.");
            }

            var message = new ChatMessage
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = content,
                SentAt = DateTime.UtcNow
            };


            try
            {
                _context.ChatMessages.Add(message);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, $"Lỗi cơ sở dữ liệu: {dbEx.InnerException?.Message ?? dbEx.Message}");
            }
        }
        [HttpGet]
        public IActionResult GetAdminUserId()
        {
            var adminUser = _context.Users.FirstOrDefault(u => u.Role == Role.Admin);
            if (adminUser == null)
            {
                return NotFound("Không tìm thấy admin.");
            }
            return Json(new { adminUserId = adminUser.UserId });
        }
        [HttpGet]
        [Authorize]
        public IActionResult GetUserRole(int userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
            {
                return NotFound("Người dùng không tồn tại.");
            }
            return Json(new { role = user.Role.ToString() });
        }
    }
}