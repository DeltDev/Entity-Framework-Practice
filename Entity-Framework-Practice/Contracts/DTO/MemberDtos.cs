using System.ComponentModel.DataAnnotations;

namespace Entity_Framework_Practice.DTOs;

public class MemberCardDto
{
    public string CardNumber { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}

public class MemberDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
    public MemberCardDto? Card { get; set; }
    public int ActiveLoans { get; set; }
}

// Creating a member also issues their card, so the card number is part of this request
public class CreateMemberRequest
{
    [Required, MaxLength(250)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(250)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string CardNumber { get; set; } = string.Empty;
}

// The card is issued once, so updates only touch the member's own details
public class UpdateMemberRequest
{
    [Required, MaxLength(250)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(250)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(100)]
    public string Email { get; set; } = string.Empty;
}
