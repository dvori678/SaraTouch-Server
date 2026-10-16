using Microsoft.EntityFrameworkCore;
using SaraTouch.Core.Common;
using SaraTouch.Core.DTOs.Appointments;
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
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly AppDbContext _context;

        public AppointmentRepository(AppDbContext context) => _context = context;

        // כל הנתונים הקשורים נטענים בשאילתה אחת, בלי N+1
        private IQueryable<Appointment> WithDetails() =>
            _context.Appointments
                .Include(a => a.User)
                .Include(a => a.TimeSlot)
                .Include(a => a.Treatment)
                    .ThenInclude(t => t.Tags);

        public Task<PagedResult<Appointment>> GetPagedAsync(AppointmentQuery query, int? userId, CancellationToken ct = default)
        {
            var q = WithDetails().AsNoTracking();

            if (userId.HasValue)
                q = q.Where(a => a.UserId == userId.Value);

            if (query.FromUtc.HasValue)
                q = q.Where(a => a.TimeSlot.StartUtc >= query.FromUtc.Value);

            if (query.ToUtc.HasValue)
                q = q.Where(a => a.TimeSlot.StartUtc < query.ToUtc.Value);

            if (query.Status.HasValue)
                q = q.Where(a => a.Status == query.Status.Value);

            return q.OrderByDescending(a => a.TimeSlot.StartUtc)
                    .ToPagedResultAsync(query.Page, query.PageSize, ct);
        }

        public Task<Appointment?> GetByIdAsync(int id, CancellationToken ct = default) =>
            WithDetails().AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, ct);

        // tracked יחד עם הסלוט, כי בביטול משחררים גם אותו
        public Task<Appointment?> GetForUpdateAsync(int id, CancellationToken ct = default) =>
            _context.Appointments
                .Include(a => a.TimeSlot)
                .FirstOrDefaultAsync(a => a.Id == id, ct);

        public void Add(Appointment appointment) => _context.Appointments.Add(appointment);
    }
}
