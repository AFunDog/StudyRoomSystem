import { defineConfig } from 'vite'
import path from 'node:path'
import vue from '@vitejs/plugin-vue'
import tailwindcss from '@tailwindcss/vite'

// https://vite.dev/config/
export default defineConfig(({ command, mode }) => {
  const isDev = command === 'serve'

  return {
    server: {
      host: '0.0.0.0',
      proxy: isDev ? {
        '/api': {
          target: 'http://localhost:5106',
          changeOrigin: true,
        }
      } : undefined
    },
    plugins: [vue(), tailwindcss()],
    resolve: {
      alias: {
        '@': path.resolve(__dirname, './src'),
      },
    },
  }
})
