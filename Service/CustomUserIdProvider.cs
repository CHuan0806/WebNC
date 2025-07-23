using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace QLNhaSach1.Service
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            // Lấy user ID từ Claim NameIdentifier (được gán khi đăng nhập)
            return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
