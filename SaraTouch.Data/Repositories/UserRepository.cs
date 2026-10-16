using Microsoft.EntityFrameworkCore;
using SaraTouch.Core.Entities;
using SaraTouch.Core.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaraTouch.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context) => _context = context;

        public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
            _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, ct);

        public Task<bool> EmailExistsAsync(string email, CancellationToken ct = default) =>
            _context.Users.AnyAsync(u => u.Email == email, ct);

        public void Add(User user) => _context.Users.Add(user);
    }
}
