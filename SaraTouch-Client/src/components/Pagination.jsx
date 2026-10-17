// ניווט בין עמודים. כל לחיצה מבקשת מהשרת עמוד חדש (לא חיתוך בזיכרון).
export default function Pagination({ page, totalPages, totalCount, onChange, disabled }) {
  if (!totalPages || totalPages <= 1) return null

  return (
    <div className="flex items-center justify-between gap-4 pt-2 text-sm">
      <button className="btn-ghost" disabled={disabled || page <= 1} onClick={() => onChange(page - 1)}>
        → הקודם
      </button>
      <span className="text-muted">
        עמוד {page} מתוך {totalPages}
        {totalCount != null && <span className="hidden sm:inline"> · {totalCount} שעות פנויות</span>}
      </span>
      <button className="btn-ghost" disabled={disabled || page >= totalPages} onClick={() => onChange(page + 1)}>
        הבא ←
      </button>
    </div>
  )
}
