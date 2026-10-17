import { useState } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext.jsx'
import { getErrorMessage, getFieldErrors } from '../api/client.js'
import Notice from '../components/Notice.jsx'

const DEMO_USERS = [
  { label: 'לקוחה', email: 'client@saratouch.local', password: 'Client123!' },
  { label: 'לקוחה 2', email: 'client2@saratouch.local', password: 'Client123!' },
  { label: 'שרה (מנהלת)', email: 'sara@saratouch.local', password: 'Admin123!' },
]

export default function LoginPage() {
  const { login, register } = useAuth()
  const navigate = useNavigate()
  const [params] = useSearchParams()

  const [mode, setMode] = useState('login') // 'login' | 'register'
  const [form, setForm] = useState({ fullName: '', email: '', phone: '', password: '' })
  const [error, setError] = useState(null)
  const [fieldErrors, setFieldErrors] = useState([])
  const [busy, setBusy] = useState(false)

  const update = (field) => (e) => setForm({ ...form, [field]: e.target.value })

  async function handleSubmit(e) {
    e.preventDefault()
    setBusy(true)
    setError(null)
    setFieldErrors([])
    try {
      if (mode === 'login') {
        await login(form.email, form.password)
      } else {
        await register({
          fullName: form.fullName,
          email: form.email,
          password: form.password,
          phone: form.phone || undefined,
        })
      }
      navigate('/')
    } catch (err) {
      const status = err.response?.status
      if (mode === 'login' && status === 401) setError('המייל או הסיסמה לא נכונים.')
      else if (status === 409) setError(getErrorMessage(err, 'המייל הזה כבר רשום. אפשר להתחבר.'))
      else setError(getErrorMessage(err))
      setFieldErrors(getFieldErrors(err))
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="mx-auto max-w-md space-y-6">
      <div className="text-center">
        <h1 className="font-display text-4xl">{mode === 'login' ? 'ברוכה השבה' : 'הרשמה'}</h1>
        <p className="mt-2 text-muted">
          {mode === 'login' ? 'התחברי כדי לקבוע תור' : 'כמה פרטים ואת בפנים'}
        </p>
      </div>

      {params.get('expired') && !error && (
        <Notice type="error">החיבור פג. התחברי שוב, בבקשה.</Notice>
      )}

      <form onSubmit={handleSubmit} className="card space-y-4 p-6 sm:p-8">
        {mode === 'register' && (
          <>
            <div>
              <label className="label" htmlFor="fullName">שם מלא</label>
              <input id="fullName" className="input" value={form.fullName} onChange={update('fullName')} required />
            </div>
            <div>
              <label className="label" htmlFor="phone">טלפון (לא חובה)</label>
              <input id="phone" className="input" dir="ltr" value={form.phone} onChange={update('phone')} />
            </div>
          </>
        )}
        <div>
          <label className="label" htmlFor="email">מייל</label>
          <input id="email" type="email" className="input" dir="ltr" value={form.email} onChange={update('email')} required />
        </div>
        <div>
          <label className="label" htmlFor="password">סיסמה</label>
          <input id="password" type="password" className="input" dir="ltr" value={form.password} onChange={update('password')} required />
        </div>

        {error && (
          <Notice type="error" title={error}>
            {fieldErrors.length > 0 && (
              <ul className="mt-1 list-disc pr-5">{fieldErrors.map((m) => <li key={m}>{m}</li>)}</ul>
            )}
          </Notice>
        )}

        <button className="btn-primary w-full" disabled={busy}>
          {busy ? 'רגע...' : mode === 'login' ? 'התחברות' : 'הרשמה'}
        </button>

        <p className="text-center text-sm text-muted">
          {mode === 'login' ? 'עוד אין לך חשבון? ' : 'כבר רשומה? '}
          <button
            type="button"
            className="text-gold underline-offset-4 hover:underline cursor-pointer"
            onClick={() => { setMode(mode === 'login' ? 'register' : 'login'); setError(null); setFieldErrors([]) }}
          >
            {mode === 'login' ? 'להרשמה' : 'להתחברות'}
          </button>
        </p>
      </form>

      {/* קיצור דרך למשתמשות הדמו מה-Seed, נוח להדגמה */}
      {mode === 'login' && (
        <div className="text-center text-sm text-muted">
          <p className="mb-2">משתמשות דמו:</p>
          <div className="flex flex-wrap justify-center gap-2">
            {DEMO_USERS.map((u) => (
              <button
                key={u.email}
                type="button"
                className="rounded-full border border-line bg-white px-3 py-1 hover:border-gold cursor-pointer"
                onClick={() => setForm({ ...form, email: u.email, password: u.password })}
              >
                {u.label}
              </button>
            ))}
          </div>
        </div>
      )}
    </div>
  )
}
