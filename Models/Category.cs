

namespace QLNhaSach1.Models{
public class Category
{
    public int Id { get; set; }
    public string categoryName { get; set; } = string.Empty; 

    public List<Book> Books { get; set; } = new List<Book>();
}
}