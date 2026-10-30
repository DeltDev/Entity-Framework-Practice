namespace Entity_Framework_Practice.Contracts.DTO;

public class AuthorDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}