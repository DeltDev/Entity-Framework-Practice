using System.Linq.Expressions;
using Entity_Framework_Practice.Data;
using Entity_Framework_Practice.DTOs;
using Entity_Framework_Practice.Models;
using Microsoft.EntityFrameworkCore;

namespace Entity_Framework_Practice.Services;

public interface IMemberService
{
    Task<List<MemberDto>> GetAllAsync();
    Task<MemberDto?> GetByIdAsync(int id);
    Task<ServiceResult<MemberDto>> CreateAsync(CreateMemberRequest request);
    Task<ServiceResult<MemberDto>> UpdateAsync(int id, UpdateMemberRequest request);
    Task<ServiceResult> DeleteAsync(int id);
}

public class MemberService : IMemberService
{
    private const int CardValidityYears = 2;

    private readonly LibraryDbContext _db;
    public MemberService(LibraryDbContext db) => _db = db;

    private static readonly Expression<Func<Member, MemberDto>> ToDto = m => new MemberDto
    {
        Id = m.Id,
        FirstName = m.FirstName,
        LastName = m.LastName,
        Email = m.Email,
        JoinedAt = m.JoinedAt,
        Card = m.MemberCard == null
            ? null
            : new MemberCardDto
            {
                CardNumber = m.MemberCard.CardNumber,
                IssuedAt = m.MemberCard.IssuedAt,
                ExpiresAt = m.MemberCard.ExpiresAt
            },
        ActiveLoans = m.Loans.Count(l => l.ReturnedAt == null)
    };

    public Task<List<MemberDto>> GetAllAsync() =>
        _db.Members.AsNoTracking()
            .OrderBy(m => m.LastName).ThenBy(m => m.FirstName)
            .Select(ToDto)
            .ToListAsync();

    public Task<MemberDto?> GetByIdAsync(int id) =>
        _db.Members.AsNoTracking().Where(m => m.Id == id).Select(ToDto).FirstOrDefaultAsync();

    public async Task<ServiceResult<MemberDto>> CreateAsync(CreateMemberRequest request)
    {
        // Emails are stored in lowercase so the unique index treats "A@x.com" and "a@x.com" as one
        var email = request.Email.Trim().ToLowerInvariant();
        var cardNumber = request.CardNumber.Trim();

        if (await _db.Members.AnyAsync(m => m.Email == email))
            return ServiceResult<MemberDto>.Conflict($"A member with email '{email}' already exists.");

        if (await _db.MemberCards.AnyAsync(c => c.CardNumber == cardNumber))
            return ServiceResult<MemberDto>.Conflict($"Card number '{cardNumber}' is already in use.");

        var now = DateTime.UtcNow;
        var member = new Member
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            JoinedAt = now,
            MemberCard  = new MemberCard
            {
                CardNumber = cardNumber,
                IssuedAt = now,
                ExpiresAt = now.AddYears(CardValidityYears)
            }
        };

        // One SaveChanges is one transaction: the member and the card are saved together or not at all
        _db.Members.Add(member);
        await _db.SaveChangesAsync();

        return ServiceResult<MemberDto>.Ok((await GetByIdAsync(member.Id))!);
    }

    public async Task<ServiceResult<MemberDto>> UpdateAsync(int id, UpdateMemberRequest request)
    {
        var member = await _db.Members.FindAsync(id);
        if (member is null)
            return ServiceResult<MemberDto>.NotFound("Member not found.");

        var email = request.Email.Trim().ToLowerInvariant();
        if (await _db.Members.AnyAsync(m => m.Email == email && m.Id != id))
            return ServiceResult<MemberDto>.Conflict($"A member with email '{email}' already exists.");

        member.FirstName = request.FirstName.Trim();
        member.LastName = request.LastName.Trim();
        member.Email = email;
        await _db.SaveChangesAsync();

        return ServiceResult<MemberDto>.Ok((await GetByIdAsync(id))!);
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var member = await _db.Members.FindAsync(id);
        if (member is null)
            return ServiceResult.NotFound("Member not found.");

        // Loan history is never wiped, so a member with any loan row cannot be deleted
        if (await _db.Loans.AnyAsync(l => l.MemberId == id))
            return ServiceResult.Conflict("This member has loan history and cannot be deleted.");

        // The card is removed by the cascade delete on the one-to-one relationship
        _db.Members.Remove(member);
        await _db.SaveChangesAsync();
        return ServiceResult.Ok();
    }
}
