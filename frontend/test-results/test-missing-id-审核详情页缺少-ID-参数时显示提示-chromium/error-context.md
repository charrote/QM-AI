# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: test-missing-id.spec.ts >> 审核详情页缺少 ID 参数时显示提示
- Location: tests/test-missing-id.spec.ts:3:1

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
  3  | test('审核详情页缺少 ID 参数时显示提示', async ({ page }) => {
> 4  |   await page.goto('/login', { waitUntil: 'domcontentloaded', timeout: 15000 })
     |              ^ Error: page.goto: net::ERR_CONNECTION_REFUSED at http://localhost:5610/login
  5  |   await page.evaluate(() => {
  6  |     localStorage.setItem('qm-ai-token', 'mock-test-token')
  7  |     localStorage.setItem('qm-ai-refresh-token', 'mock-refresh')
  8  |     localStorage.setItem('qm-ai-user', JSON.stringify({ id: 1, username: 'admin', displayName: 'admin', role: 'admin' }))
  9  |   })
  10 |   await page.waitForTimeout(300)
  11 |   
  12 |   // 导航到缺少 ID 的审核详情页
  13 |   await page.goto('/audits/detail', { waitUntil: 'networkidle', timeout: 20000 })
  14 |   await page.waitForTimeout(2000)
  15 | 
  16 |   // 检查是否显示提示信息
  17 |   const hasMissingIdMessage = await page.$('.empty-state')
  18 |   console.log('Has empty-state (missing ID message):', !!hasMissingIdMessage)
  19 |   
  20 |   const hasReturnButton = await page.$('text=返回审核列表')
  21 |   console.log('Has return button:', !!hasReturnButton)
  22 |   
  23 |   // 截图
  24 |   await page.screenshot({ path: 'screenshots/audit-missing-id.png', fullPage: true })
  25 |   console.log('Screenshot saved')
  26 | })
  27 | 
```