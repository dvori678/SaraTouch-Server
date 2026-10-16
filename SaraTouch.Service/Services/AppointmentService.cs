using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SaraTouch.Core.Common;
using SaraTouch.Core.DTOs.Appointments;
using SaraTouch.Core.Entities;
using SaraTouch.Core.Enums;
using SaraTouch.Core.Exceptions;
using SaraTouch.Core.Interfaces.Infrastructure;
using SaraTouch.Core.Interfaces.Repositories;
using SaraTouch.Core.Interfaces.Services;
using SaraTouch.Core.Settings;
using SaraTouch.Service.Common;
using SaraTouch.Service.Mapping;

namespace SaraTouch.Service.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointments;
    private readonly ITimeSlotRepository _slots;
    private readonly ITreatmentRepository _treatments;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IClock _clock;
    private readonly BookingSettings _settings;
    private readonly ILogger<AppointmentService> _logger;

    public AppointmentService(
        IAppointmentRepository appointments,
        ITimeSlotRepository slots,
        ITreatmentRepository treatments,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IClock clock,
        IOptions<BookingSettings> settings,
        ILogger<AppointmentService> logger)
    {
        _appointments = appointments;
        _slots = slots;
        _treatments = treatments;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _clock = clock;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<PagedResult<AppointmentResponse>> GetPagedAsync(
        AppointmentQuery query, int currentUserId, bool isAdmin, CancellationToken ct = default)
    {
        query.FromUtc = query.FromUtc.AsUtc();
        query.ToUtc = query.ToUtc.AsUtc();

        // שרה רואה את כל התורים, לקוחה רואה רק את שלה
        var page = await _appointments.GetPagedAsync(query, isAdmin ? null : currentUserId, ct);
        return page.MapItems<Appointment, AppointmentResponse>(_mapper);
    }

    public async Task<Result<AppointmentResponse>> GetByIdAsync(
        int id, int currentUserId, bool isAdmin, CancellationToken ct = default)
    {
        var appointment = await _appointments.GetByIdAsync(id, ct);
        if (appointment is null)
            return Result<AppointmentResponse>.NotFound("התור לא נמצא");

        // הרשאה אמיתית: נבדקת בשרת, לא רק בצד הלקוח
        if (!isAdmin && appointment.UserId != currentUserId)
            return Result<AppointmentResponse>.Forbidden("אין לך הרשאה לצפות בתור הזה");

        return Result<AppointmentResponse>.Success(_mapper.Map<AppointmentResponse>(appointment));
    }

    // ===== קביעת תור: המשאב המוגבל =====
    public async Task<Result<AppointmentResponse>> BookAsync(
        CreateAppointmentRequest request, int currentUserId, CancellationToken ct = default)
    {
        var treatment = await _treatments.GetByIdAsync(request.TreatmentId, ct);
        if (treatment is null || !treatment.IsActive)
            return Result<AppointmentResponse>.NotFound("הטיפול לא נמצא");

        // tracked: הסלוט נטען יחד עם ה-Version הנוכחי שלו
        var slot = await _slots.GetForUpdateAsync(request.TimeSlotId, ct);
        if (slot is null)
            return Result<AppointmentResponse>.NotFound("השעה שנבחרה לא נמצאה");

        if (slot.StartUtc <= _clock.UtcNow)
            return Result<AppointmentResponse>.Validation("לא ניתן לקבוע תור לשעה שכבר עברה");

        // קו הגנה ראשון: בדיקה עסקית
        if (slot.Status != SlotStatus.Available)
        {
            _logger.LogWarning("Booking rejected: slot {SlotId} is {Status} (user {UserId})",
                slot.Id, slot.Status, currentUserId);
            return Result<AppointmentResponse>.Conflict("השעה הזו כבר תפוסה. בחרי שעה אחרת");
        }

        slot.Status = SlotStatus.Booked;

        var appointment = new Appointment
        {
            UserId = currentUserId,
            TreatmentId = treatment.Id,
            TimeSlotId = slot.Id,
            TimeSlot = slot,
            Status = AppointmentStatus.Confirmed,
            CreatedAtUtc = _clock.UtcNow
        };
        _appointments.Add(appointment);

        // קו הגנה שני: SaveChanges אחד, שרץ בטרנזקציה אחת:
        //   UPDATE TimeSlots SET Status='Booked', Version=@new WHERE Id=@id AND Version=@old
        //   INSERT INTO Appointments ...
        // אם ה-UPDATE לא מצא שורה, כל הטרנזקציה מתבטלת ולא נשאר תור "יתום"
        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (ConcurrencyConflictException)
        {
            _logger.LogWarning("Concurrency conflict: slot {SlotId} was booked by another user (user {UserId})",
                slot.Id, currentUserId);
            return Result<AppointmentResponse>.Conflict("השעה נתפסה ממש עכשיו על ידי לקוחה אחרת. בחרי שעה אחרת");
        }

        _logger.LogInformation("Appointment {AppointmentId} booked: slot {SlotId}, user {UserId}",
            appointment.Id, slot.Id, currentUserId);

        // טוענים מחדש עם כל הפרטים (שם הלקוחה, הטיפול והתגיות) בשביל התשובה
        var created = await _appointments.GetByIdAsync(appointment.Id, ct);
        return Result<AppointmentResponse>.Success(_mapper.Map<AppointmentResponse>(created!));
    }

    // ===== ביטול תור: משחרר את הסלוט =====
    public async Task<Result> CancelAsync(int id, int currentUserId, bool isAdmin, CancellationToken ct = default)
    {
        var appointment = await _appointments.GetForUpdateAsync(id, ct);
        if (appointment is null)
            return Result.NotFound("התור לא נמצא");

        if (!isAdmin && appointment.UserId != currentUserId)
            return Result.Forbidden("אין לך הרשאה לבטל תור של לקוחה אחרת");

        if (appointment.Status != AppointmentStatus.Confirmed)
            return Result.Conflict("התור כבר בוטל או הושלם");

        // שרה יכולה לבטל בכל זמן. לקוחה רק עד X שעות לפני התור
        var deadline = TimeSpan.FromHours(_settings.CancellationDeadlineHours);
        if (!isAdmin && appointment.TimeSlot.StartUtc - _clock.UtcNow < deadline)
            return Result.Conflict($"אפשר לבטל עד {_settings.CancellationDeadlineHours} שעות לפני התור");

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.TimeSlot.Status = SlotStatus.Available;   // הסלוט חוזר להיות פנוי

        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (ConcurrencyConflictException)
        {
            _logger.LogWarning("Concurrency conflict while cancelling appointment {AppointmentId}", id);
            return Result.Conflict("התור עודכן במקביל. רענני ונסי שוב");
        }

        _logger.LogInformation("Appointment {AppointmentId} cancelled by user {UserId}", id, currentUserId);
        return Result.Success();
    }

    // ===== שרה מסמנת תור כ"הושלם" =====
    public async Task<Result> UpdateStatusAsync(int id, UpdateAppointmentRequest request, CancellationToken ct = default)
    {
        var appointment = await _appointments.GetForUpdateAsync(id, ct);
        if (appointment is null)
            return Result.NotFound("התור לא נמצא");

        if (request.Status != AppointmentStatus.Completed)
            return Result.Validation("אפשר רק לסמן תור כהושלם. לביטול משתמשים בביטול תור");

        if (appointment.Status != AppointmentStatus.Confirmed)
            return Result.Conflict("רק תור מאושר יכול להיות מסומן כהושלם");

        appointment.Status = AppointmentStatus.Completed;
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Appointment {AppointmentId} marked as completed", id);
        return Result.Success();
    }
}