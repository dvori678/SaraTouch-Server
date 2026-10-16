using AutoMapper;
using Microsoft.Extensions.Logging;
using SaraTouch.Core.Common;
using SaraTouch.Core.DTOs.Auth;
using SaraTouch.Core.Entities;
using SaraTouch.Core.Enums;
using SaraTouch.Core.Interfaces.Infrastructure;
using SaraTouch.Core.Interfaces.Repositories;
using SaraTouch.Core.Interfaces.Services;

namespace SaraTouch.Service.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokens;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserRepository users, IUnitOfWork unitOfWork, IPasswordHasher hasher,
        ITokenService tokens, IMapper mapper, ILogger<AuthService> logger)
    {
        _users = users;
        _unitOfWork = unitOfWork;
        _hasher = hasher;
        _tokens = tokens;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _users.EmailExistsAsync(email, ct))
            return Result<AuthResponse>.Conflict("כתובת המייל כבר רשומה במערכת");

        var user = _mapper.Map<User>(request);
        user.Email = email;
        user.FullName = request.FullName.Trim();
        user.PasswordHash = _hasher.Hash(request.Password);
        user.Role = Role.Client;   // הרשמה עצמית תמיד יוצרת לקוחה, אף פעם לא Admin

        _users.Add(user);
        await _unitOfWork.SaveChangesAsync(ct);

        // בלי מייל, בלי סיסמה ובלי token: לוג הוא קובץ שאנשים קוראים
        _logger.LogInformation("User {UserId} registered", user.Id);
        return Result<AuthResponse>.Success(BuildResponse(user));
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _users.GetByEmailAsync(email, ct);

        // אותה הודעה למייל לא קיים ולסיסמה שגויה, כדי לא לחשוף אילו מיילים רשומים
        if (user is null || !_hasher.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt");
            return Result<AuthResponse>.Unauthorized("מייל או סיסמה שגויים");
        }

        _logger.LogInformation("User {UserId} logged in", user.Id);
        return Result<AuthResponse>.Success(BuildResponse(user));
    }

    private AuthResponse BuildResponse(User user)
    {
        var (token, expiresAtUtc) = _tokens.CreateToken(user);
        return new AuthResponse
        {
            Token = token,
            ExpiresAtUtc = expiresAtUtc,
            UserId = user.Id,
            FullName = user.FullName,
            Role = user.Role
        };
    }
}