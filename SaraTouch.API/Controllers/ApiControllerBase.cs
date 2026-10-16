using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SaraTouch.API.Models;
using SaraTouch.Core.Common;
using SaraTouch.Core.Enums;

namespace SaraTouch.API.Controllers;

[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    protected int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    protected bool IsAdmin => User.IsInRole(nameof(Role.Admin));

    // Result שנכשל → סטטוס HTTP + תשובת JSON אחידה
    protected ActionResult FromFailure(Result result)
    {
        var (status, title) = result.ErrorType switch
        {
            ErrorType.Validation => (StatusCodes.Status400BadRequest, "Bad Request"),
            ErrorType.Unauthorized => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            ErrorType.Forbidden => (StatusCodes.Status403Forbidden, "Forbidden"),
            ErrorType.NotFound => (StatusCodes.Status404NotFound, "Not Found"),
            ErrorType.Conflict => (StatusCodes.Status409Conflict, "Conflict"),
            _ => (StatusCodes.Status500InternalServerError, "Server Error")
        };

        return StatusCode(status, new ErrorResponse
        {
            Status = status,
            Title = title,
            Message = result.Error ?? title,
            CorrelationId = HttpContext.TraceIdentifier
        });
    }

    protected ActionResult<T> OkOrFailure<T>(Result<T> result) =>
        result.IsSuccess ? Ok(result.Value) : FromFailure(result);

    protected ActionResult NoContentOrFailure(Result result) =>
        result.IsSuccess ? NoContent() : FromFailure(result);
}