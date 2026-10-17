// עזרים להצגת נתונים מהשרת.
//
// שימי לב: אם שמות השדות ב-DTOs שלך שונים (למשל StartsAtUtc במקום StartUtc),
// זה המקום היחיד שצריך לעדכן. כל הפונקציות כאן בודקות כמה שמות נפוצים.

const TZ = 'Asia/Jerusalem'

// השרת שומר ב-UTC. אם התאריך הגיע בלי Z בסוף, מוסיפים אותו כדי שהדפדפן לא יחשוב שזה שעון מקומי.
export function parseUtc(value) {
  if (!value) return null
  const s = String(value)
  const hasZone = /Z$|[+-]\d{2}:?\d{2}$/.test(s)
  return new Date(hasZone ? s : s + 'Z')
}

export const slotStart = (s) =>
  parseUtc(s?.startUtc ?? s?.startsAtUtc ?? s?.startTimeUtc ?? s?.startTime ?? s?.start)

export const slotEnd = (s) =>
  parseUtc(s?.endUtc ?? s?.endsAtUtc ?? s?.endTimeUtc ?? s?.endTime ?? s?.end)

export const appointmentStart = (a) =>
  parseUtc(
    a?.timeSlot?.startUtc ??
      a?.timeSlot?.startsAtUtc ??
      a?.slotStartUtc ??
      a?.timeSlotStartUtc ??
      a?.startUtc ??
      a?.startsAtUtc,
  )

export const appointmentTreatment = (a) =>
  a?.treatmentName ?? a?.treatment?.name ?? ''

export const appointmentClient = (a) =>
  a?.clientName ?? a?.userFullName ?? a?.clientFullName ?? a?.user?.fullName ?? ''

// מפתח של יום (בשעון ישראל) כדי לקבץ סלוטים לפי יום
export const dayKey = (date) =>
  date.toLocaleDateString('en-CA', { timeZone: TZ }) // 2026-10-18

export const formatDay = (date) =>
  date.toLocaleDateString('he-IL', { timeZone: TZ, weekday: 'long', day: 'numeric', month: 'long' })

export const formatTime = (date) =>
  date.toLocaleTimeString('he-IL', { timeZone: TZ, hour: '2-digit', minute: '2-digit' })

export const formatDateTime = (date) => (date ? `${formatDay(date)}, ${formatTime(date)}` : '')

export const formatPrice = (price) =>
  price == null ? '' : `₪${Number(price).toLocaleString('he-IL')}`

const STATUS_LABELS = {
  Confirmed: 'מאושר',
  Cancelled: 'בוטל',
  Completed: 'הושלם',
}
export const statusLabel = (status) => STATUS_LABELS[status] ?? status
