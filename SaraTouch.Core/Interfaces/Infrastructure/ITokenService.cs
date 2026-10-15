using SaraTouch.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaraTouch.Core.Interfaces.Infrastructure
{
    public interface ITokenService
    {
        (string Token, DateTime ExpiresAtUtc) CreateToken(User user);

    }
}
