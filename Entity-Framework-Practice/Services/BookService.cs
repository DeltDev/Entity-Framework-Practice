using System.Linq.Expressions;
using Entity_Framework_Practice.Data;
using Entity_Framework_Practice.DTOs;
using Entity_Framework_Practice.Models;
using Microsoft.EntityFrameworkCore;

namespace Entity_Framework_Practice.Services;

public interface IBookService
{
    Task<List<BookDto>> GetAllAsync(int? authorId = null, int? categoryId = null);
    Task<BookDto?> GetByIdAsync(int id);
    Task<ServiceResult<BookDto>> CreateAsync(BookRequest request);
    Task<ServiceResult<BookDto>> UpdateAsync(int id, BookRequest request);
    Task<ServiceResult> DeleteAsync(int id);
}

public class BookService : IBookService
{
    private readonly LibraryDbContext _db;
    public BookService(LibraryDbContext db) => _db = db;

    // Author and category names, plus available copies, are all computed in the SQL query
    private static readonly Expression<Func<Book, BookDto>> ToDto = b => new BookDto
    {
        Id = b.Id,
        Title = b.Title,
        ISBN = b.ISBN,
        PublishDate = b.PublishDate,
        TotalCopies = b.TotalCopies,
        AvailableCopies = b.TotalCopies - b.Loans.Count(l => l.ReturnedAt == null),
        AuthorId = b.AuthorId,
        AuthorName = b.Author.Name,
        CategoryId = b.CategoryId,
        CategoryName = b.Category.Name
    };

    public async Task<List<BookDto>> GetAllAsync(int? authorId = null, int? categoryId = null)
    {
        var query = _db.Books.AsNoTracking();

        if (authorId is not null) query = query.Where(b => b.AuthorId == authorId);
        if (categoryId is not null) query = query.Where(b => b.CategoryId == categoryId);

        return await query.OrderBy(b => b.Title).Select(ToDto).ToListAsync();
    }

    public Task<BookDto?> GetByIdAsync(int id) =>
        _db.Books.AsNoTracking().Where(b => b.Id == id).Select(ToDto).FirstOrDefaultAsync();

    public async Task<ServiceResult<BookDto>> CreateAsync(BookRequest request)
    {
        var referenceError = await CheckReferencesAsync(request);
        if (referenceError is not null)
            return ServiceResult<BookDto>.Invalid(referenceError);

        var isbn = request.ISBN.Trim();
        if (await _db.Books.AnyAsync(b => b.ISBN == isbn))
            return ServiceResult<BookDto>.Conflict($"A book with ISBN '{isbn}' already exists.");

        var book = new Book
        {
            Title = request.Title.Trim(),
            ISBN = isbn,
            PublishDate = request.PublishDate,
            TotalCopies = request.TotalCopies,
            AuthorId = request.AuthorId,
            CategoryId = request.CategoryId
        };

        _db.Books.Add(book);
        await _db.SaveChangesAsync();

        return ServiceResult<BookDto>.Ok((await GetByIdAsync(book.Id))!);
    }

    public async Task<ServiceResult<BookDto>> UpdateAsync(int id, BookRequest request)
    {
        var book = await _db.Books.FindAsync(id);
        if (book is null)
            return ServiceResult<BookDto>.NotFound("Book not found.");

        var referenceError = await CheckReferencesAsync(request);
        if (referenceError is not null)
            return ServiceResult<BookDto>.Invalid(referenceError);

        var isbn = request.ISBN.Trim();
        if (await _db.Books.AnyAsync(b => b.ISBN == isbn && b.Id != id))
            return ServiceResult<BookDto>.Conflict($"A book with ISBN '{isbn}' already exists.");

        // Total copies cannot drop below the copies that are currently out on loan
        var activeLoans = await _db.Loans.CountAsync(l => l.BookId == id && l.ReturnedAt == null);
        if (request.TotalCopies < activeLoans)
            return ServiceResult<BookDto>.Conflict(
                $"{activeLoans} copies are currently on loan, so total copies cannot be lower than that.");

        book.Title = request.Title.Trim();
        book.ISBN = isbn;
        book.PublishDate = request.PublishDate;
        book.TotalCopies = request.TotalCopies;
        book.AuthorId = request.AuthorId;
        book.CategoryId = request.CategoryId;
        await _db.SaveChangesAsync();

        return ServiceResult<BookDto>.Ok((await GetByIdAsync(id))!);
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var book = await _db.Books.FindAsync(id);
        if (book is null)
            return ServiceResult.NotFound("Book not found.");

        // Any loan row, returned or not, counts as history that must be kept
        if (await _db.Loans.AnyAsync(l => l.BookId == id))
            return ServiceResult.Conflict("This book has loan history and cannot be deleted.");

        _db.Books.Remove(book);
        await _db.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    // Returns an error message, or null when the author and category both exist
    private async Task<string?> CheckReferencesAsync(BookRequest request)
    {
        if (!await _db.Authors.AnyAsync(a => a.Id == request.AuthorId))
            return "Author not found.";

        if (!await _db.Categories.AnyAsync(c => c.Id == request.CategoryId))
            return "Category not found.";

        return null;
    }
}
