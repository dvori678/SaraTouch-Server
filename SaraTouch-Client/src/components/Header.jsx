import { Link, NavLink, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext.jsx'
import Logo from './Logo.jsx'

const navClass = ({ isActive }) =>
  `px-1 pb-1 border-b transition ${isActive ? 'border-gold text-ink' : 'border-transparent text-muted hover:text-ink'}`

export default function Header() {
  const { user, isAdmin, logout } = useAuth()
  const navigate = useNavigate()

  function handleLogout() {
    logout()
    navigate('/login')
  }

  return (
    <header className="border-b border-line bg-cream/90">
      <div className="mx-auto flex max-w-4xl flex-wrap items-center justify-between gap-4 px-4 py-5">
        <Link to="/"><Logo /></Link>

        <nav className="flex items-center gap-6 text-sm">
          <NavLink to="/" end className={navClass}>שעות פנויות</NavLink>
          {user && (
            <NavLink to="/appointments" className={navClass}>
              {isAdmin ? 'כל התורים' : 'התורים שלי'}
            </NavLink>
          )}
          {user ? (
            <div className="flex items-center gap-3">
              <span className="text-muted">שלום, {user.fullName}{isAdmin && ' (מנהלת)'}</span>
              <button onClick={handleLogout} className="text-muted underline-offset-4 hover:text-ink hover:underline cursor-pointer">
                יציאה
              </button>
            </div>
          ) : (
            <NavLink to="/login" className={navClass}>התחברות</NavLink>
          )}
        </nav>
      </div>
    </header>
  )
}
