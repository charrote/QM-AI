import { test, expect } from '@playwright/test'

test('检查 el-descriptions 渲染', async ({ page }) => {
  await page.goto('/login', { waitUntil: 'domcontentloaded', timeout: 15000 })
  await page.evaluate(() => {
    localStorage.setItem('qm-ai-token', 'mock-test-token')
    localStorage.setItem('qm-ai-refresh-token', 'mock-refresh')
    localStorage.setItem('qm-ai-user', JSON.stringify({ id: 1, username: 'admin', displayName: 'admin', role: 'admin' }))
  })
  await page.waitForTimeout(300)
  await page.goto('/audits/detail?id=1', { waitUntil: 'networkidle', timeout: 20000 })
  await page.waitForTimeout(3000)

  // 获取 page-container 的内容
  const content = await page.$eval('.page-container', el => el.innerHTML)
  console.log('Page container HTML length:', content.length)
  
  // 检查是否包含 auditCode
  const hasAuditCode = content.includes('AUD-20260701-001') || content.includes('审核代码')
  console.log('Has auditCode:', hasAuditCode)
  
  // 检查 el-descriptions
  const descriptions = await page.$$('.el-descriptions')
  console.log('Has el-descriptions:', descriptions.length)
  
  // 检查 el-descriptions__label
  const labels = await page.$$('.el-descriptions__label')
  console.log('Has el-descriptions__label:', labels.length)
  
  // 检查 el-card 的文本内容
  const cards = await page.$$('.el-card')
  for (let i = 0; i < cards.length; i++) {
    const text = await cards[i].textContent()
    console.log(`Card ${i} text length:`, text?.length)
  }
  
  // 截图
  await page.screenshot({ path: 'screenshots/audit-descriptions-debug.png', fullPage: true })
})
