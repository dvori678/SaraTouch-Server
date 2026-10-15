using SaraTouch.Core.Common;
using SaraTouch.Core.DTOs.Treatments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaraTouch.Core.Interfaces.Services
{
    public interface ITreatmentService
    {
        Task<PagedResult<TreatmentResponse>> GetPagedAsync(TreatmentQuery query, CancellationToken ct = default);
        Task<Result<TreatmentResponse>> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Result<TreatmentResponse>> CreateAsync(TreatmentRequest request, CancellationToken ct = default);
        Task<Result> UpdateAsync(int id, TreatmentRequest request, CancellationToken ct = default);
        Task<Result> DeleteAsync(int id, CancellationToken ct = default);
    }
}
