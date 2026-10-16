using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaraTouch.API.Models;
using SaraTouch.Core.DTOs.Auth;
using SaraTouch.Core.Enums;
using SaraTouch.Core.Interfaces.Services;

namespace SaraTouch.API.Controllers;

[Route("api/users")]
public class UsersController : ApiControllerBase
{
    private readonly IAuthService _auth;

    public UsersController(IAuthService auth) => _auth = auth;

    /// <summary>הרשמה של לקוחה חדשה</summary>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await _auth.RegisterAsync(request, ct);
        if (!result.IsSuccess)
            return FromFailure(result);

        // 201 + כותרת Location שמצביעה על /api/users/me
        return CreatedAtAction(nameof(Me), null, result.Value);
    }

    /// <summary>פרטי המשתמשת המחוברת, לפי ה-token</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(CurrentUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<CurrentUserResponse> Me() => Ok(new CurrentUserResponse
    {
        Id = CurrentUserId,
        FullName = User.FindFirstValue(ClaimTypes.Name) ?? string.Empty,
        Email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
        Role = Enum.Parse<Role>(User.FindFirstValue(ClaimTypes.Role)!)
    });
}