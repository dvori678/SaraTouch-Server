import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

// כתובת השרת (ASP.NET Core). אם הפורט אצלך שונה, משנים רק כאן.
const API_TARGET = 'https://localhost:7127'

export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    port: 5173,
    // Proxy: כל בקשה מהדפדפן ל-/api/... עוברת דרך שרת הפיתוח של Vite אל השרת שלנו.
    // למה: מבחינת הדפדפן הבקשה נשארת באותו origin (localhost:5173), ולכן אין צורך ב-CORS בשרת,
    // ואין בעיה עם תעודת ה-HTTPS של סביבת הפיתוח.
    proxy: {
      '/api': {
        target: API_TARGET,
        changeOrigin: true,
        secure: false, // תעודת הפיתוח של dotnet היא self-signed
      },
    },
  },
})
