import { createContext, useContext, useState } from 'react'
import { AUTH_STORAGE_KEY, readAuth } from '../api/client.js'
import * as endpoints from '../api/endpoints.js'

// Context אחד שמחזיק את המשתמשת המחוברת, כדי שכל מסך יוכל לדעת מי מחוברת ומה התפקיד שלה.
const AuthContext = createContext(null)

export function AuthProvider({ children }) {
  // בטעינת הדף קוראים מה-localStorage, כך שרענון לא מנתק
  const [user, setUser] = useState(() => readAuth())

  async function login(email, password) {
    // השרת מחזיר { token, expiresAtUtc, userId, fullName, role }
    const data = await endpoints.login(email, password)
    localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(data))
    setUser(data)
    return data
  }

  async function register(form) {
    await endpoints.register(form)
    // אחרי הרשמה מוצלחת מתחברים מיד, כדי לא לבקש את הסיסמה פעמיים
    return login(form.email, form.password)
  }

  function logout() {
    localStorage.removeItem(AUTH_STORAGE_KEY)
    setUser(null)
  }

  const value = {
    user,
    isAdmin: user?.role === 'Admin',
    isClient: user?.role === 'Client',
    login,
    register,
    logout,
  }

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

// eslint-disable-next-line react-refresh/only-export-components
export function useAuth() {
  return useContext(AuthContext)
}
