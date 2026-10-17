import { useCallback, useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { getAvailableSlots } from '../api/endpoints.js'
import { getErrorMessage } from '../api/client.js'
import { dayKey, formatDay, formatTime, slotEnd, slotStart } from '../api/format.js'
import BookingDialog from '../components/BookingDialog.jsx'
import Notice from '../components/Notice.jsx'
import Pagination from '../components/Pagination.jsx'
import { HeartIcon } from '../components/Logo.jsx'

const PAGE_SIZE = 12

// מקבצים את הסלוטים של העמוד הנוכחי לפי יום (בשעון ישראל)
function groupByDay(slots) {
  const groups = new Map()
  for (const slot of slots) {
    const start = slotStart(slot)
    if (!start) continue
    const key = dayKey(start)
    if (!groups.has(key)) groups.set(key, { label: formatDay(start), slots: [] })
    groups.get(key).slots.push(slot)
  }
  return [...groups.values()]
}

export default function SlotsPage() {
  const [page, setPage] = useState(1)
  const [data, setData] = useState(null) // PagedResult: { items, page, pageSize, totalCount, totalPages }
  const [loading, setLoading] = useState(true)
  const [loadError, setLoadError] = useState(null)
  const [selected, setSelected] = useState(null)
  const [notice, setNotice] = useState(null) // { type, title, text }

  const load = useCallback(async (pageToLoad) => {
    setLoading(true)
    setLoadError(null)
    try {
      const result = await getAvailableSlots(pageToLoad, PAGE_SIZE)
      // אם העמוד התרוקן (למשל אחרי שהסלוט האחרון בו נתפס), חוזרים עמוד אחורה
      if (result.items.length === 0 && pageToLoad > 1) {
        setPage(pageToLoad - 1)
        return
      }
      setData(result)
    } catch (err) {
      setLoadError(getErrorMessage(err, 'לא הצלחנו לטעון את השעות הפנויות.'))
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    load(page)
  }, [page, load])

  function handleBooked() {
    setSelected(null)
    setNotice({ type: 'success', title: 'התור שלך נקבע ♡', text: 'אפשר לראות אותו ב"התורים שלי".' })
    load(page)
  }

  // 409: השעה נתפסה. מציגים הודעה עדינה ומרעננים את הרשימה, כך שהשעה שנתפסה נעלמת ממנה.
  function handleConflict(serverMessage) {
    setSelected(null)
    setNotice({
      type: 'conflict',
      title: 'אוי, השעה הזו נתפסה הרגע',
   text: 'בחרי שעה אחרת.',    })
    load(page)
  }

  const groups = data ? groupByDay(data.items) : []

  return (
    <div className="space-y-8">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <h1 className="font-display text-4xl">שעות פנויות</h1>
          <p className="mt-2 text-muted">בחרי שעה שנוחה לך, והיא שלך.</p>
        </div>
        <button className="btn-ghost text-sm" onClick={() => load(page)} disabled={loading}>
          רענון
        </button>
      </div>

      {notice && (
        <Notice type={notice.type} title={notice.title} onClose={() => setNotice(null)}>
          {notice.text}
          {notice.type === 'success' && (
            <Link to="/appointments" className="mr-2 underline underline-offset-4">לתורים שלי</Link>
          )}
        </Notice>
      )}

      {loadError && <Notice type="error" title={loadError} />}

      {loading && !data && <p className="py-16 text-center text-muted">טוענת...</p>}

      {data && data.items.length === 0 && (
        <div className="card py-16 text-center text-muted">
          <HeartIcon className="mx-auto mb-3 h-6 w-6 text-gold" />
          כרגע אין שעות פנויות. כדאי לבדוק שוב בקרוב.
        </div>
      )}

      <div className={`space-y-6 transition ${loading ? 'opacity-60' : ''}`}>
        {groups.map((group) => (
          <section key={group.label} className="card p-5 sm:p-6">
            <h2 className="mb-4 font-display text-xl">{group.label}</h2>
            <div className="grid grid-cols-3 gap-3 sm:grid-cols-4 md:grid-cols-6">
              {group.slots.map((slot) => {
                const end = slotEnd(slot)
                return (
                  <button
                    key={slot.id}
                    onClick={() => setSelected(slot)}
                    className="rounded-xl border border-line bg-cream px-2 py-3 text-center transition hover:border-gold hover:bg-white cursor-pointer"
                  >
                    <span className="block font-semibold" dir="ltr">{formatTime(slotStart(slot))}</span>
                    {end && <span className="block text-xs text-muted" dir="ltr">עד {formatTime(end)}</span>}
                  </button>
                )
              })}
            </div>
          </section>
        ))}
      </div>

      {data && (
        <Pagination
          page={data.page}
          totalPages={data.totalPages}
          totalCount={data.totalCount}
          disabled={loading}
          onChange={setPage}
        />
      )}

      {selected && (
        <BookingDialog
          slot={selected}
          onClose={() => setSelected(null)}
          onBooked={handleBooked}
          onConflict={handleConflict}
        />
      )}
    </div>
  )
}
