using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaraTouch.API.Models;
using SaraTouch.Core.Common;
using SaraTouch.Core.DTOs.TimeSlots;
using SaraTouch.Core.Interfaces.Services;

namespace SaraTouch.API.Controllers;

[Route("api/timeslots")]
public class TimeSlotsController : ApiControllerBase
{
    private readonly ITimeSlotService _slots;

    public TimeSlotsController(ITimeSlotService slots) => _slots = slots;

    /// <summary>סלוטים ביומן. למשל ?status=Available&amp;fromUtc=2026-10-20</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<TimeSlotResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<TimeSlotResponse>>> GetAll(
        [FromQuery] TimeSlotQuery query, CancellationToken ct) =>
        Ok(await _slots.GetPagedAsync(query, ct));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(TimeSlotResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TimeSlotResponse>> Create([FromBody] CreateTimeSlotRequest request, CancellationToken ct)
    {
        var result = await _slots.CreateAsync(request, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetAll), null, result.Value)
            : FromFailure(result);
    }

    /// <summary>שרה חוסמת סלוט (Blocked) או פותחת אותו (Available)</summary>
    [HttpPatch("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult> UpdateStatus([FromRoute] int id, [FromBody] UpdateTimeSlotRequest request, CancellationToken ct) =>
        NoContentOrFailure(await _slots.UpdateStatusAsync(id, request, ct));
}