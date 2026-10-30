using Entity_Framework_Practice.Contracts.DTO;
using Entity_Framework_Practice.Contracts.Requests;
using Entity_Framework_Practice.Models;
using Entity_Framework_Practice.Services;
using Entity_Framework_Practice.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Entity_Framework_Practice.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthorController : ControllerBase
{
    private readonly IAuthorService _service;

    public AuthorController(IAuthorService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuthorDTO>>> GetAllAuthors()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AuthorDTO>> GetAuthorById(int id)
    {
        var author = await _service.GetByIdAsync(id);
        return author is null ? NotFound() : Ok(author);
    }

    [HttpPost]
    public async Task<ActionResult<AuthorDTO>> CreateAuthor(AuthorRequest authorRequest)
    {
        var created = await _service.CreateAsync(authorRequest);
        return CreatedAtAction(nameof(GetAuthorById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AuthorDTO>> UpdateAuthor(int id, AuthorRequest authorRequest)
    {
        var updated = await _service.UpdateAsync(id, authorRequest);
        return updated is null ? NotFound() : Ok(updated);
    }
    
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        return await _service.DeleteAsync(id) switch
        {
            DeleteResult.NotFound => NotFound(),
            DeleteResult.Blocked => Conflict("This author still has books."),
            _ => NoContent()
        };
    }
}