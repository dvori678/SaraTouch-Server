using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaraTouch.API.Models;
using SaraTouch.Core.Common;
using SaraTouch.Core.DTOs.Treatments;
using SaraTouch.Core.Interfaces.Services;

namespace SaraTouch.API.Controllers;

[Route("api/treatments")]
public class TreatmentsController : ApiControllerBase
{
    private readonly ITreatmentService _treatments;

    public TreatmentsController(ITreatmentService treatments) => _treatments = treatments;

    /// <summary>רשימת טיפולים עם חיפוש, סינון לפי תגית, מיון ו-pagination</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<TreatmentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<TreatmentResponse>>> GetAll(
        [FromQuery] TreatmentQuery query, CancellationToken ct) =>
        Ok(await _treatments.GetPagedAsync(query, ct));

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TreatmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TreatmentResponse>> GetById([FromRoute] int id, CancellationToken ct) =>
        OkOrFailure(await _treatments.GetByIdAsync(id, ct));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(TreatmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TreatmentResponse>> Create([FromBody] TreatmentRequest request, CancellationToken ct)
    {
        var result = await _treatments.CreateAsync(request, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : FromFailure(result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Update([FromRoute] int id, [FromBody] TreatmentRequest request, CancellationToken ct) =>
        NoContentOrFailure(await _treatments.UpdateAsync(id, request, ct));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Delete([FromRoute] int id, CancellationToken ct) =>
        NoContentOrFailure(await _treatments.DeleteAsync(id, ct));
}