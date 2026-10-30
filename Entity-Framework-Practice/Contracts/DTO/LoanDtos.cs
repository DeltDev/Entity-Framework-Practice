using System.ComponentModel.DataAnnotations;

namespace Entity_Framework_Practice.DTOs;

public class LoanDto
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public int BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public DateTime LoanedAt { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnedAt { get; set; }

    // Computed on the way out, not stored in the database
    public bool IsOverdue => ReturnedAt is null && DueDate < DateTime.UtcNow;
}

// Only the ids come from the client. LoanedAt and DueDate are set by the service.
public class BorrowRequest
{
    [Range(1, int.MaxValue)]
    public int MemberId { get; set; }

    [Range(1, int.MaxValue)]
    public int BookId { get; set; }
}
