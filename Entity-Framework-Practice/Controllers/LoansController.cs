using Entity_Framework_Practice.DTOs;
using Entity_Framework_Practice.Services;
using Microsoft.AspNetCore.Mvc;

namespace Entity_Framework_Practice.Controllers;

// There is no DELETE here on purpose: loans are history. Borrow creates one, return updates it.
[ApiController]
[Route("api/[controller]")]
public class LoansController : ControllerBase
{
    private readonly ILoanService _service;
    public LoansController(ILoanService service) => _service = service;

    // GET /api/loans?memberId=2&bookId=1 (both filters optional)
    [HttpGet]
    public async Task<ActionResult<List<LoanDto>>> GetAll(int? memberId, int? bookId)
        => Ok(await _service.GetAllAsync(memberId, bookId));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LoanDto>> GetById(int id)
    {
        var loan = await _service.GetByIdAsync(id);
        return loan is null ? NotFound() : Ok(loan);
    }

    // GET /api/loans/active or /api/loans/active?memberId=2
    [HttpGet("active")]
    public async Task<ActionResult<List<LoanDto>>> GetActive(int? memberId)
        => Ok(await _service.GetActiveAsync(memberId));

    [HttpGet("overdue")]
    public async Task<ActionResult<List<LoanDto>>> GetOverdue()
        => Ok(await _service.GetOverdueAsync());

    // Create: borrow a book
    [HttpPost]
    public async Task<ActionResult<LoanDto>> Borrow(BorrowRequest request)
    {
        var result = await _service.BorrowAsync(request);
        if (!result.IsSuccess) return this.ToProblem(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    // Update: return a book
    [HttpPost("{id:int}/return")]
    public async Task<ActionResult<LoanDto>> Return(int id)
    {
        var result = await _service.ReturnAsync(id);
        if (!result.IsSuccess) return this.ToProblem(result);

        return Ok(result.Value);
    }
}
