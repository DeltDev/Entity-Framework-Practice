using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Entity_Framework_Practice.Models;

[Index(nameof(CardNumber), IsUnique = true)]
public class MemberCard
{
    public int Id { get; set; }
    [Required]
    [MaxLength(30)]
    public string CardNumber { get; set; } = String.Empty;
    public DateTime IssuedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int MemberId { get; set; }
    public Member Member { get; set; } = null!;
}