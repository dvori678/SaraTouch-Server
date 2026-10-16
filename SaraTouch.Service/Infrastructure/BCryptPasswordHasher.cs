using SaraTouch.Core.Interfaces.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaraTouch.Service.Infrastructure
{
    public class BCryptPasswordHasher : IPasswordHasher
    {
        // BCrypt מוסיף salt אקראי לכל סיסמה, ולכן שתי סיסמאות זהות יקבלו hash שונה
        public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);

        public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
