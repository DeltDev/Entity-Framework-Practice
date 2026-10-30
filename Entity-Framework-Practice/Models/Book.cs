using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Entity_Framework_Practice.Models;


[Index(nameof(ISBN), IsUnique = true)]
public class Book
{
    public int Id { get; set; }
    
    [Required]                          
    [MaxLength(250)]
    public string Title { get; set; }
    [Required]
    [MaxLength(20)]
    public string ISBN { get; set; }
    public DateTime PublishDate { get; set; }
    public int TotalCopies { get; set; }
    public int AuthorId { get; set; }
    public Author Author { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; }
    
    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}