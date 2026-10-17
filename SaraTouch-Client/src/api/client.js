import axios from 'axios'

// המפתח שתחתיו נשמרים פרטי ההתחברות ב-localStorage
export const AUTH_STORAGE_KEY = 'saratouch.auth'

// מופע axios אחד לכל האפליקציה.
// baseURL הוא '/api' כי Vite מעביר את הבקשות לשרת (ראו vite.config.js).
const api = axios.create({ baseURL: '/api' })

// Request interceptor: לפני כל בקשה, אם יש token שמור, מוסיפים אותו ל-header.
// כך אף מסך לא צריך לזכור לשלוח את ה-JWT בעצמו.
api.interceptors.request.use((config) => {
  const saved = readAuth()
  if (saved?.token) {
    config.headers.Authorization = `Bearer ${saved.token}`
  }
  return config
})

// Response interceptor: אם השרת החזיר 401 (token פג או לא תקין), מנקים ושולחים להתחברות.
// לא עושים את זה בבקשת ההתחברות עצמה, כי שם 401 פירושו "סיסמה שגויה".
api.interceptors.response.use(
  (response) => response,
  (error) => {
    const isLoginRequest = error.config?.url?.startsWith('/tokens')
    if (error.response?.status === 401 && !isLoginRequest) {
      localStorage.removeItem(AUTH_STORAGE_KEY)
      window.location.assign('/login?expired=1')
    }
    return Promise.reject(error)
  },
)

export function readAuth() {
  try {
    const saved = JSON.parse(localStorage.getItem(AUTH_STORAGE_KEY))
    if (!saved?.token) return null
    // token שפג תוקפו לא שווה כלום, עדיף לזרוק אותו מיד
    if (saved.expiresAtUtc && new Date(saved.expiresAtUtc) <= new Date()) {
      localStorage.removeItem(AUTH_STORAGE_KEY)
      return null
    }
    return saved
  } catch {
    return null
  }
}

// השרת מחזיר כל שגיאה כ-ErrorResponse { status, title, message, correlationId, errors }.
// הפונקציה הזו מוציאה ממנו הודעה אחת שאפשר להציג למשתמשת.
export function getErrorMessage(error, fallback = 'משהו השתבש. נסי שוב.') {
  if (!error?.response) {
    return 'אין חיבור לשרת. ודאי שהשרת רץ ב-Visual Studio.'
  }
  const data = error.response.data
  return data?.message || data?.title || fallback
}

// שגיאות ולידציה (400) מגיעות ב-errors: { "Email": ["..."], ... }. מחזירים רשימה שטוחה.
export function getFieldErrors(error) {
  const errors = error?.response?.data?.errors
  if (!errors || typeof errors !== 'object') return []
  return Object.values(errors).flat().filter(Boolean)
}

export default api
