// הודעה עדינה בראש המסך: הצלחה, שגיאה או התנגשות (409)
const styles = {
  success: 'border-success/30 bg-success/10 text-success',
  error: 'border-danger/30 bg-danger/10 text-danger',
  conflict: 'border-danger/30 bg-danger/5 text-danger',
}

export default function Notice({ type = 'error', title, children, onClose }) {
  if (!children && !title) return null
  return (
    <div role="alert" className={`flex items-start justify-between gap-4 rounded-2xl border px-5 py-4 ${styles[type]}`}>
      <div>
        {title && <p className="font-semibold">{title}</p>}
        {children && <div className="text-sm opacity-90">{children}</div>}
      </div>
      {onClose && (
        <button onClick={onClose} className="text-lg leading-none opacity-60 hover:opacity-100 cursor-pointer" aria-label="סגירה">
          ×
        </button>
      )}
    </div>
  )
}
