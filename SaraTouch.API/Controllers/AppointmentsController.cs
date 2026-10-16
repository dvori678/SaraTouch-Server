using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaraTouch.API.Models;
using SaraTouch.Core.Common;
using SaraTouch.Core.DTOs.Appointments;
using SaraTouch.Core.Interfaces.Services;

namespace SaraTouch.API.Controllers;

[Route("api/appointments")]
[Authorize]
public class AppointmentsController : ApiControllerBase
{
    private readonly IAppointmentService _appointments;

    public AppointmentsController(IAppointmentService appointments) => _appointments = appointments;

    /// <summary>שרה רואה את כל התורים, לקוחה רואה רק את שלה</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AppointmentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AppointmentResponse>>> GetAll(
        [FromQuery] AppointmentQuery query, CancellationToken ct) =>
        Ok(await _appointments.GetPagedAsync(query, CurrentUserId, IsAdmin, ct));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AppointmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AppointmentResponse>> GetById([FromRoute] int id, CancellationToken ct) =>
        OkOrFailure(await _appointments.GetByIdAsync(id, CurrentUserId, IsAdmin, ct));

    /// <summary>קביעת תור: המשאב המוגבל. 409 אם הסלוט נתפס</summary>
    [HttpPost]
    [Authorize(Roles = "Client")]
    [ProducesResponseType(typeof(AppointmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AppointmentResponse>> Book([FromBody] CreateAppointmentRequest request, CancellationToken ct)
    {
        var result = await _appointments.BookAsync(request, CurrentUserId, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : FromFailure(result);
    }

    /// <summary>ביטול תור: משחרר את הסלוט</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Cancel([FromRoute] int id, CancellationToken ct) =>
        NoContentOrFailure(await _appointments.CancelAsync(id, CurrentUserId, IsAdmin, ct));

    /// <summary>שרה מסמנת תור כהושלם</summary>
    [HttpPatch("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult> UpdateStatus([FromRoute] int id, [FromBody] UpdateAppointmentRequest request, CancellationToken ct) =>
        NoContentOrFailure(await _appointments.UpdateStatusAsync(id, request, ct));
}