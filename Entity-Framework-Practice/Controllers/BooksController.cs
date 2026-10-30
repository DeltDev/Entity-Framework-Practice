using Entity_Framework_Practice.DTOs;
using Entity_Framework_Practice.Services;
using Microsoft.AspNetCore.Mvc;

namespace Entity_Framework_Practice.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookService _service;
    public BooksController(IBookService service) => _service = service;

    // Optional filters: GET /api/books?authorId=2&categoryId=1
    [HttpGet]
    public async Task<ActionResult<List<BookDto>>> GetAll(int? authorId, int? categoryId)
        => Ok(await _service.GetAllAsync(authorId, categoryId));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookDto>> GetById(int id)
    {
        var book = await _service.GetByIdAsync(id);
        return book is null ? NotFound() : Ok(book);
    }

    [HttpPost]
    public async Task<ActionResult<BookDto>> Create(BookRequest request)
    {
        var result = await _service.CreateAsync(request);
        if (!result.IsSuccess) return this.ToProblem(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<BookDto>> Update(int id, BookRequest request)
    {
        var result = await _service.UpdateAsync(id, request);
        if (!result.IsSuccess) return this.ToProblem(result);

        return Ok(result.Value);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result.IsSuccess) return this.ToProblem(result);

        return NoContent();
    }
}
