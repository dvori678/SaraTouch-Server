using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaraTouch.API.Models;
using SaraTouch.Core.DTOs.Auth;
using SaraTouch.Core.Interfaces.Services;

namespace SaraTouch.API.Controllers;

[Route("api/tokens")]
public class TokensController : ApiControllerBase
{
    private readonly IAuthService _auth;

    public TokensController(IAuthService auth) => _auth = auth;

    /// <summary>התחברות: מחזירה JWT</summary>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken ct) =>
        OkOrFailure(await _auth.LoginAsync(request, ct));
}