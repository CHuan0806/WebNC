using System.ComponentModel.DataAnnotations;
using QLNhaSach1.Models;

namespace QLNhaSach1.ViewModels{
    public class UserUpdateViewModel
    {
        public int UserId { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? PasswordHash { get; set; }  // tên khác PasswordHash
        public Role Role { get; set; }
    }
}