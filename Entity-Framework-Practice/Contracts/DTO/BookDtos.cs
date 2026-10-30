using System.ComponentModel.DataAnnotations;

namespace Entity_Framework_Practice.DTOs;

public class BookDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public DateTime PublishDate { get; set; }
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }
    public int AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}

// One request class for both create and update
public class BookRequest
{
    [Required, MaxLength(250)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string ISBN { get; set; } = string.Empty;

    public DateTime PublishDate { get; set; }

    [Range(1, 1000)]
    public int TotalCopies { get; set; }

    [Range(1, int.MaxValue)]
    public int AuthorId { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }
}
