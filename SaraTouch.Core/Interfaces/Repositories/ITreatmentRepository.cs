using SaraTouch.Core.Common;
using SaraTouch.Core.DTOs.Treatments;
using SaraTouch.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaraTouch.Core.Interfaces.Repositories
{
    public interface ITreatmentRepository
    {
        Task<PagedResult<Treatment>> GetPagedAsync(TreatmentQuery query, CancellationToken ct = default);
        Task<Treatment?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Treatment?> GetForUpdateAsync(int id, CancellationToken ct = default);
        Task<bool> HasFutureAppointmentsAsync(int treatmentId, DateTime nowUtc, CancellationToken ct = default);
        void Add(Treatment treatment);
        void Remove(Treatment treatment);
    }
}
