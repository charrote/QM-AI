import { test, expect, type Page } from '@playwright/test'
import { fileURLToPath } from 'url'
import { dirname, join } from 'path'
import fs from 'fs'

// ES module __dirname replacement
const __filename = fileURLToPath(import.meta.url)
const __dirname = dirname(__filename)

const SCREENSHOTS_DIR = join(__dirname, '..', 'screenshots')

// Ensure screenshots directory exists
if (!fs.existsSync(SCREENSHOTS_DIR)) {
  fs.mkdirSync(SCREENSHOTS_DIR, { recursive: true })
}

// ─── AI 质量分析功能页面列表 ─────────────────────────────────────────────────
const AI_PAGES = [
  { path: '/ai/alerts', name: '预警中心' },
  { path: '/ai/root-cause', name: '根因分析' },
  { path: '/ai/models', name: '模型管理' },
  { path: '/ipqc/risk', name: 'AI风险仪表盘' },
  { path: '/equipment-link/correlation', name: '质量关联分析' },
  { path: '/reports/dashboard', name: '质量仪表盘' },
]

// ─── 收集错误的工具函数 ─────────────────────────────────────────────────
async function setupErrorListeners(page: Page, pageName: string): Promise<{
  errors: string[]
  cleanup: () => void
}> {
  const errors: string[] = []
  const consoleHandler = (msg: any) => {
    if (msg.type() === 'error') {
      errors.push(`[CONSOLE] ${msg.text()}`)
    }
  }
  const pageErrorHandler = (err: Error) => {
    errors.push(`[PAGE] ${err.message}`)
  }
  const responseHandler = async (response: any) => {
    if (response.status() >= 400) {
      let body = ''
      try { body = await response.text().slice(0, 300) } catch {}
      errors.push(`[HTTP ${response.status()}] ${response.url()} → ${body}`)
    }
  }

  page.on('console', consoleHandler)
  page.on('pageerror', pageErrorHandler)
  page.on('response', responseHandler)

  return {
    errors,
    cleanup: () => {
      page.off('console', consoleHandler)
      page.off('pageerror', pageErrorHandler)
      page.off('response', responseHandler)
    },
  }
}

// ─── 测试: 每个 AI 质量分析页面 ─────────────────────────────────────────────────
test.describe('AI 质量分析功能页面错误检查', () => {
  const allResults: Array<{ name: string; errors: string[]; passed: boolean }> = []

  for (const { path, name } of AI_PAGES) {
    test(`${name} (${path})`, async ({ page }) => {
      // 1. 先导航到登录页（绕过 AuthGuard）
      await page.goto('/login', { waitUntil: 'domcontentloaded', timeout: 15000 })
      await page.waitForTimeout(500)

      // 2. 注入模拟认证信息到 localStorage
      await page.evaluate(() => {
        localStorage.setItem('qm-ai-token', 'mock-test-token-12345')
        localStorage.setItem('qm-ai-refresh-token', 'mock-refresh-token')
        localStorage.setItem('qm-ai-user', JSON.stringify({
          id: 1,
          username: 'admin',
          displayName: '管理员',
          role: '管理员',
        }))
      })

      // 3. 清除错误监听器，重新开始收集
      const { errors, cleanup } = await setupErrorListeners(page, name)

      // 4. 导航到目标页面
      console.log(`\n--- 访问: ${name} (${path}) ---`)
      await page.goto(path, { waitUntil: 'networkidle', timeout: 20000 })
      await page.waitForTimeout(2000)

      // 5. 截图
      const safeName = name.replace(/[/\\s]/g, '_')
      await page.screenshot({ path: join(SCREENSHOTS_DIR, `${safeName}.png`), fullPage: true })

      // 6. 检查页面标题
      const title = await page.title()
      console.log(`  页面标题: ${title}`)

      // 7. 检查是否成功渲染
      const hasContent = await page.$('.page-container') || await page.$('.ipqc-content') ||
        await page.$('.page-header-banner') || await page.$('.el-page-header') ||
        await page.$('.el-card') || await page.$('.data-card')
      if (hasContent) {
        console.log('  ✅ 页面主体已渲染')
      } else {
        console.log('  ⚠️ 未检测到预期的页面主体元素')
      }

      // 8. 检查错误
      if (errors.length > 0) {
        console.log(`  ❌ 发现 ${errors.length} 个错误:`)
        for (const err of errors) {
          console.log(`    ${err}`)
        }
      } else {
        console.log('  ✅ 无错误')
      }

      allResults.push({ name, errors, passed: errors.length === 0 })
      cleanup()
    })
  }

  test('结果汇总', async ({}) => {
    console.log('\n\n============================')
    console.log('   AI 质量分析页面检查汇总')
    console.log('============================')
    let passCount = 0
    let failCount = 0
    for (const r of allResults) {
      if (r.passed) {
        console.log(`  ✅ ${r.name}: 通过`)
        passCount++
      } else {
        console.log(`  ❌ ${r.name}: 失败 (${r.errors.length} 个错误)`)
        failCount++
        for (const err of r.errors) {
          console.log(`     ${err}`)
        }
      }
    }
    console.log(`\n  总计: ${allResults.length} 个页面, 通过 ${passCount}, 失败 ${failCount}`)
    console.log('============================\n')

    if (failCount > 0) {
      const reportPath = join(SCREENSHOTS_DIR, 'error-report.txt')
      let report = 'AI 质量分析页面错误报告\n'
      report += '='.repeat(50) + '\n\n'
      for (const r of allResults) {
        if (!r.passed) {
          report += `${r.name}:\n`
          for (const err of r.errors) {
            report += `  ${err}\n`
          }
          report += '\n'
        }
      }
      fs.writeFileSync(reportPath, report)
      console.log(`详细错误报告已保存至: ${reportPath}`)
    }
  })
})

// ─── 测试: 登录页面 ─────────────────────────────────────────────────
test.describe('登录页面检查', () => {
  test('登录页面可正常加载', async ({ page }) => {
    const { errors, cleanup } = await setupErrorListeners(page, '登录页面')

    await page.goto('/login', { waitUntil: 'networkidle', timeout: 15000 })
    await page.waitForTimeout(1000)

    const title = await page.title()
    console.log(`  登录页面标题: ${title}`)

    const hasForm = await page.$('.form-container')
    console.log(hasForm ? '  ✅ 登录表单已渲染' : '  ⚠️ 登录表单未渲染')

    if (errors.length > 0) {
      console.log(`  ❌ 发现 ${errors.length} 个错误:`)
      for (const err of errors) console.log(`    ${err}`)
    } else {
      console.log('  ✅ 登录页面无错误')
    }
    cleanup()
  })
})
