import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext.jsx'
import { bookAppointment, getTreatments } from '../api/endpoints.js'
import { getErrorMessage, getFieldErrors } from '../api/client.js'
import { formatDateTime, formatPrice, slotStart } from '../api/format.js'
import Notice from './Notice.jsx'

// חלון קביעת תור לסלוט שנבחר.
// onBooked  - נקרא אחרי 201 (התור נקבע)
// onConflict - נקרא אחרי 409 (השעה נתפסה בינתיים), עם ההודעה מהשרת
export default function BookingDialog({ slot, onClose, onBooked, onConflict }) {
  const { user, isClient } = useAuth()
  const [treatments, setTreatments] = useState([])
  const [treatmentId, setTreatmentId] = useState('')
  const [notes, setNotes] = useState('')
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState(null)
  const [fieldErrors, setFieldErrors] = useState([])
  const [raceResult, setRaceResult] = useState(null)

  useEffect(() => {
    getTreatments()
      .then((data) => {
        const list = data.items ?? data // PagedResult או מערך רגיל
        setTreatments(list)
        if (list.length > 0) setTreatmentId(String(list[0].id))
      })
      .catch((err) => setError(getErrorMessage(err, 'לא הצלחנו לטעון את הטיפולים.')))
  }, [])

  // סגירה עם Escape
  useEffect(() => {
    const onKey = (e) => e.key === 'Escape' && !busy && onClose()
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [busy, onClose])

  async function handleBook() {
    setBusy(true)
    setError(null)
    setFieldErrors([])
    try {
      await bookAppointment(slot.id, Number(treatmentId), notes)
      onBooked()
    } catch (err) {
      if (err.response?.status === 409) {
        // המקרה החשוב: מישהי אחרת תפסה את השעה בין הטעינה ללחיצה
        onConflict(getErrorMessage(err))
      } else {
        setError(getErrorMessage(err))
        setFieldErrors(getFieldErrors(err))
      }
    } finally {
      setBusy(false)
    }
  }

  // הדגמת תחרות: שולחים שתי בקשות זהות באותו רגע, כמו שתי לקוחות שלוחצות יחד.
  // השרת צריך לאשר אחת (201) ולדחות את השנייה (409).
  async function handleRaceDemo() {
    setBusy(true)
    setError(null)
    const requests = [1, 2].map(() => bookAppointment(slot.id, Number(treatmentId), notes))
    const results = await Promise.allSettled(requests)
    const statuses = results.map((r) => (r.status === 'fulfilled' ? r.value.status : r.reason.response?.status ?? 'ללא תשובה'))
    setRaceResult(statuses)
    setBusy(false)
  }

  const start = slotStart(slot)

  return (
    <div className="fixed inset-0 z-50 flex items-end justify-center bg-ink/30 p-4 sm:items-center" onClick={() => !busy && onClose()}>
      <div className="card w-full max-w-md space-y-5 p-6 sm:p-8" onClick={(e) => e.stopPropagation()} role="dialog" aria-modal="true">
        <div>
          <p className="text-sm text-muted">קביעת תור</p>
          <h2 className="font-display text-2xl">{formatDateTime(start)}</h2>
        </div>

        {!user && (
          <>
            <p className="text-muted">כדי לקבוע תור צריך להתחבר.</p>
            <div className="flex gap-3">
              <Link to="/login" className="btn-primary">להתחברות</Link>
              <button className="btn-ghost" onClick={onClose}>סגירה</button>
            </div>
          </>
        )}

        {user && !isClient && (
          <>
            <p className="text-muted">קביעת תורים פתוחה ללקוחות בלבד. התחברי כלקוחה כדי לנסות.</p>
            <button className="btn-ghost" onClick={onClose}>סגירה</button>
          </>
        )}

        {isClient && raceResult && (
          <>
            <Notice type="success" title="תוצאות הבדיקה">
              <ul className="mt-1 space-y-0.5" dir="ltr">
                {raceResult.map((s, i) => (
                  <li key={i} className="text-right">
                    בקשה {i + 1}: <b>{s}</b> {s === 201 ? '(נקבע)' : s === 409 ? '(נדחה, השעה נתפסה)' : ''}
                  </li>
                ))}
              </ul>
            </Notice>
            <button
              className="btn-primary w-full"
              onClick={() => (raceResult.includes(201) ? onBooked() : onConflict(''))}
            >
              סגירה ורענון
            </button>
          </>
        )}

        {isClient && !raceResult && (
          <>
            <div>
              <label className="label" htmlFor="treatment">טיפול</label>
              <select id="treatment" className="input" value={treatmentId} onChange={(e) => setTreatmentId(e.target.value)}>
                {treatments.map((t) => (
                  <option key={t.id} value={t.id}>
                    {t.name}{t.price != null && ` · ${formatPrice(t.price)}`}
                  </option>
                ))}
              </select>
            </div>
            <div>
              <label className="label" htmlFor="notes">הערות (לא חובה)</label>
              <textarea id="notes" rows={2} className="input resize-none" value={notes} onChange={(e) => setNotes(e.target.value)} />
            </div>

            {error && (
              <Notice type="error" title={error}>
                {fieldErrors.length > 0 && (
                  <ul className="mt-1 list-disc pr-5">{fieldErrors.map((m) => <li key={m}>{m}</li>)}</ul>
                )}
              </Notice>
            )}

            <div className="flex gap-3">
              <button className="btn-primary flex-1" disabled={busy || !treatmentId} onClick={handleBook}>
                {busy ? 'רגע...' : 'קבעי תור'}
              </button>
              <button className="btn-ghost" disabled={busy} onClick={onClose}>ביטול</button>
            </div>

            <button
              className="w-full text-center text-xs text-muted underline-offset-4 hover:text-ink hover:underline cursor-pointer"
              disabled={busy || !treatmentId}
              onClick={handleRaceDemo}
            >
              הדגמת תחרות: שליחת שתי בקשות באותו רגע
            </button>
          </>
        )}
      </div>
    </div>
  )
}
