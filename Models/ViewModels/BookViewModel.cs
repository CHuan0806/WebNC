using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QLNhaSach1.Models.ViewModels
{
    public class BookViewModel
    {
        public Book Book { get; set; } = new Book();

        [Display(Name = "Danh mục")]
        public SelectList Categories { get; set; } = new SelectList(new List<Category>(), "CategoryId", "categoryName");
    }
}
