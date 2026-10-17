# שרה · טאצ' שתאהבי (לקוח React)

הלקוח של מערכת התורים SaraTouch. הוא מדגים את השרת (`SaraTouch-Server`) מקצה לקצה.

## הרצה

1. מריצים את השרת ב-Visual Studio (`https://localhost:7127`).
2. בתיקייה הזו:
   ```
   npm install
   npm run dev
   ```
3. פותחים את `http://localhost:5173`.

ה-Vite מעביר כל בקשה ל-`/api` אל השרת (proxy ב-`vite.config.js`), ולכן אין צורך ב-CORS. אם הפורט של השרת שונה, משנים את `API_TARGET` באותו קובץ.

## מסכים

| מסך | מה מדגים |
| --- | --- |
| התחברות והרשמה | `POST /api/tokens` ו-`POST /api/users`. ה-JWT נשמר ב-localStorage, ו-axios interceptor מוסיף `Authorization: Bearer` לכל בקשה. על 401 המשתמשת מועברת להתחברות. |
| שעות פנויות | `GET /api/timeslots` עם pagination אמיתי מול השרת (`page`, `pageSize`), מקובצות לפי יום בשעון ישראל. |
| קביעת תור | חלון לבחירת טיפול והערות, ו-`POST /api/appointments`. |
| התנגשות (409) | כשהשעה נתפסה בינתיים, מוצגת הודעה עדינה והרשימה מתרעננת. |
| התורים שלי | `GET /api/appointments` וביטול עם `DELETE`. Admin רואה את כל התורים. |

## איך מדגימים 409

**דרך א, שתי לקוחות:** חלון רגיל עם `client@saratouch.local`, וחלון גלישה בסתר עם `client2@saratouch.local`. בשני החלונות פותחים את אותה שעה. קובעים בראשון, ואחר כך בשני. השני מקבל 409.

**דרך ב, כפתור ההדגמה:** בחלון קביעת התור יש קישור "הדגמת תחרות". הוא שולח שתי בקשות זהות באותו רגע ומציג את התשובות: אחת 201 ואחת 409.

## מבנה

```
src/
  api/client.js       axios, interceptors, קריאת הודעות שגיאה מ-ErrorResponse
  api/endpoints.js    כל הקריאות לשרת
  api/format.js       תאריכים (UTC לשעון ישראל) ושמות שדות
  auth/AuthContext.jsx  המשתמשת המחוברת, login, register, logout
  components/         Header, Logo, Notice, Pagination, BookingDialog
  pages/              LoginPage, SlotsPage, AppointmentsPage
```

ההרשאות האמיתיות נבדקות בשרת. הסתרת כפתורים בלקוח היא רק נוחות.
