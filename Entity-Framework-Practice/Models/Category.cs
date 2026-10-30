using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Entity_Framework_Practice.Models;

[Index(nameof(Name), IsUnique = true)]
public class Category
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; }
    
    // satu category bisa dimiliki banyak buku
    public ICollection<Book> Books { get; set; } = new List<Book>();
}