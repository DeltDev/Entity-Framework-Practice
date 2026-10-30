using Entity_Framework_Practice.DTOs;
using Entity_Framework_Practice.Services;
using Microsoft.AspNetCore.Mvc;

namespace Entity_Framework_Practice.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MembersController : ControllerBase
{
    private readonly IMemberService _service;
    public MembersController(IMemberService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<List<MemberDto>>> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MemberDto>> GetById(int id)
    {
        var member = await _service.GetByIdAsync(id);
        return member is null ? NotFound() : Ok(member);
    }

    // Creates the member and issues their card in one request
    [HttpPost]
    public async Task<ActionResult<MemberDto>> Create(CreateMemberRequest request)
    {
        var result = await _service.CreateAsync(request);
        if (!result.IsSuccess) return this.ToProblem(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<MemberDto>> Update(int id, UpdateMemberRequest request)
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
