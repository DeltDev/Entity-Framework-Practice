using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Entity_Framework_Practice.Models;

[Index(nameof(Email), IsUnique = true)]
public class Member
{
    public int Id { get; set; }
    [Required] [MaxLength(250)] public string FirstName { get; set; } = string.Empty;
    [Required] [MaxLength(250)] public string LastName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty; 
    public DateTime JoinedAt { get; set; }
    
    public MemberCard? MemberCard { get; set; }
    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}