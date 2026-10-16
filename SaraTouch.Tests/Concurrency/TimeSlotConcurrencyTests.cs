using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SaraTouch.Core.Entities;
using SaraTouch.Core.Enums;
using SaraTouch.Core.Exceptions;
using SaraTouch.Data;
using SaraTouch.Data.Repositories;
using Xunit;

namespace SaraTouch.Tests.Concurrency;

/// <summary>
/// ההוכחה שה-concurrency token עובד מול בסיס נתונים אמיתי:
/// שני DbContext נפרדים, כמו שתי בקשות HTTP שרצות במקביל, קוראים ומשנים את אותו סלוט.
/// הראשון שומר בהצלחה, והשני מקבל חריגה.
/// SQLite בזיכרון מאפשר להריץ את הבדיקה בלי PostgreSQL. ה-token מסוג Guid עובד בכל בסיס נתונים.
/// </summary>
public class TimeSlotConcurrencyTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;
    private readonly Guid _originalVersion;

    public TimeSlotConcurrencyTests()
    {
        // החיבור נשאר פתוח כל הבדיקה. בסיס נתונים בזיכרון חי רק כל עוד החיבור פתוח
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new AppDbContext(_options);
        context.Database.EnsureCreated();

        var start = new DateTime(2026, 11, 1, 7, 0, 0, DateTimeKind.Utc);
        var slot = new TimeSlot { StartUtc = start, EndUtc = start.AddHours(1), Status = SlotStatus.Available };
        context.TimeSlots.Add(slot);
        context.SaveChanges();
        _originalVersion = slot.Version;
    }

    [Fact]
    public async Task TwoContexts_BookSameSlot_FirstSucceedsSecondThrows()
    {
        await using var contextA = new AppDbContext(_options);
        await using var contextB = new AppDbContext(_options);

        // שתי ה"בקשות" קוראות את הסלוט כשהוא עדיין פנוי, עם אותה Version
        var slotA = await contextA.TimeSlots.SingleAsync();
        var slotB = await contextB.TimeSlots.SingleAsync();
        Assert.Equal(SlotStatus.Available, slotA.Status);
        Assert.Equal(SlotStatus.Available, slotB.Status);

        // שתיהן תופסות אותו
        slotA.Status = SlotStatus.Booked;
        slotB.Status = SlotStatus.Booked;

        // הראשונה שומרת בהצלחה. ה-Version בבסיס הנתונים מתחלף
        await contextA.SaveChangesAsync();

        // השנייה שומרת עם ה-Version הישנה, ה-UPDATE לא מוצא שורה, ונזרקת חריגה
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => contextB.SaveChangesAsync());

        // בבסיס הנתונים: הסלוט תפוס פעם אחת, וה-Version השתנתה
        await using var verify = new AppDbContext(_options);
        var saved = await verify.TimeSlots.SingleAsync();
        Assert.Equal(SlotStatus.Booked, saved.Status);
        Assert.NotEqual(_originalVersion, saved.Version);
    }

    [Fact]
    public async Task UnitOfWork_OnConflict_TranslatesToConcurrencyConflictException()
    {
        await using var contextA = new AppDbContext(_options);
        await using var contextB = new AppDbContext(_options);

        var slotA = await contextA.TimeSlots.SingleAsync();
        var slotB = await contextB.TimeSlots.SingleAsync();
        slotA.Status = SlotStatus.Booked;
        slotB.Status = SlotStatus.Blocked;   // שרה מנסה לחסום באותו רגע

        await new UnitOfWork(contextA).SaveChangesAsync();

        // ה-UnitOfWork מתרגם את החריגה של EF לחריגה של Core
        await Assert.ThrowsAsync<ConcurrencyConflictException>(
            () => new UnitOfWork(contextB).SaveChangesAsync());
    }

    public void Dispose() => _connection.Dispose();
}