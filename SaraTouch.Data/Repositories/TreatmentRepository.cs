using Microsoft.EntityFrameworkCore;
using SaraTouch.Core.Common;
using SaraTouch.Core.DTOs.Treatments;
using SaraTouch.Core.Entities;
using SaraTouch.Core.Enums;
using SaraTouch.Core.Interfaces.Repositories;
using SaraTouch.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaraTouch.Data.Repositories
{
    public class TreatmentRepository : ITreatmentRepository
    {
        private readonly AppDbContext _context;

        public TreatmentRepository(AppDbContext context) => _context = context;

        public Task<PagedResult<Treatment>> GetPagedAsync(TreatmentQuery query, CancellationToken ct = default)
        {
            IQueryable<Treatment> q = _context.Treatments
                .AsNoTracking()
                .Include(t => t.Tags)
                .Where(t => t.IsActive);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim().ToLower();
                q = q.Where(t => t.Name.ToLower().Contains(search) ||
                                 t.Description.ToLower().Contains(search));
            }

            if (query.TagId.HasValue)
                q = q.Where(t => t.Tags.Any(g => g.Id == query.TagId.Value));

            q = (query.SortBy?.ToLower(), query.Descending) switch
            {
                ("price", false) => q.OrderBy(t => t.Price),
                ("price", true) => q.OrderByDescending(t => t.Price),
                (_, true) => q.OrderByDescending(t => t.Name),
                _ => q.OrderBy(t => t.Name)
            };

            return q.ToPagedResultAsync(query.Page, query.PageSize, ct);
        }

        public Task<Treatment?> GetByIdAsync(int id, CancellationToken ct = default) =>
            _context.Treatments.AsNoTracking()
                .Include(t => t.Tags)
                .FirstOrDefaultAsync(t => t.Id == id, ct);

        public Task<Treatment?> GetForUpdateAsync(int id, CancellationToken ct = default) =>
            _context.Treatments
                .Include(t => t.Tags)
                .FirstOrDefaultAsync(t => t.Id == id, ct);

        public Task<bool> HasFutureAppointmentsAsync(int treatmentId, DateTime nowUtc, CancellationToken ct = default) =>
            _context.Appointments.AnyAsync(a =>
                a.TreatmentId == treatmentId &&
                a.Status == AppointmentStatus.Confirmed &&
                a.TimeSlot.StartUtc > nowUtc, ct);

        public void Add(Treatment treatment) => _context.Treatments.Add(treatment);

        public void Remove(Treatment treatment) => _context.Treatments.Remove(treatment);
    }
}
