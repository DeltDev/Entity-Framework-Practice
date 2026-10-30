namespace Entity_Framework_Practice.Models;

public class Loan
{
    public int Id { get; set; }
    
    public int MemberId { get; set; }
    public Member Member { get; set; } = null!;
    
    public int BookId { get; set; }
    public Book Book { get; set; } = null!;
    
    public DateTime LoanedAt { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnedAt { get; set; }
}