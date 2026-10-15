using SaraTouch.Core.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaraTouch.Core.Interfaces.Services
{
    public interface IAuthService
    {
        Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
        Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default);
    }
}
