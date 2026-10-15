using SaraTouch.Core.Common;
using SaraTouch.Core.DTOs.TimeSlots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaraTouch.Core.Interfaces.Services
{
    public interface ITimeSlotService
    {
        Task<PagedResult<TimeSlotResponse>> GetPagedAsync(TimeSlotQuery query, CancellationToken ct = default);
        Task<Result<TimeSlotResponse>> CreateAsync(CreateTimeSlotRequest request, CancellationToken ct = default);
        Task<Result> UpdateStatusAsync(int id, UpdateTimeSlotRequest request, CancellationToken ct = default);
    }
}
