using Entity_Framework_Practice.Contracts.DTO;
using Entity_Framework_Practice.Contracts.Requests;
using Entity_Framework_Practice.Data;
using Entity_Framework_Practice.Models;
using Entity_Framework_Practice.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Entity_Framework_Practice.Services;

public class AuthorService : IAuthorService
{
    private readonly LibraryDbContext _db;
    public AuthorService(LibraryDbContext db) => _db = db;
    
    private static AuthorDTO ToDto(Author a) =>
        new() { Id = a.Id, Name = a.Name, Country = a.Country };
    public async Task<List<AuthorDTO>> GetAllAsync()
    {
        var authors = await _db.Authors.AsNoTracking().ToListAsync();
        return authors.Select(ToDto).ToList();
    }

    public async Task<AuthorDTO?> GetByIdAsync(int id)
    {
        var author = await _db.Authors.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
        return author is null ? null : ToDto(author);
    }

    public async Task<AuthorDTO> CreateAsync(AuthorRequest request)
    {
        var author = new Author{ Name = request.Name, Country = request.Country };
        _db.Authors.Add(author);
        await _db.SaveChangesAsync();
        return ToDto(author);
    }

    public async Task<AuthorDTO?> UpdateAsync(int id, AuthorRequest request)
    {
        var author = await _db.Authors.FindAsync(id);
        if (author is null)
        {
            return null;
        }

        author.Name = request.Name;
        author.Country = request.Country;
        await _db.SaveChangesAsync();
        return ToDto(author);
    }

    public async Task<DeleteResult> DeleteAsync(int id)
    {
        var author = await _db.Authors.FindAsync(id);
        if (author is null) return DeleteResult.NotFound;


        if (await _db.Books.AnyAsync(b => b.AuthorId == id))
        {
            return DeleteResult.Blocked;
        }

        _db.Authors.Remove(author);
        await _db.SaveChangesAsync();
        return DeleteResult.Deleted;
    }
}