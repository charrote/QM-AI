import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import { resolve } from 'path'

export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      '@': resolve(__dirname, 'src'),
    },
  },
  build: {
    rollupOptions: {
      output: {
        manualChunks(id) {
          // Element Plus UI 组件库单独打包
          if (id.includes('node_modules') && id.includes('element-plus')) {
            return 'vendor-element-plus'
          }
          // ECharts 图表库单独打包（体积最大）
          if (id.includes('node_modules') && id.includes('echarts')) {
            return 'vendor-echarts'
          }
          // VueUse 工具库单独打包
          if (id.includes('node_modules') && id.includes('@vueuse')) {
            return 'vendor-vueuse'
          }
        },
      },
    },
  },
  server: {
    port: 5610,
    proxy: {
      '/api': {
        target: 'http://localhost:5611',
        changeOrigin: true,
      },
    },
  },
})
