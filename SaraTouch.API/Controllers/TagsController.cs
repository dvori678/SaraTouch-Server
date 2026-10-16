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

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(TagResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TagResponse>> Create([FromBody] TagRequest request, CancellationToken ct)
    {
        var result = await _tags.CreateAsync(request, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetAll), null, result.Value)
            : FromFailure(result);
    }
}