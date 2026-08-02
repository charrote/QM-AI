# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: audit-detail-debug.spec.ts >> 审核详情页控制台错误检查
- Location: tests/audit-detail-debug.spec.ts:3:1

# Error details

```
Error: page.goto: net::ERR_CONNECTION_REFUSED at http://localhost:5610/login
Call log:
  - navigating to "http://localhost:5610/login", waiting until "domcontentloaded"

```

# Test source

```ts
  1  | import { test, expect, type Page } from '@playwright/test'
  2  | 
  3  | test('审核详情页控制台错误检查', async ({ page }) => {
  4  |   const consoleMessages: string[] = []
  5  |   const pageErrors: string[] = []
  6  |   
  7  |   page.on('console', msg => {
  8  |     if (msg.type() === 'error') {
  9  |       consoleMessages.push(msg.text())
  10 |     }
  11 |   })
  12 |   page.on('pageerror', err => {
  13 |     pageErrors.push(err.message)
  14 |   })
  15 | 
  16 |   // 先导航到登录页设置 origin
> 17 |   await page.goto('/login', { waitUntil: 'domcontentloaded', timeout: 15000 })
     |              ^ Error: page.goto: net::ERR_CONNECTION_REFUSED at http://localhost:5610/login
  18 |   
  19 |   // 设置认证
  20 |   await page.evaluate(() => {
  21 |     localStorage.setItem('qm-ai-token', 'mock-test-token')
  22 |     localStorage.setItem('qm-ai-refresh-token', 'mock-refresh')
  23 |     localStorage.setItem('qm-ai-user', JSON.stringify({ id: 1, username: 'admin', displayName: 'admin', role: 'admin' }))
  24 |   })
  25 |   await page.waitForTimeout(300)
  26 | 
  27 |   // 导航到审核详情页面
  28 |   await page.goto('/audits/detail?id=1', { waitUntil: 'networkidle', timeout: 20000 })
  29 |   await page.waitForTimeout(3000)
  30 | 
  31 |   // 检查页面内容
  32 |   const title = await page.title()
  33 |   console.log('Page title:', title)
  34 | 
  35 |   const hasPageContainer = await page.$('.page-container')
  36 |   console.log('Has page-container:', !!hasPageContainer)
  37 | 
  38 |   const hasDescriptions = await page.$$('.el-descriptions-item')
  39 |   console.log('Has el-descriptions-item:', hasDescriptions.length)
  40 | 
  41 |   const hasCard = await page.$$('.el-card')
  42 |   console.log('Has el-card:', hasCard.length)
  43 | 
  44 |   const hasTable = await page.$$('.el-table__row')
  45 |   console.log('Has table rows:', hasTable.length)
  46 | 
  47 |   // 检查控制台错误
  48 |   if (consoleMessages.length > 0) {
  49 |     console.log('Console errors:', consoleMessages)
  50 |   } else {
  51 |     console.log('No console errors')
  52 |   }
  53 | 
  54 |   if (pageErrors.length > 0) {
  55 |     console.log('Page errors:', pageErrors)
  56 |   } else {
  57 |     console.log('No page errors')
  58 |   }
  59 | 
  60 |   // 截图
  61 |   await page.screenshot({ path: 'screenshots/audit-detail-debug.png', fullPage: true })
  62 |   console.log('Screenshot saved')
  63 | })
  64 | 
```