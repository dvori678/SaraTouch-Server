import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext.jsx'
import { cancelAppointment, getAppointments } from '../api/endpoints.js'
import { getErrorMessage } from '../api/client.js'
import {
  appointmentClient, appointmentStart, appointmentTreatment, formatDateTime, statusLabel,
} from '../api/format.js'
import Notice from '../components/Notice.jsx'

const statusStyle = {
  Confirmed: 'bg-success/10 text-success',
  Cancelled: 'bg-line text-muted',
  Completed: 'bg-gold/10 text-gold',
}

// "התורים שלי": הלקוחה רואה רק את שלה, שרה (Admin) רואה את כולם. הסינון נעשה בשרת.
export default function AppointmentsPage() {
  const { isAdmin } = useAuth()
  const [appointments, setAppointments] = useState(null)
  const [loadError, setLoadError] = useState(null)
  const [message, setMessage] = useState(null)
  const [rowError, setRowError] = useState(null) // { id, text } - שגיאה שמוצגת מתחת לתור הספציפי
  const [confirmId, setConfirmId] = useState(null) // ביטול בשתי לחיצות, כדי לא לבטל בטעות
  const [busyId, setBusyId] = useState(null)

  async function load() {
    try {
      const data = await getAppointments()
      const list = data.items ?? data // PagedResult או מערך רגיל
      // הקרובים קודם
      list.sort((a, b) => (appointmentStart(a) ?? 0) - (appointmentStart(b) ?? 0))
      setAppointments(list)
    } catch (err) {
      setLoadError(getErrorMessage(err, 'לא הצלחנו לטעון את התורים.'))
    }
  }

  useEffect(() => {
    load()
  }, [])

  async function handleCancel(id) {
    setBusyId(id)
    setRowError(null)
    setMessage(null)
    try {
      await cancelAppointment(id)
      setMessage('התור בוטל, והשעה חזרה להיות פנויה.')
    } catch (err) {
      // למשל 409: לקוחה שמנסה לבטל פחות מ-24 שעות לפני התור. ההחלטה היא של השרת.
      setRowError({ id, text: getErrorMessage(err, 'לא ניתן לבטל את התור.') })
    } finally {
      // בכל מקרה טוענים מחדש מהשרת, כדי שהמסך יציג בדיוק את מה ששמור בבסיס הנתונים
      await load()
      setBusyId(null)
      setConfirmId(null)
    }
  }

  // תורים פעילים למעלה, תורים שבוטלו או הושלמו בנפרד למטה - כדי שלא יתבלבלו עם תור חדש באותה שעה
  const active = appointments?.filter((a) => a.status === 'Confirmed') ?? []
  const history = appointments?.filter((a) => a.status !== 'Confirmed') ?? []

  function renderRow(a) {
    return (
      <li key={a.id} className="p-5">
        <div className="flex flex-wrap items-center justify-between gap-4">
          <div>
            <p className="font-semibold">{formatDateTime(appointmentStart(a))}</p>
            <p className="text-sm text-muted">
              {appointmentTreatment(a)}
              {isAdmin && appointmentClient(a) && ` · ${appointmentClient(a)}`}
              {a.notes && ` · "${a.notes}"`}
            </p>
          </div>
          <div className="flex items-center gap-3">
            <span className={`rounded-full px-3 py-1 text-xs font-semibold ${statusStyle[a.status] ?? 'bg-line'}`}>
              {statusLabel(a.status)}
            </span>
            {a.status === 'Confirmed' && (
              confirmId === a.id ? (
                <>
                  <button className="btn border border-danger/40 bg-white px-3 py-1.5 text-sm text-danger hover:bg-danger/5"
                    disabled={busyId === a.id} onClick={() => handleCancel(a.id)}>
                    {busyId === a.id ? 'מבטלת...' : 'כן, לבטל'}
                  </button>
                  <button className="text-sm text-muted hover:text-ink cursor-pointer" onClick={() => setConfirmId(null)}>
                    לא
                  </button>
                </>
              ) : (
                <button className="btn-ghost px-3 py-1.5 text-sm" onClick={() => { setConfirmId(a.id); setRowError(null) }}>
                  ביטול תור
                </button>
              )
            )}
          </div>
        </div>
        {rowError?.id === a.id && (
          <p className="mt-3 rounded-xl bg-danger/10 px-4 py-2 text-sm text-danger" role="alert">
            {rowError.text} - התור לא בוטל.
          </p>
        )}
      </li>
    )
  }

  return (
    <div className="space-y-8">
      <div>
        <h1 className="font-display text-4xl">{isAdmin ? 'כל התורים' : 'התורים שלי'}</h1>
        <p className="mt-2 text-muted">
          {isAdmin ? 'כל התורים במערכת.' : 'אפשר לבטל תור עד 24 שעות לפני מועדו.'}
        </p>
      </div>

      {message && <Notice type="success" title={message} onClose={() => setMessage(null)} />}
      {loadError && <Notice type="error" title={loadError} onClose={() => setLoadError(null)} />}

      {!appointments && !loadError && <p className="py-16 text-center text-muted">טוענת...</p>}

      {appointments && active.length === 0 && (
        <div className="card py-16 text-center text-muted">
          אין תורים פעילים. <Link to="/" className="text-gold underline underline-offset-4">לשעות הפנויות</Link>
        </div>
      )}

      {active.length > 0 && <ul className="card divide-y divide-line">{active.map(renderRow)}</ul>}

      {history.length > 0 && (
        <section className="space-y-3">
          <h2 className="font-display text-xl text-muted">תורים שבוטלו או הושלמו</h2>
          <ul className="card divide-y divide-line opacity-70">{history.map(renderRow)}</ul>
        </section>
      )}
    </div>
  )
}
