import { test, expect, type Page } from '@playwright/test'

test('审核详情页控制台错误检查', async ({ page }) => {
  const consoleMessages: string[] = []
  const pageErrors: string[] = []
  
  page.on('console', msg => {
    if (msg.type() === 'error') {
      consoleMessages.push(msg.text())
    }
  })
  page.on('pageerror', err => {
    pageErrors.push(err.message)
  })

  // 先导航到登录页设置 origin
  await page.goto('/login', { waitUntil: 'domcontentloaded', timeout: 15000 })
  
  // 设置认证
  await page.evaluate(() => {
    localStorage.setItem('qm-ai-token', 'mock-test-token')
    localStorage.setItem('qm-ai-refresh-token', 'mock-refresh')
    localStorage.setItem('qm-ai-user', JSON.stringify({ id: 1, username: 'admin', displayName: 'admin', role: 'admin' }))
  })
  await page.waitForTimeout(300)

  // 导航到审核详情页面
  await page.goto('/audits/detail?id=1', { waitUntil: 'networkidle', timeout: 20000 })
  await page.waitForTimeout(3000)

  // 检查页面内容
  const title = await page.title()
  console.log('Page title:', title)

  const hasPageContainer = await page.$('.page-container')
  console.log('Has page-container:', !!hasPageContainer)

  const hasDescriptions = await page.$$('.el-descriptions-item')
  console.log('Has el-descriptions-item:', hasDescriptions.length)

  const hasCard = await page.$$('.el-card')
  console.log('Has el-card:', hasCard.length)

  const hasTable = await page.$$('.el-table__row')
  console.log('Has table rows:', hasTable.length)

  // 检查控制台错误
  if (consoleMessages.length > 0) {
    console.log('Console errors:', consoleMessages)
  } else {
    console.log('No console errors')
  }

  if (pageErrors.length > 0) {
    console.log('Page errors:', pageErrors)
  } else {
    console.log('No page errors')
  }

  // 截图
  await page.screenshot({ path: 'screenshots/audit-detail-debug.png', fullPage: true })
  console.log('Screenshot saved')
})
