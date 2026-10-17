using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaraTouch.API.Models;
using SaraTouch.Core.DTOs.Tags;
using SaraTouch.Core.Interfaces.Services;

namespace SaraTouch.API.Controllers;

[Route("api/tags")]
public class TagsController : ApiControllerBase
{
    private readonly ITagService _tags;

    public TagsController(ITagService tags) => _tags = tags;

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<TagResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TagResponse>>> GetAll(CancellationToken ct) =>
        Ok(await _tags.GetAllAsync(ct));

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TagResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TagResponse>> GetById([FromRoute] int id, CancellationToken ct)
    {
        var result = await _tags.GetByIdAsync(id, ct);
        return result.IsSuccess ? Ok(result.Value) : FromFailure(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(TagResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TagResponse>> Create([FromBody] TagRequest request, CancellationToken ct)
    {
        var result = await _tags.CreateAsync(request, ct);
        return result.IsSuccess
? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
: FromFailure(result);
    }
}