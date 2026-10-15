using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaraTouch.Core.Common
{
    public enum ErrorType
    {
        None = 0,
        Validation = 1,   // 400
        Unauthorized = 2, // 401
        Forbidden = 3,    // 403
        NotFound = 4,     // 404
        Conflict = 5      // 409
    }
}
