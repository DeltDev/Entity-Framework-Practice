using System.Linq.Expressions;
using Entity_Framework_Practice.Data;
using Entity_Framework_Practice.DTOs;
using Entity_Framework_Practice.Models;
using Microsoft.EntityFrameworkCore;

namespace Entity_Framework_Practice.Services;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync();
    Task<CategoryDto?> GetByIdAsync(int id);
    Task<ServiceResult<CategoryDto>> CreateAsync(CategoryRequest request);
    Task<ServiceResult<CategoryDto>> UpdateAsync(int id, CategoryRequest request);
    Task<ServiceResult> DeleteAsync(int id);
}

public class CategoryService : ICategoryService
{
    private readonly LibraryDbContext _db;
    public CategoryService(LibraryDbContext db) => _db = db;

    // A projection lets EF compute BookCount in SQL, so no Include is needed
    private static readonly Expression<Func<Category, CategoryDto>> ToDto = c => new CategoryDto
    {
        Id = c.Id,
        Name = c.Name,
        BookCount = c.Books.Count
    };

    public Task<List<CategoryDto>> GetAllAsync() =>
        _db.Categories.AsNoTracking().OrderBy(c => c.Name).Select(ToDto).ToListAsync();

    public Task<CategoryDto?> GetByIdAsync(int id) =>
        _db.Categories.AsNoTracking().Where(c => c.Id == id).Select(ToDto).FirstOrDefaultAsync();

    public async Task<ServiceResult<CategoryDto>> CreateAsync(CategoryRequest request)
    {
        var name = request.Name.Trim();

        if (await _db.Categories.AnyAsync(c => c.Name == name))
            return ServiceResult<CategoryDto>.Conflict($"Category '{name}' already exists.");

        var category = new Category { Name = name };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        return ServiceResult<CategoryDto>.Ok((await GetByIdAsync(category.Id))!);
    }

    public async Task<ServiceResult<CategoryDto>> UpdateAsync(int id, CategoryRequest request)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category is null)
            return ServiceResult<CategoryDto>.NotFound("Category not found.");

        var name = request.Name.Trim();

        // Exclude the category itself, otherwise saving it unchanged would count as a duplicate
        if (await _db.Categories.AnyAsync(c => c.Name == name && c.Id != id))
            return ServiceResult<CategoryDto>.Conflict($"Category '{name}' already exists.");

        category.Name = name;
        await _db.SaveChangesAsync();

        return ServiceResult<CategoryDto>.Ok((await GetByIdAsync(id))!);
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category is null)
            return ServiceResult.NotFound("Category not found.");

        // Checked here for a clear message. The Restrict delete behavior is the safety net.
        if (await _db.Books.AnyAsync(b => b.CategoryId == id))
            return ServiceResult.Conflict("This category still has books.");

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        return ServiceResult.Ok();
    }
}
