import { test, expect } from '@playwright/test'

test('审核详情页缺少 ID 参数时显示提示', async ({ page }) => {
  await page.goto('/login', { waitUntil: 'domcontentloaded', timeout: 15000 })
  await page.evaluate(() => {
    localStorage.setItem('qm-ai-token', 'mock-test-token')
    localStorage.setItem('qm-ai-refresh-token', 'mock-refresh')
    localStorage.setItem('qm-ai-user', JSON.stringify({ id: 1, username: 'admin', displayName: 'admin', role: 'admin' }))
  })
  await page.waitForTimeout(300)
  
  // 导航到缺少 ID 的审核详情页
  await page.goto('/audits/detail', { waitUntil: 'networkidle', timeout: 20000 })
  await page.waitForTimeout(2000)

  // 检查是否显示提示信息
  const hasMissingIdMessage = await page.$('.empty-state')
  console.log('Has empty-state (missing ID message):', !!hasMissingIdMessage)
  
  const hasReturnButton = await page.$('text=返回审核列表')
  console.log('Has return button:', !!hasReturnButton)
  
  // 截图
  await page.screenshot({ path: 'screenshots/audit-missing-id.png', fullPage: true })
  console.log('Screenshot saved')
})
