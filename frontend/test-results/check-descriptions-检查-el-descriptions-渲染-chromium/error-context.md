# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: check-descriptions.spec.ts >> 检查 el-descriptions 渲染
- Location: tests/check-descriptions.spec.ts:3:1

# Error details

```
Error: page.goto: net::ERR_CONNECTION_REFUSED at http://localhost:5610/login
Call log:
  - navigating to "http://localhost:5610/login", waiting until "domcontentloaded"

```

# Test source

```ts
  1  | import { test, expect } from '@playwright/test'
  2  | 
  3  | test('检查 el-descriptions 渲染', async ({ page }) => {
> 4  |   await page.goto('/login', { waitUntil: 'domcontentloaded', timeout: 15000 })
     |              ^ Error: page.goto: net::ERR_CONNECTION_REFUSED at http://localhost:5610/login
  5  |   await page.evaluate(() => {
  6  |     localStorage.setItem('qm-ai-token', 'mock-test-token')
  7  |     localStorage.setItem('qm-ai-refresh-token', 'mock-refresh')
  8  |     localStorage.setItem('qm-ai-user', JSON.stringify({ id: 1, username: 'admin', displayName: 'admin', role: 'admin' }))
  9  |   })
  10 |   await page.waitForTimeout(300)
  11 |   await page.goto('/audits/detail?id=1', { waitUntil: 'networkidle', timeout: 20000 })
  12 |   await page.waitForTimeout(3000)
  13 | 
  14 |   // 获取 page-container 的内容
  15 |   const content = await page.$eval('.page-container', el => el.innerHTML)
  16 |   console.log('Page container HTML length:', content.length)
  17 |   
  18 |   // 检查是否包含 auditCode
  19 |   const hasAuditCode = content.includes('AUD-20260701-001') || content.includes('审核代码')
  20 |   console.log('Has auditCode:', hasAuditCode)
  21 |   
  22 |   // 检查 el-descriptions
  23 |   const descriptions = await page.$$('.el-descriptions')
  24 |   console.log('Has el-descriptions:', descriptions.length)
  25 |   
  26 |   // 检查 el-descriptions__label
  27 |   const labels = await page.$$('.el-descriptions__label')
  28 |   console.log('Has el-descriptions__label:', labels.length)
  29 |   
  30 |   // 检查 el-card 的文本内容
  31 |   const cards = await page.$$('.el-card')
  32 |   for (let i = 0; i < cards.length; i++) {
  33 |     const text = await cards[i].textContent()
  34 |     console.log(`Card ${i} text length:`, text?.length)
  35 |   }
  36 |   
  37 |   // 截图
  38 |   await page.screenshot({ path: 'screenshots/audit-descriptions-debug.png', fullPage: true })
  39 | })
  40 | 
```