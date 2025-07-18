
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace QLNhaSach1.Models
{
    public class Review
    {
        public int ReviewId { get; set; }

        public int UserId { get; set; }
        [JsonIgnore]
        public User user { get; set; }

        public int BookId { get; set; }
        [JsonIgnore]
        public Book book { get; set; }

        [Required]
        [MaxLength(1000)]
        public required String ReviewText { get; set; }

        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        public DateTime ReviewDate { get; set; }
    }
}