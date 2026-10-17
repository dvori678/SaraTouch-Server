using SaraTouch.Core.Common;
using SaraTouch.Core.DTOs.TimeSlots;
using SaraTouch.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaraTouch.Core.Interfaces.Repositories
{
    public interface ITimeSlotRepository
    {
        Task<PagedResult<TimeSlot>> GetPagedAsync(TimeSlotQuery query, CancellationToken ct = default);
        Task<TimeSlot?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<TimeSlot?> GetForUpdateAsync(int id, CancellationToken ct = default);
        Task<bool> OverlapsAsync(DateTime startUtc, DateTime endUtc, CancellationToken ct = default);
        void Add(TimeSlot slot);
    }
}
