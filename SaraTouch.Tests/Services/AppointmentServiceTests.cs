using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using SaraTouch.Core.Common;
using SaraTouch.Core.DTOs.Appointments;
using SaraTouch.Core.Entities;
using SaraTouch.Core.Enums;
using SaraTouch.Core.Exceptions;
using SaraTouch.Core.Interfaces.Infrastructure;
using SaraTouch.Core.Interfaces.Repositories;
using SaraTouch.Core.Settings;
using SaraTouch.Service.Mapping;
using SaraTouch.Service.Services;
using Xunit;

namespace SaraTouch.Tests.Services;

public class AppointmentServiceTests
{
    // "עכשיו" קבוע. בזכות IClock הבדיקה לא תלויה בשעה שבה מריצים אותה
    private static readonly DateTime Now = new(2026, 10, 20, 8, 0, 0, DateTimeKind.Utc);
    private const int ClientId = 2;
    private const int OtherClientId = 3;

    private readonly Mock<IAppointmentRepository> _appointments = new();
    private readonly Mock<ITimeSlotRepository> _slots = new();
    private readonly Mock<ITreatmentRepository> _treatments = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IClock> _clock = new();
    private readonly IMapper _mapper;

    public AppointmentServiceTests()
    {
        _clock.Setup(c => c.UtcNow).Returns(Now);

        // ה-mapper אמיתי, עם ה-Profile האמיתי
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();
    }

    private AppointmentService CreateService() => new(
        _appointments.Object,
        _slots.Object,
        _treatments.Object,
        _unitOfWork.Object,
        _mapper,
        _clock.Object,
        Options.Create(new BookingSettings { CancellationDeadlineHours = 24 }),
        NullLogger<AppointmentService>.Instance);

    private static Treatment ActiveTreatment() =>
        new() { Id = 1, Name = "איפור ערב", Price = 350, IsActive = true };

    private static TimeSlot Slot(SlotStatus status, DateTime startUtc) =>
        new() { Id = 5, StartUtc = startUtc, EndUtc = startUtc.AddHours(1), Status = status };

    private static CreateAppointmentRequest BookRequest() =>
        new() { TimeSlotId = 5, TreatmentId = 1 };

    private void SetupTreatmentAndSlot(TimeSlot slot)
    {
        _treatments.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(ActiveTreatment());
        _slots.Setup(r => r.GetForUpdateAsync(slot.Id, It.IsAny<CancellationToken>()))
              .ReturnsAsync(slot);
    }

    // ================= קביעת תור: המשאב המוגבל =================

    [Fact]
    public async Task Book_AvailableSlot_ReturnsSuccessAndMarksSlotBooked()
    {
        // Arrange
        var slot = Slot(SlotStatus.Available, Now.AddDays(2));
        SetupTreatmentAndSlot(slot);
        _appointments.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Appointment
            {
                Id = 10,
                UserId = ClientId,
                User = new User { FullName = "לקוחה" },
                Treatment = ActiveTreatment(),
                TimeSlot = slot,
                Status = AppointmentStatus.Confirmed
            });

        // Act
        var result = await CreateService().BookAsync(BookRequest(), ClientId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(SlotStatus.Booked, slot.Status);
        _appointments.Verify(r => r.Add(It.Is<Appointment>(a =>
            a.UserId == ClientId && a.TimeSlotId == 5)), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Book_SlotAlreadyBooked_ReturnsConflictAndDoesNotSave()
    {
        var slot = Slot(SlotStatus.Booked, Now.AddDays(2));
        SetupTreatmentAndSlot(slot);

        var result = await CreateService().BookAsync(BookRequest(), ClientId);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
        _appointments.Verify(r => r.Add(It.IsAny<Appointment>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Book_ConcurrentSaveConflict_ReturnsConflict()
    {
        // הסלוט נראה פנוי בקריאה, אבל בזמן השמירה מישהי אחרת כבר תפסה אותו
        var slot = Slot(SlotStatus.Available, Now.AddDays(2));
        SetupTreatmentAndSlot(slot);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                   .ThrowsAsync(new ConcurrencyConflictException("conflict"));

        var result = await CreateService().BookAsync(BookRequest(), ClientId);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
    }

    [Fact]
    public async Task Book_SlotInThePast_ReturnsValidationError()
    {
        var slot = Slot(SlotStatus.Available, Now.AddHours(-1));
        SetupTreatmentAndSlot(slot);

        var result = await CreateService().BookAsync(BookRequest(), ClientId);

        Assert.Equal(ErrorType.Validation, result.ErrorType);
    }

    [Fact]
    public async Task Book_TreatmentNotFound_ReturnsNotFound()
    {
        _treatments.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                   .ReturnsAsync((Treatment?)null);

        var result = await CreateService().BookAsync(BookRequest(), ClientId);

        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    // ================= ביטול תור =================

    private Appointment SetupConfirmedAppointment(int ownerId, DateTime startUtc)
    {
        var appointment = new Appointment
        {
            Id = 7,
            UserId = ownerId,
            Status = AppointmentStatus.Confirmed,
            TimeSlot = Slot(SlotStatus.Booked, startUtc)
        };
        _appointments.Setup(r => r.GetForUpdateAsync(7, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(appointment);
        return appointment;
    }

    [Fact]
    public async Task Cancel_OtherClientsAppointment_ReturnsForbidden()
    {
        SetupConfirmedAppointment(OtherClientId, Now.AddDays(3));

        var result = await CreateService().CancelAsync(7, ClientId, isAdmin: false);

        Assert.Equal(ErrorType.Forbidden, result.ErrorType);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Cancel_LessThan24HoursBefore_ReturnsConflict()
    {
        SetupConfirmedAppointment(ClientId, Now.AddHours(10));

        var result = await CreateService().CancelAsync(7, ClientId, isAdmin: false);

        Assert.Equal(ErrorType.Conflict, result.ErrorType);
    }

    [Fact]
    public async Task Cancel_ValidRequest_CancelsAndFreesSlot()
    {
        var appointment = SetupConfirmedAppointment(ClientId, Now.AddDays(3));

        var result = await CreateService().CancelAsync(7, ClientId, isAdmin: false);

        Assert.True(result.IsSuccess);
        Assert.Equal(AppointmentStatus.Cancelled, appointment.Status);
        Assert.Equal(SlotStatus.Available, appointment.TimeSlot.Status);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Cancel_AdminLessThan24HoursBefore_Succeeds()
    {
        SetupConfirmedAppointment(ClientId, Now.AddHours(2));

        var result = await CreateService().CancelAsync(7, currentUserId: 1, isAdmin: true);

        Assert.True(result.IsSuccess);
    }
}