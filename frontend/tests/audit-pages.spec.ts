import { test, expect, type Page, type BrowserContext } from '@playwright/test'
import { fileURLToPath } from 'url'
import { dirname, join } from 'path'
import fs from 'fs'

// ES module __dirname replacement
const __filename = fileURLToPath(import.meta.url)
const __dirname = dirname(__filename)

const SCREENSHOTS_DIR = join(__dirname, '..', 'screenshots')

if (!fs.existsSync(SCREENSHOTS_DIR)) {
  fs.mkdirSync(SCREENSHOTS_DIR, { recursive: true })
}

// ─── 审核与稽查功能页面列表 ─────────────────────────────────────────────────
const AUDIT_PAGES = [
  { path: '/audits/list', name: '审核列表' },
  { path: '/audits/detail', name: '审核详情' },
  { path: '/audits/finding', name: '不符合项管理' },
]

// ─── 收集错误的工具函数 ─────────────────────────────────────────────────
async function setupErrorListeners(page: Page, pageName: string): Promise<{
  errors: string[]
  consoleErrors: string[]
  pageErrors: string[]
  httpErrors: string[]
  cleanup: () => void
}> {
  const errors: string[] = []
  const consoleErrors: string[] = []
  const pageErrors: string[] = []
  const httpErrors: string[] = []

  const consoleHandler = (msg: any) => {
    if (msg.type() === 'error') {
      const text = msg.text()
      consoleErrors.push(text)
      errors.push(`[CONSOLE] ${text}`)
    }
  }
  const pageErrorHandler = (err: Error) => {
    const msg = err.message
    pageErrors.push(msg)
    errors.push(`[PAGE] ${msg}`)
  }
  const responseHandler = async (response: any) => {
    if (response.status() >= 400) {
      // 排除已知的基础设施 401 错误（auth 刷新和 token 检查）
      const url = response.url()
      if (url.includes('/auth/refresh') || url.includes('/auth/login') ||
          url.includes('/api/v1/auth')) {
        return
      }
      let body = ''
      try { body = await response.text().slice(0, 300) } catch {}
      const httpErr = `[HTTP ${response.status()}] ${response.url()} → ${body}`
      httpErrors.push(httpErr)
      errors.push(httpErr)
    }
  }

  page.on('console', consoleHandler)
  page.on('pageerror', pageErrorHandler)
  page.on('response', responseHandler)

  return {
    errors,
    consoleErrors,
    pageErrors,
    httpErrors,
    cleanup: () => {
      page.off('console', consoleHandler)
      page.off('pageerror', pageErrorHandler)
      page.off('response', responseHandler)
    },
  }
}

// ─── Mock API 数据 ─────────────────────────────────────────────────
const MOCK_AUDITS = [
  {
    id: 1,
    auditCode: 'AUD-20260701-001',
    auditType: 'internal',
    title: '2026年Q1内部质量审核',
    description: '涵盖生产、检验、仓储全流程的季度内审',
    startDate: '2026-07-01',
    endDate: '2026-07-15',
    auditorId: 1,
    scope: '生产部、品质部、仓储部',
    status: 'in_progress',
    createdAt: '2026-06-28T10:00:00',
    updatedAt: '2026-07-01T08:00:00',
  },
  {
    id: 2,
    auditCode: 'AUD-20260615-002',
    auditType: 'process',
    title: '焊接工序过程审核',
    description: '针对焊接工序的专项过程审核',
    startDate: '2026-06-15',
    endDate: '2026-06-20',
    auditorId: 2,
    scope: '焊接车间',
    status: 'completed',
    createdAt: '2026-06-10T09:00:00',
    updatedAt: '2026-06-20T17:00:00',
  },
  {
    id: 3,
    auditCode: 'AUD-20260501-003',
    auditType: 'product',
    title: '产品交付质量审核',
    description: '针对客户交付产品的专项质量审核',
    startDate: '2026-05-01',
    endDate: '2026-05-10',
    auditorId: 3,
    scope: '成品包装与出货',
    status: 'archived',
    createdAt: '2026-04-25T08:00:00',
    updatedAt: '2026-05-10T16:00:00',
  },
]

const MOCK_FINDINGS = [
  {
    id: 1,
    auditId: 1,
    findingType: 'non_conformity',
    severity: 'major',
    description: '焊接温度未达到工艺要求',
    evidence: '温度记录显示温度低于标准值10°C',
    requirementRef: 'ISO 9001:2015 8.5.1',
    status: 'open',
    rectificationPlan: '',
    responsibleUserId: 5,
    rectificationDueDate: '2026-07-10',
    createdAt: '2026-07-02T10:00:00',
    updatedAt: '2026-07-02T10:00:00',
  },
  {
    id: 2,
    auditId: 1,
    findingType: 'opportunity',
    severity: 'minor',
    description: '检验记录格式不统一',
    evidence: '部分记录使用旧版表单',
    requirementRef: '公司内部质量管理规范',
    status: 'open',
    rectificationPlan: '',
    responsibleUserId: 6,
    rectificationDueDate: '2026-07-20',
    createdAt: '2026-07-03T14:00:00',
    updatedAt: '2026-07-03T14:00:00',
  },
  {
    id: 3,
    auditId: 1,
    findingType: 'conformity',
    severity: 'observation',
    description: '培训记录完整',
    evidence: '全部相关人员已完成年度培训',
    requirementRef: 'ISO 9001:2015 7.2',
    status: 'closed',
    rectificationPlan: '',
    responsibleUserId: 0,
    rectificationDueDate: '',
    createdAt: '2026-07-04T09:00:00',
    updatedAt: '2026-07-04T09:00:00',
  },
]

// ─── 注册 API Mock ─────────────────────────────────────────────────
function mockAuditApis(context: BrowserContext) {
  // GET /api/v1/m13/audits (list)
  context.route(/\/api\/v1\/m13\/audits(\?|$)/, async (route) => {
    const request = route.request()
    if (request.method() === 'POST') {
      // Create
      const data = JSON.parse(request.postData() || '{}')
      await route.fulfill({
        status: 201,
        contentType: 'application/json',
        body: JSON.stringify({ ...data, id: MOCK_AUDITS.length + 1, createdAt: new Date().toISOString(), updatedAt: new Date().toISOString() }),
      })
      return
    }
    if (request.method() === 'DELETE') {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ success: true }),
      })
      return
    }

    // Parse pagination from URL query string
    const url = new URL(request.url())
    const page = parseInt(url.searchParams.get('page') || '1')
    const pageSize = parseInt(url.searchParams.get('pageSize') || '20')
    const start = (page - 1) * pageSize
    const items = MOCK_AUDITS.slice(start, start + pageSize)
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ items, total: MOCK_AUDITS.length }),
    })
  })

  // GET /api/v1/m13/audits/:id/findings
  context.route(/\/api\/v1\/m13\/audits\/\d+\/findings(\?|$)/, async (route) => {
    const request = route.request()
    if (request.method() === 'POST') {
      const data = JSON.parse(request.postData() || '{}')
      await route.fulfill({
        status: 201,
        contentType: 'application/json',
        body: JSON.stringify({ ...data, id: 4, createdAt: new Date().toISOString(), updatedAt: new Date().toISOString() }),
      })
      return
    }
    const url = request.url()
    const match = url.match(/\/audits\/(\d+)\/findings/)
    if (!match) return await route.continue()
    const auditId = parseInt(match[1])
    const filtered = MOCK_FINDINGS.filter(f => f.auditId === auditId)
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(filtered),
    })
  })

  // DELETE /api/v1/m13/audits/findings/:id
  context.route(/\/api\/v1\/m13\/audits\/findings\/\d+\/?$/, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ success: true }),
    })
  })

  // PUT /api/v1/m13/audits/findings/:id
  context.route(/\/api\/v1\/m13\/audits\/findings\/\d+\/?$/, async (route) => {
    const request = route.request()
    if (request.method() === 'PUT') {
      const data = JSON.parse(request.postData() || '{}')
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ ...data, id: 1, updatedAt: new Date().toISOString() }),
      })
    } else {
      await route.continue()
    }
  })

  // POST /api/v1/m13/audits/findings/:id/verify
  context.route(/\/api\/v1\/m13\/audits\/findings\/\d+\/verify/, async (route) => {
    const data = JSON.parse(route.request().postData() || '{}')
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ ...data, status: 'verified', updatedAt: new Date().toISOString() }),
    })
  })

  // GET /api/v1/m13/audits/:id
  context.route(/\/api\/v1\/m13\/audits\/\d+\/?$/, async (route) => {
    const request = route.request()
    const url = request.url()
    const match = url.match(/\/audits\/(\d+)/)
    if (!match) return await route.continue()
    const id = parseInt(match[1])
    const audit = MOCK_AUDITS.find(a => a.id === id)
    if (!audit) {
      await route.fulfill({ status: 404, contentType: 'application/json', body: JSON.stringify({ message: '审核不存在' }) })
      return
    }
    if (request.method() === 'GET') {
      await route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify(audit) })
    } else if (request.method() === 'PUT') {
      const data = JSON.parse(request.postData() || '{}')
      await route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify({ ...audit, ...data, updatedAt: new Date().toISOString() }) })
    } else if (request.method() === 'DELETE') {
      await route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify({ success: true }) })
    } else {
      await route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify(audit) })
    }
  })
}

// ─── 辅助: 注入认证 ─────────────────────────────────────────────────
async function injectAuth(page: Page) {
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
}

// ─── Mock 基础设施 API（避免 401 干扰）────────────────────────────────────────────────
function mockInfrastructureApis(context: BrowserContext) {
  // organizations/tree
  context.route(/\/api\/v1\/organizations\/tree/, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify([
        { id: 1, name: '总公司', level: 'company', path: '/' },
        { id: 2, name: '生产部', level: 'workshop', path: '/production' },
        { id: 3, name: '品质部', level: 'workshop', path: '/quality' },
      ]),
    })
  })

  // auth/refresh
  context.route(/\/api\/v1\/auth\/refresh/, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ token: 'mock-test-token-12345', refreshToken: 'mock-refresh-token' }),
    })
  })
}

// ─── 测试: 每个审核与稽查页面 ─────────────────────────────────────────────────
test.describe('审核与稽查功能页面错误检查', () => {
  const allResults: Array<{ name: string; path: string; errors: string[]; consoleErrors: string[]; pageErrors: string[]; httpErrors: string[]; passed: boolean }> = []

  for (const { path, name } of AUDIT_PAGES) {
    test(`${name} (${path})`, async ({ page, context }) => {
      // 注册 API Mock
      mockAuditApis(context)
      mockInfrastructureApis(context)

      // 先导航到登录页以设置 origin
      await page.goto('/login', { waitUntil: 'domcontentloaded', timeout: 15000 })
      await injectAuth(page)
      await page.waitForTimeout(300)

      // 清除错误监听，重新开始
      const { errors, consoleErrors, pageErrors, httpErrors, cleanup } = await setupErrorListeners(page, name)

      // 导航到目标页面
      console.log(`\n--- 访问: ${name} (${path}) ---`)
      await page.goto(path, { waitUntil: 'networkidle', timeout: 20000 })
      await page.waitForTimeout(3000)

      // 截图
      const safeName = name.replace(/[/\\s]/g, '_')
      await page.screenshot({ path: join(SCREENSHOTS_DIR, `${safeName}.png`), fullPage: true })

      // 检查页面标题
      const title = await page.title()
      console.log(`  页面标题: ${title}`)

      // 检查页面是否成功渲染
      const hasContent = await page.$('.page-container') || await page.$('.el-card') || await page.$('.data-card')
      if (hasContent) {
        console.log('  ✅ 页面主体已渲染')
      } else {
        console.log('  ⚠️ 未检测到预期的页面主体元素')
      }

      // 检查表格数据
      const tableRows = await page.$$('.el-table__row')
      console.log(`  表格数据: ${tableRows.length} 条`)

      // 检查错误
      if (errors.length > 0) {
        console.log(`  ❌ 发现 ${errors.length} 个错误:`)
        for (const err of errors) console.log(`    ${err}`)
      } else {
        console.log('  ✅ 无错误')
      }

      allResults.push({ name, path, errors, consoleErrors, pageErrors, httpErrors, passed: errors.length === 0 })
      cleanup()
    })
  }

  // ─── 测试: 带参数的详细页面 ─────────────────────────────────────────────────
  test('审核详情 (带ID参数)', async ({ page, context }) => {
    mockAuditApis(context)
    mockInfrastructureApis(context)
    await page.goto('/login', { waitUntil: 'domcontentloaded', timeout: 15000 })
    await injectAuth(page)
    await page.waitForTimeout(300)

    const { errors, cleanup } = await setupErrorListeners(page, '审核详情(id=1)')

    console.log('\n--- 访问: 审核详情(id=1) ---')
    await page.goto('/audits/detail?id=1', { waitUntil: 'networkidle', timeout: 20000 })
    await page.waitForTimeout(3000)

    const title = await page.title()
    console.log(`  页面标题: ${title}`)

    const descs = await page.$$('.el-descriptions-item')
    console.log(`  审核详情字段数: ${descs?.length || 0}`)

    const findingsTable = await page.$$('.el-table__row')
    console.log(`  不符合项表格: ${findingsTable.length} 条`)

    if (errors.length > 0) {
      console.log(`  ❌ 发现 ${errors.length} 个错误:`)
      for (const err of errors) console.log(`    ${err}`)
    } else {
      console.log('  ✅ 无错误')
    }

    allResults.push({ name: '审核详情(id=1)', path: '/audits/detail?id=1', errors, consoleErrors: [], pageErrors: [], httpErrors: [], passed: errors.length === 0 })
    cleanup()
  })

  test('不符合项管理 (带auditId参数)', async ({ page, context }) => {
    mockAuditApis(context)
    mockInfrastructureApis(context)
    await page.goto('/login', { waitUntil: 'domcontentloaded', timeout: 15000 })
    await injectAuth(page)
    await page.waitForTimeout(300)

    const { errors, cleanup } = await setupErrorListeners(page, '不符合项(auditId=1)')

    console.log('\n--- 访问: 不符合项(auditId=1) ---')
    await page.goto('/audits/finding?auditId=1', { waitUntil: 'networkidle', timeout: 20000 })
    await page.waitForTimeout(3000)

    const tableRows = await page.$$('.el-table__row')
    console.log(`  表格数据: ${tableRows.length} 条`)

    if (errors.length > 0) {
      console.log(`  ❌ 发现 ${errors.length} 个错误:`)
      for (const err of errors) console.log(`    ${err}`)
    } else {
      console.log('  ✅ 无错误')
    }

    allResults.push({ name: '不符合项(auditId=1)', path: '/audits/finding?auditId=1', errors, consoleErrors: [], pageErrors: [], httpErrors: [], passed: errors.length === 0 })
    cleanup()
  })

  // ─── 结果汇总 ─────────────────────────────────────────────────
  test('结果汇总', async ({}) => {
    console.log('\n\n============================')
    console.log('   审核与稽查页面检查汇总')
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
      const reportPath = join(SCREENSHOTS_DIR, 'audit-error-report.txt')
      let report = '审核与稽查页面错误报告\n'
      report += '='.repeat(50) + '\n\n'
      for (const r of allResults) {
        if (!r.passed) {
          report += `${r.name} (${r.path}):\n`
          if (r.consoleErrors.length > 0) {
            report += '  控制台错误:\n'
            for (const err of r.consoleErrors) report += `    ${err}\n`
          }
          if (r.pageErrors.length > 0) {
            report += '  页面错误:\n'
            for (const err of r.pageErrors) report += `    ${err}\n`
          }
          if (r.httpErrors.length > 0) {
            report += '  HTTP响应错误:\n'
            for (const err of r.httpErrors) report += `    ${err}\n`
          }
          report += '\n'
        }
      }
      fs.writeFileSync(reportPath, report)
      console.log(`详细错误报告已保存至: ${reportPath}`)
    }
  })
})
