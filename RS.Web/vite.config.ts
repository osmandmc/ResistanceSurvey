import { fileURLToPath, URL } from 'node:url'
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

// The app is served by RS.MVC at /app from wwwroot/rsweb (wwwroot/app holds the old pages' scripts).
// In development (npm run dev), /api calls are forwarded to the locally running RS.MVC.
export default defineConfig({
  base: '/app/',
  plugins: [vue()],
  resolve: {
    alias: { '@': fileURLToPath(new URL('./src', import.meta.url)) },
  },
  build: {
    outDir: '../RS.MVC/wwwroot/rsweb',
    emptyOutDir: true,
  },
  server: {
    port: 5173,
    proxy: {
      '/api': process.env.RS_API_URL ?? 'http://localhost:56455',
    },
  },
})
