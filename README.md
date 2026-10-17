# שרה, טאצ' שתאהבי ♡

שרת ASP.NET Core Web API לקביעת תורים אצל שרה, מאפרת.
לקוחות נרשמות, בוחרות טיפול ושעה פנויה ביומן, וקובעות תור. שרה (Admin) מנהלת את הטיפולים, התגיות והיומן, ורואה את כל התורים.

**טכנולוגיות:** .NET 8 · ASP.NET Core Web API · EF Core 8 (Code-First) · PostgreSQL · JWT · AutoMapper · NLog · xUnit + Moq

---

## המשאב המוגבל: סלוט ביומן

המשאב שמתחרים עליו הוא **`TimeSlot`**: שעה ביומן של שרה. בכל סלוט אפשר לקבוע תור מאושר אחד בלבד.

**איפה קורית התחרות:** שתי לקוחות לוחצות "קבעי תור" על אותה שעה באותו רגע. שתיהן קוראות את הסלוט כשהוא עדיין `Available`, ושתיהן עוברות את הבדיקה. בלי הגנה, שתיהן היו מקבלות אישור. אותו מצב קיים גם כששרה חוסמת סלוט בזמן שלקוחה קובעת בו תור, וגם כשסלוט מתפנה בביטול ושתי לקוחות מנסות לתפוס אותו.

**איך המערכת מגיבה: optimistic concurrency**
- לישות `TimeSlot` יש שדה `Version` (Guid), שמוגדר כ-concurrency token ב-Fluent API (`IsConcurrencyToken`).
- הדריסה של `AppDbContext.SaveChangesAsync` מחליפה את ה-`Version` בכל עדכון של סלוט, כך שאף Service לא צריך לזכור לעשות את זה.
- ה-`UPDATE` כולל `WHERE "Version" = @old`. אם מישהו אחר כבר שמר, השורה לא נמצאת, ו-EF זורק `DbUpdateConcurrencyException`.
- ה-`UnitOfWork` (בשכבת Data) מתרגם את החריגה ל-`ConcurrencyConflictException` של Core. כך שכבת Service לא תלויה ב-EF.
- `AppointmentService` תופס את החריגה הספציפית, כותב **Warning** ללוג ומחזיר **409 Conflict** עם ההודעה: "השעה נתפסה ממש עכשיו על ידי לקוחה אחרת. בחרי שעה אחרת".
- העדכון של הסלוט והוספת התור נשמרים ב-`SaveChangesAsync` אחד, כלומר בטרנזקציה אחת. אם העדכון נכשל, גם התור לא נשמר.

**ההוכחה:** הבדיקה `TimeSlotConcurrencyTests` פותחת שני `DbContext` נפרדים. שניהם קוראים ומשנים את אותו סלוט, הראשון שומר בהצלחה, והשני מקבל `DbUpdateConcurrencyException`.

---

## הרצה מקומית

### דרישות
- .NET 8 SDK
- PostgreSQL 16 או 17, שרץ על `localhost:5432`

### 1. הגדרת סודות (User Secrets)
הסיסמה של בסיס הנתונים ומפתח ה-JWT **לא** נמצאים ב-repo. מריצים מהתיקייה הראשית:
```bash
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=SaraTouchDb;Username=postgres;Password=YOUR_PASSWORD" --project SaraTouch.API
dotnet user-secrets set "Jwt:Key" "any-random-string-of-at-least-32-characters" --project SaraTouch.API
```
ב-Visual Studio אפשר גם: קליק ימני על `SaraTouch.API` > **Manage User Secrets**.

### 2. הרצה
```bash
dotnet run --project SaraTouch.API
```
או **F5** ב-Visual Studio.

**אין צורך ליצור את בסיס הנתונים ידנית.** בעליית השרת `DbInitializer` מריץ את כל ה-migrations ומכניס נתוני דמו: משתמשות, טיפולים, תגיות, ו-80 סלוטים לשבועיים הקרובים.

Swagger זמין בכתובת `https://localhost:7127/swagger`.

### 3. בדיקות
```bash
dotnet test
```

---

## משתמשות דמו

| תפקיד | מייל | סיסמה |
| --- | --- | --- |
| Admin (שרה) | `sara@saratouch.local` | `Admin123!` |
| Client | `client@saratouch.local` | `Client123!` |
| Client | `client2@saratouch.local` | `Client123!` |

**הדגמת 409:** מתחברים כ-`client` ב-Swagger, קובעים תור בסלוט פנוי (`POST /api/appointments`), ושולחים את אותה בקשה שוב.

---

## מבנה הפרויקט

```
SaraTouch.sln
├── SaraTouch.Core       Entities, Enums, DTOs, Interfaces, Result. בלי תלויות חיצוניות
├── SaraTouch.Data       AppDbContext, Fluent API, Migrations, Repositories, UnitOfWork, Seed
├── SaraTouch.Service    לוגיקה עסקית, AutoMapper Profiles, הצפנת סיסמאות
├── SaraTouch.API        Controllers, Middleware, JWT, NLog, Program.cs
└── SaraTouch.Tests      xUnit + Moq, ובדיקת concurrency עם שני DbContext
```

**כיוון התלויות:** Core לא תלוי באף פרויקט. Data ו-Service תלויים ב-Core. API תלוי ב-Service וב-Core, ומכיר את Data רק לרישום ב-DI.

## Endpoints עיקריים

| Method | נתיב | הרשאה |
| --- | --- | --- |
| POST | `/api/users` | כולם (הרשמה) |
| POST | `/api/tokens` | כולם (התחברות) |
| GET | `/api/users/me` | מחוברות |
| GET / POST / PUT / DELETE | `/api/treatments` | קריאה לכולם, כתיבה ל-Admin |
| GET / POST | `/api/tags` | קריאה לכולם, כתיבה ל-Admin |
| GET / POST / PATCH | `/api/timeslots` | קריאה לכולם, כתיבה ל-Admin |
| GET / POST / DELETE / PATCH | `/api/appointments` | מחוברות. קביעה ל-Client, PATCH ל-Admin |

## לוגים
NLog כותב ל-`SaraTouch.API/bin/Debug/net8.0/logs/saratouch-{date}.log`. בכל שורה מופיע ה-CorrelationId של הבקשה, שחוזר גם בכותרת `X-Correlation-Id` של התשובה. התנגשויות נכתבות ברמת Warning, וחריגות ברמת Error. סיסמאות ו-tokens לא נכתבים ללוג.

## צד הלקוח (React)

הלקוח נמצא בתיקייה `SaraTouch-Client`. הוא מדגים את השרת מקצה לקצה: התחברות והרשמה, רשימת שעות פנויות עם pagination מול השרת, קביעת תור, והצגת 409 כשהשעה נתפסה.

**הרצה:**
1. מריצים את השרת ב-Visual Studio בפרופיל `https` (`https://localhost:7127`).
2. בתיקייה `SaraTouch-Client`:
```
   npm install
   npm run dev
```
3. פותחים את `http://localhost:5173`.

Vite מעביר כל בקשה ל-`/api` אל השרת (proxy ב-`vite.config.js`), ולכן אין צורך ב-CORS.

**הדגמת 409:** בחלון רגיל מתחברים כ-`client@saratouch.local`, ובחלון גלישה בסתר כ-`client2@saratouch.local`. בשני החלונות בוחרים את אותה שעה. הראשונה שמאשרת מקבלת 201, והשנייה מקבלת 409 עם הודעה, והרשימה מתרעננת.