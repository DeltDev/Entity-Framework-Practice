using System.ComponentModel.DataAnnotations;

namespace Entity_Framework_Practice.Models;

public class Author
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(250)]
    public string Name { get; set; }
    
    [Required]
    public string Country { get; set; }
    
    //satu author bisa punya banyak bku yang ditulis
    public ICollection<Book> Books { get; set; } = new List<Book>();
}