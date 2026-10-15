using SaraTouch.Core.Common;
using SaraTouch.Core.DTOs.Appointments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaraTouch.Core.Interfaces.Services
{
    public interface IAppointmentService
    {
        Task<PagedResult<AppointmentResponse>> GetPagedAsync(AppointmentQuery query, int currentUserId, bool isAdmin, CancellationToken ct = default);
        Task<Result<AppointmentResponse>> GetByIdAsync(int id, int currentUserId, bool isAdmin, CancellationToken ct = default);
        Task<Result<AppointmentResponse>> BookAsync(CreateAppointmentRequest request, int currentUserId, CancellationToken ct = default);
        Task<Result> CancelAsync(int id, int currentUserId, bool isAdmin, CancellationToken ct = default);
        Task<Result> UpdateStatusAsync(int id, UpdateAppointmentRequest request, CancellationToken ct = default);
    }
}
