using System.ComponentModel.DataAnnotations;

namespace Entity_Framework_Practice.Contracts.Requests;

public class AuthorRequest
{
    [Required, MaxLength(250)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Country { get; set; } = string.Empty;
}