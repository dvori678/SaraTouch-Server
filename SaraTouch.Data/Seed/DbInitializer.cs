using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SaraTouch.Core.Entities;
using SaraTouch.Core.Enums;
using SaraTouch.Core.Interfaces.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaraTouch.Data.Seed
{
    public class DbInitializer
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher _hasher;
        private readonly ILogger<DbInitializer> _logger;

        public DbInitializer(AppDbContext context, IPasswordHasher hasher, ILogger<DbInitializer> logger)
        {
            _context = context;
            _hasher = hasher;
            _logger = logger;
        }

        public async Task InitializeAsync(CancellationToken ct = default)
        {
            // מריץ את כל ה-migrations שעוד לא רצו. אם בסיס הנתונים לא קיים, הוא נוצר
            await _context.Database.MigrateAsync(ct);

            if (await _context.Users.AnyAsync(ct))
            {
                _logger.LogInformation("Database already seeded");
                return;
            }

            _logger.LogInformation("Seeding database");

            // משתמשות דמו
            var sara = new User
            {
                FullName = "שרה",
                Email = "sara@saratouch.local",
                Phone = "050-0000000",
                PasswordHash = _hasher.Hash("Admin123!"),
                Role = Role.Admin
            };
            var client1 = new User
            {
                FullName = "לקוחה ראשונה",
                Email = "client@saratouch.local",
                Phone = "050-1111111",
                PasswordHash = _hasher.Hash("Client123!"),
                Role = Role.Client
            };
            var client2 = new User
            {
                FullName = "לקוחה שנייה",
                Email = "client2@saratouch.local",
                Phone = "050-2222222",
                PasswordHash = _hasher.Hash("Client123!"),
                Role = Role.Client
            };
            _context.Users.AddRange(sara, client1, client2);

            // תגיות
            var tagEvent = new Tag { Name = "אירוע" };
            var tagTrial = new Tag { Name = "ניסיון" };
            var tagPackage = new Tag { Name = "חבילה" };
            var tagDaily = new Tag { Name = "יומיומי" };
            _context.Tags.AddRange(tagEvent, tagTrial, tagPackage, tagDaily);

            // טיפולים, עם קשר many-to-many לתגיות
            var evening = new Treatment
            {
                Name = "איפור ערב",
                Description = "איפור מלא לאירוע ערב",
                Price = 350,
                DurationMinutes = 60,
                Tags = { tagEvent }
            };
            var daily = new Treatment
            {
                Name = "איפור יומיומי",
                Description = "איפור קליל ועדין ליום-יום",
                Price = 200,
                DurationMinutes = 45,
                Tags = { tagDaily }
            };
            var trial = new Treatment
            {
                Name = "ניסיון איפור",
                Description = "מפגש ניסיון לפני אירוע",
                Price = 150,
                DurationMinutes = 60,
                Tags = { tagTrial, tagEvent }
            };
            var package = new Treatment
            {
                Name = "חבילת אירוע",
                Description = "ניסיון ואיפור ביום האירוע",
                Price = 450,
                DurationMinutes = 60,
                Tags = { tagPackage, tagEvent }
            };
            _context.Treatments.AddRange(evening, daily, trial, package);

            // סלוטים לשבועיים הקרובים: ראשון עד חמישי, 09:00 עד 17:00 שעון ישראל
            var israel = TimeZoneInfo.FindSystemTimeZoneById("Asia/Jerusalem");
            var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, israel).Date;
            var slots = new List<TimeSlot>();

            for (var day = 1; day <= 14; day++)
            {
                var date = today.AddDays(day);
                if (date.DayOfWeek is DayOfWeek.Friday or DayOfWeek.Saturday)
                    continue;

                for (var hour = 9; hour < 17; hour++)
                {
                    var localStart = DateTime.SpecifyKind(date.AddHours(hour), DateTimeKind.Unspecified);
                    var startUtc = TimeZoneInfo.ConvertTimeToUtc(localStart, israel);

                    slots.Add(new TimeSlot
                    {
                        StartUtc = startUtc,
                        EndUtc = startUtc.AddMinutes(60),
                        Status = SlotStatus.Available
                    });
                }
            }
            _context.TimeSlots.AddRange(slots);

            // תור אחד קיים, כדי שיהיה מה להציג
            var bookedSlot = slots[0];
            bookedSlot.Status = SlotStatus.Booked;
            _context.Appointments.Add(new Appointment
            {
                User = client1,
                Treatment = evening,
                TimeSlot = bookedSlot,
                Status = AppointmentStatus.Confirmed,
                CreatedAtUtc = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Seeded {SlotCount} time slots", slots.Count);
        }
    }
}
