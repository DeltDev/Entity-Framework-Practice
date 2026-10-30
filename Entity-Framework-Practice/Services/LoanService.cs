using System.Linq.Expressions;
using Entity_Framework_Practice.Data;
using Entity_Framework_Practice.DTOs;
using Entity_Framework_Practice.Models;
using Microsoft.EntityFrameworkCore;

namespace Entity_Framework_Practice.Services;

public interface ILoanService
{
    Task<List<LoanDto>> GetAllAsync(int? memberId = null, int? bookId = null);
    Task<LoanDto?> GetByIdAsync(int id);
    Task<List<LoanDto>> GetActiveAsync(int? memberId = null);
    Task<List<LoanDto>> GetOverdueAsync();
    Task<ServiceResult<LoanDto>> BorrowAsync(BorrowRequest request);
    Task<ServiceResult<LoanDto>> ReturnAsync(int id);
}

public class LoanService : ILoanService
{
    private const int MaxActiveLoans = 3; // rule 1
    private const int LoanDays = 14;      // rule 3

    private readonly LibraryDbContext _db;
    public LoanService(LibraryDbContext db) => _db = db;

    private static readonly Expression<Func<Loan, LoanDto>> ToDto = l => new LoanDto
    {
        Id = l.Id,
        MemberId = l.MemberId,
        MemberName = l.Member.FirstName + " " + l.Member.LastName,
        BookId = l.BookId,
        BookTitle = l.Book.Title,
        LoanedAt = l.LoanedAt,
        DueDate = l.DueDate,
        ReturnedAt = l.ReturnedAt
    };

    public async Task<List<LoanDto>> GetAllAsync(int? memberId = null, int? bookId = null)
    {
        var query = _db.Loans.AsNoTracking();

        if (memberId is not null) query = query.Where(l => l.MemberId == memberId);
        if (bookId is not null) query = query.Where(l => l.BookId == bookId);

        return await query.OrderByDescending(l => l.LoanedAt).Select(ToDto).ToListAsync();
    }

    public Task<LoanDto?> GetByIdAsync(int id) =>
        _db.Loans.AsNoTracking().Where(l => l.Id == id).Select(ToDto).FirstOrDefaultAsync();

    public async Task<List<LoanDto>> GetActiveAsync(int? memberId = null)
    {
        var query = _db.Loans.AsNoTracking().Where(l => l.ReturnedAt == null);

        if (memberId is not null) query = query.Where(l => l.MemberId == memberId);

        return await query.OrderBy(l => l.DueDate).Select(ToDto).ToListAsync();
    }

    public async Task<List<LoanDto>> GetOverdueAsync()
    {
        var now = DateTime.UtcNow;

        return await _db.Loans.AsNoTracking()
            .Where(l => l.ReturnedAt == null && l.DueDate < now)
            .OrderBy(l => l.DueDate)
            .Select(ToDto)
            .ToListAsync();
    }

    public async Task<ServiceResult<LoanDto>> BorrowAsync(BorrowRequest request)
    {
        if (!await _db.Members.AnyAsync(m => m.Id == request.MemberId))
            return ServiceResult<LoanDto>.NotFound("Member not found.");

        var totalCopies = await _db.Books
            .Where(b => b.Id == request.BookId)
            .Select(b => (int?)b.TotalCopies)
            .FirstOrDefaultAsync();

        if (totalCopies is null)
            return ServiceResult<LoanDto>.NotFound("Book not found.");

        // Rule 1: a member can have at most 3 active loans
        var memberActive = await _db.Loans
            .CountAsync(l => l.MemberId == request.MemberId && l.ReturnedAt == null);
        if (memberActive >= MaxActiveLoans)
            return ServiceResult<LoanDto>.Conflict(
                $"The member already has {MaxActiveLoans} active loans.");

        // Rule 2: active loans for the book must stay below its total copies
        var bookActive = await _db.Loans
            .CountAsync(l => l.BookId == request.BookId && l.ReturnedAt == null);
        if (bookActive >= totalCopies.Value)
            return ServiceResult<LoanDto>.Conflict("No copies of this book are available.");

        // Rule 3: due date is the loan date plus 14 days
        var now = DateTime.UtcNow;
        var loan = new Loan
        {
            MemberId = request.MemberId,
            BookId = request.BookId,
            LoanedAt = now,
            DueDate = now.AddDays(LoanDays)
        };

        _db.Loans.Add(loan);
        await _db.SaveChangesAsync();

        return ServiceResult<LoanDto>.Ok((await GetByIdAsync(loan.Id))!);
    }

    // Rule 4: returning a book sets ReturnedAt. This is the "update" for a loan.
    public async Task<ServiceResult<LoanDto>> ReturnAsync(int id)
    {
        var loan = await _db.Loans.FindAsync(id);
        if (loan is null)
            return ServiceResult<LoanDto>.NotFound("Loan not found.");

        if (loan.ReturnedAt is not null)
            return ServiceResult<LoanDto>.Conflict("This loan has already been returned.");

        loan.ReturnedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return ServiceResult<LoanDto>.Ok((await GetByIdAsync(id))!);
    }
}
