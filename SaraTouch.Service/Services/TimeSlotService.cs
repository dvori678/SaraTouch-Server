using AutoMapper;
using Microsoft.Extensions.Logging;
using SaraTouch.Core.Common;
using SaraTouch.Core.DTOs.TimeSlots;
using SaraTouch.Core.Entities;
using SaraTouch.Core.Enums;
using SaraTouch.Core.Exceptions;
using SaraTouch.Core.Interfaces.Infrastructure;
using SaraTouch.Core.Interfaces.Repositories;
using SaraTouch.Core.Interfaces.Services;
using SaraTouch.Service.Common;
using SaraTouch.Service.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaraTouch.Service.Services
{
    public class TimeSlotService : ITimeSlotService
    {
        private readonly ITimeSlotRepository _slots;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IClock _clock;
        private readonly ILogger<TimeSlotService> _logger;

        public TimeSlotService(ITimeSlotRepository slots, IUnitOfWork unitOfWork, IMapper mapper,
            IClock clock, ILogger<TimeSlotService> logger)
        {
            _slots = slots;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _clock = clock;
            _logger = logger;
        }

        public async Task<PagedResult<TimeSlotResponse>> GetPagedAsync(TimeSlotQuery query, CancellationToken ct = default)
        {
            query.FromUtc = query.FromUtc.AsUtc();
            query.ToUtc = query.ToUtc.AsUtc();

            var page = await _slots.GetPagedAsync(query, ct);
            return page.MapItems<TimeSlot, TimeSlotResponse>(_mapper);
        }

        public async Task<Result<TimeSlotResponse>> CreateAsync(CreateTimeSlotRequest request, CancellationToken ct = default)
        {
            var start = request.StartUtc!.Value.AsUtc();
            var end = start.AddMinutes(request.DurationMinutes);

            if (start <= _clock.UtcNow)
                return Result<TimeSlotResponse>.Validation("לא ניתן לפתוח סלוט בזמן שכבר עבר");

            if (await _slots.OverlapsAsync(start, end, ct))
                return Result<TimeSlotResponse>.Conflict("הסלוט חופף לסלוט קיים ביומן");

            var slot = new TimeSlot { StartUtc = start, EndUtc = end, Status = SlotStatus.Available };
            _slots.Add(slot);
            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("Time slot {SlotId} created for {StartUtc}", slot.Id, start);
            return Result<TimeSlotResponse>.Success(_mapper.Map<TimeSlotResponse>(slot));
        }

        // שרה חוסמת סלוט או פותחת אותו מחדש
        public async Task<Result> UpdateStatusAsync(int id, UpdateTimeSlotRequest request, CancellationToken ct = default)
        {
            var slot = await _slots.GetForUpdateAsync(id, ct);
            if (slot is null)
                return Result.NotFound("הסלוט לא נמצא");

            var target = request.Status!.Value;

            if (target == SlotStatus.Booked)
                return Result.Validation("סלוט נתפס רק דרך קביעת תור, לא ידנית");

            if (slot.Status == SlotStatus.Booked)
                return Result.Conflict("יש תור על הסלוט הזה. צריך לבטל אותו קודם");

            slot.Status = target;

            try
            {
                await _unitOfWork.SaveChangesAsync(ct);
            }
            catch (ConcurrencyConflictException)
            {
                // לקוחה קבעה תור בסלוט בדיוק בין הקריאה לשמירה
                _logger.LogWarning("Concurrency conflict while updating slot {SlotId} status", id);
                return Result.Conflict("הסלוט עודכן בדיוק עכשיו על ידי משתמש אחר. רענני ונסי שוב");
            }

            _logger.LogInformation("Slot {SlotId} status changed to {Status}", id, target);
            return Result.Success();
        }
    }
}
