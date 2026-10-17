import { Navigate, Route, Routes } from 'react-router-dom'
import { useAuth } from './auth/AuthContext.jsx'
import Header from './components/Header.jsx'
import LoginPage from './pages/LoginPage.jsx'
import SlotsPage from './pages/SlotsPage.jsx'
import AppointmentsPage from './pages/AppointmentsPage.jsx'

// מסך שדורש התחברות. זו רק נוחות למשתמשת: ההרשאה האמיתית נבדקת בשרת ([Authorize]).
function RequireAuth({ children }) {
  const { user } = useAuth()
  return user ? children : <Navigate to="/login" replace />
}

export default function App() {
  return (
    <div className="min-h-screen">
      <Header />
      <main className="mx-auto max-w-4xl px-4 py-10">
        <Routes>
          <Route path="/" element={<SlotsPage />} />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/appointments" element={<RequireAuth><AppointmentsPage /></RequireAuth>} />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </main>
    </div>
  )
}
