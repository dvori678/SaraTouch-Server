using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaraTouch.Core.Exceptions
{
    public class ConcurrencyConflictException:Exception
    {
        public ConcurrencyConflictException(string message, Exception? inner = null)
            : base(message, inner) { }
    }
}
