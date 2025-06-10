using System.ComponentModel.DataAnnotations;

namespace QLNhaSach1.Models
{
    
    public class User
    {
        public int UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string UserName { get; set; } = string.Empty;    

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [DataType(DataType.PhoneNumber)]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Address { get; set; } = string.Empty;

        public Role Role { get; set; } = Role.User;
    }
}