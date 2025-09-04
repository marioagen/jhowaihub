import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import path from 'path'
import fs from 'node:fs';
import pathNode from 'node:path';
const isDev = process.env.NODE_ENV === 'development';

export default defineConfig({
  build: {
    sourcemap: false,
    minify: 'terser',
    terserOptions: {
      compress: {
        drop_console: true,
        drop_debugger: true,
      },
    },
  },
  base: './',
  plugins: [
    vue(),
  ],
  server: (() => {
    if (!isDev) return undefined;

    const keyPath = path.resolve(process.cwd(), 'localhost-key.pem');
    const certPath = path.resolve(process.cwd(), 'localhost.pem');

    if (fs.existsSync(keyPath) && fs.existsSync(certPath)) {
      return {
        https: {
          key: fs.readFileSync(keyPath),
          cert: fs.readFileSync(certPath),
        },
        host: 'localhost',
        port: 3000
      };
    }

    console.warn('Certificados HTTPS não encontrados. Usando HTTP em modo desenvolvimento.');
    return {
      host: 'localhost',
      port: 3000
    };
  })(),
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
      'vue$': 'vue/dist/vue.runtime.esm-bundler.js',
      'vue-i18n$': 'vue-i18n/dist/vue-i18n.runtime.esm-bundler.js',
    },
    extensions: ['.vue', '.js', '.json']
  },
  define: {
    __VUE_PROD_HYDRATION_MISMATCH_DETAILS__: 'false'
  },
  logLevel: 'info'
})
