export function HeartIcon({ className = 'h-5 w-5' }) {
  // לב בקו דק (לא מלא), כמו ב-favicon
  return (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.6"
      strokeLinecap="round" strokeLinejoin="round" className={className} aria-hidden="true">
      <path d="M12 20.5s-7.5-4.6-7.5-10.2A4.3 4.3 0 0 1 12 7.6a4.3 4.3 0 0 1 7.5 2.7c0 5.6-7.5 10.2-7.5 10.2z" />
    </svg>
  )
}

export default function Logo() {
  return (
    <div className="flex items-baseline gap-3">
      <span className="font-display text-3xl font-medium leading-none">
        שרה
        <HeartIcon className="mr-1 inline h-4 w-4 align-top text-gold" />
      </span>
      <span className="text-sm text-muted">טאצ' שתאהבי</span>
    </div>
  )
}
