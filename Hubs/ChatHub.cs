using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using QLNhaSach1.Models;
using Microsoft.EntityFrameworkCore;

namespace QLNhaSach1.Hubs
{
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;

        public ChatHub(AppDbContext context)
        {
            _context = context;
        }

        public async Task SendMessage(string message, int? receiverId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(Context.UserIdentifier))
                    throw new HubException("User not authenticated");

                int senderId = int.Parse(Context.UserIdentifier);
                var sender = await _context.Users.FindAsync(senderId);
                if (sender == null)
                    throw new HubException("Sender not found");

                int targetReceiverId;

                // Debug logging
                Console.WriteLine($"SendMessage - SenderId: {senderId}, ReceiverIdParam: {receiverId}, SenderRole: {sender.Role}");

                if (sender.Role == Role.Admin)
                {
                    if (!receiverId.HasValue)
                        throw new HubException("Admin must specify a receiver");

                    var receiver = await _context.Users.FindAsync(receiverId.Value);
                    Console.WriteLine($"Admin sending to user {receiverId.Value}, found: {receiver?.UserName}");
                    
                    if (receiver == null || receiver.Role == Role.Admin)
                        throw new HubException("Receiver not found or invalid");

                    targetReceiverId = receiverId.Value;
                }
                else
                {
                    var admin = await _context.Users.FirstOrDefaultAsync(u => u.Role == Role.Admin);
                    if (admin == null)
                        throw new HubException("Admin not found");

                    targetReceiverId = admin.UserId;
                    Console.WriteLine($"User sending to admin {targetReceiverId}");
                }

                var chatMessage = new ChatMessage
                {
                    SenderId = senderId,
                    ReceiverId = targetReceiverId,
                    Content = message,
                    SentAt = DateTime.UtcNow
                };

                _context.ChatMessages.Add(chatMessage);
                await _context.SaveChangesAsync();

                // Gửi tin nhắn tới người nhận
                await Clients.User(targetReceiverId.ToString()).SendAsync("ReceiveMessage",
                    sender.UserName,
                    message,
                    sender.Role.ToString(),
                    senderId);

                // Gửi lại tin nhắn cho người gửi để hiển thị trong UI
                if (senderId != targetReceiverId)
                {
                    await Clients.Caller.SendAsync("ReceiveMessage",
                        sender.UserName,
                        message,
                        sender.Role.ToString(),
                        senderId);
                }

                // **THÊM NOTIFICATION CHO TIN NHẮN MỚI**
                await Clients.User(targetReceiverId.ToString()).SendAsync("NewMessageNotification", 
                    senderId, sender.UserName);

                Console.WriteLine($"Message sent successfully from {senderId} to {targetReceiverId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SendMessage ERROR: {ex}");
                throw new HubException($"Message send failed: {ex.Message}");
            }
        }

        public override async Task OnConnectedAsync()
        {
            if (!string.IsNullOrEmpty(Context.UserIdentifier))
            {
                int userId = int.Parse(Context.UserIdentifier);
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    string role = user.Role == Role.Admin ? "Admin" : "User";

                    // Add vào group nếu muốn broadcast
                    await Groups.AddToGroupAsync(Context.ConnectionId, role);

                    Console.WriteLine($"User {user.UserName} (ID: {userId}) connected to group: {role}");
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value ?? "User";

                Console.WriteLine($"User disconnected - ID: {userId}, Role: {role}");

                if (!string.IsNullOrEmpty(userId))
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, role);
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
                }

                if (exception != null)
                {
                    Console.WriteLine($"Disconnection error: {exception.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnDisconnectedAsync: {ex.Message}");
            }

            await base.OnDisconnectedAsync(exception);
        }

        private int GetFirstUserId()
        {
            // Đây chỉ là ví dụ — bạn có thể lấy người dùng đang chat gần nhất, hoặc chọn theo logic riêng
            return _context.Users.FirstOrDefault(u => u.Role == Role.User)?.UserId ?? 2;
        }
    }
}
