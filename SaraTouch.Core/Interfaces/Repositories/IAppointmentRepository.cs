using SaraTouch.Core.Common;
using SaraTouch.Core.DTOs.Appointments;
using SaraTouch.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaraTouch.Core.Interfaces.Repositories
{
    public interface IAppointmentRepository
    {
        Task<PagedResult<Appointment>> GetPagedAsync(AppointmentQuery query, int? userId, CancellationToken ct = default);
        Task<Appointment?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Appointment?> GetForUpdateAsync(int id, CancellationToken ct = default);
        void Add(Appointment appointment);
    }
}
