import api from './client.js'

// כל הקריאות לשרת במקום אחד. המסכים קוראים לפונקציות האלה ולא ל-axios ישירות.

// ---- התחברות והרשמה ----
export const login = (email, password) =>
  api.post('/tokens', { email, password }).then((r) => r.data)

export const register = (data) => api.post('/users', data).then((r) => r.data)

// ---- טיפולים ----
export const getTreatments = () =>
  api
    .get('/treatments', { params: { sortBy: 'name', page: 1, pageSize: 50 } })
    .then((r) => r.data)

// ---- סלוטים ----
// pagination אמיתי: השרת מחזיר רק את העמוד המבוקש (Skip/Take בתוך השאילתה)
export const getAvailableSlots = (page, pageSize) =>
  api
    .get('/timeslots', {
      params: {
        fromUtc: new Date().toISOString(),
        status: 'Available',
        page,
        pageSize,
      },
    })
    .then((r) => r.data)

// ---- תורים ----
export const bookAppointment = (timeSlotId, treatmentId, notes) =>
  api.post('/appointments', { timeSlotId, treatmentId, notes: notes || null })

export const getAppointments = (page = 1, pageSize = 50) =>
  api.get('/appointments', { params: { page, pageSize } }).then((r) => r.data)

export const cancelAppointment = (id) => api.delete(`/appointments/${id}`)
