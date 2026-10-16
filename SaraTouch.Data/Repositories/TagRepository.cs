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
    public class TagRepository : ITagRepository
    {
        private readonly AppDbContext _context;

        public TagRepository(AppDbContext context) => _context = context;

        public Task<List<Tag>> GetAllAsync(CancellationToken ct = default) =>
            _context.Tags.AsNoTracking().OrderBy(g => g.Name).ToListAsync(ct);

        public Task<List<Tag>> GetByIdsForUpdateAsync(IEnumerable<int> ids, CancellationToken ct = default)
        {
            var idList = ids.Distinct().ToList();
            return _context.Tags.Where(g => idList.Contains(g.Id)).ToListAsync(ct);
        }

        public Task<bool> NameExistsAsync(string name, CancellationToken ct = default)
        {
            var normalized = name.Trim().ToLower();
            return _context.Tags.AnyAsync(g => g.Name.ToLower() == normalized, ct);
        }

        public void Add(Tag tag) => _context.Tags.Add(tag);
    }
}
