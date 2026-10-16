using Microsoft.EntityFrameworkCore;
using SaraTouch.Core.Common;
using SaraTouch.Core.DTOs.TimeSlots;
using SaraTouch.Core.Entities;
using SaraTouch.Core.Interfaces.Repositories;
using SaraTouch.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaraTouch.Data.Repositories
{
    public class TimeSlotRepository : ITimeSlotRepository
    {
        private readonly AppDbContext _context;

        public TimeSlotRepository(AppDbContext context) => _context = context;

        public Task<PagedResult<TimeSlot>> GetPagedAsync(TimeSlotQuery query, CancellationToken ct = default)
        {
            IQueryable<TimeSlot> q = _context.TimeSlots.AsNoTracking();

            if (query.FromUtc.HasValue)
                q = q.Where(s => s.StartUtc >= query.FromUtc.Value);

            if (query.ToUtc.HasValue)
                q = q.Where(s => s.StartUtc < query.ToUtc.Value);

            if (query.Status.HasValue)
                q = q.Where(s => s.Status == query.Status.Value);

            return q.OrderBy(s => s.StartUtc)
                    .ToPagedResultAsync(query.Page, query.PageSize, ct);
        }

        // tracked: הסלוט נטען יחד עם ה-Version הנוכחי, ו-EF ישים אותו ב-WHERE בשמירה
        public Task<TimeSlot?> GetForUpdateAsync(int id, CancellationToken ct = default) =>
            _context.TimeSlots.FirstOrDefaultAsync(s => s.Id == id, ct);

        // שני טווחים חופפים אם כל אחד מתחיל לפני שהשני נגמר
        public Task<bool> OverlapsAsync(DateTime startUtc, DateTime endUtc, CancellationToken ct = default) =>
            _context.TimeSlots.AnyAsync(s => s.StartUtc < endUtc && startUtc < s.EndUtc, ct);

        public void Add(TimeSlot slot) => _context.TimeSlots.Add(slot);
    }
}
