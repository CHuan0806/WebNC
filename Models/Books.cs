using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace QLNhaSach1.Models
{
    public class Book
    {
        public int bookId { get; set; }

        [Required]
        [MaxLength(50)]
        public string? bookName { get; set; }

        [Required]
        [MaxLength(1000)]
        public string? description { get; set; }

        [Required]
        [MaxLength(30)]
        public string? author { get; set; }


        [DisplayName("Trạng thái")]
        public bool bookStatus { get; set; }

        [Required,
        DataType(DataType.Currency),
        DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = true)]

        public int price { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public Category? Category { get; set; }

        [Display(Name = "Image URL")]
        public string? ImageUrl { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng phải >= 0")]
        public int quantity { get; set; }



    }
}
