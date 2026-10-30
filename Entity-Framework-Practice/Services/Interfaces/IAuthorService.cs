using Entity_Framework_Practice.Contracts.DTO;
using Entity_Framework_Practice.Contracts.Requests;
using Entity_Framework_Practice.Models;

namespace Entity_Framework_Practice.Services.Interfaces;

public interface IAuthorService
{
    Task<List<AuthorDTO>> GetAllAsync();
    Task<AuthorDTO?> GetByIdAsync(int id);
    Task<AuthorDTO> CreateAsync(AuthorRequest request);
    Task<AuthorDTO?> UpdateAsync(int id, AuthorRequest request);
    Task<DeleteResult> DeleteAsync(int id);
}