import { test, expect } from '@playwright/test'

test.describe('客户管理页面', () => {
  test('页面加载', async ({ page }) => {
    // 收集控制台消息
    const consoleMessages: string[] = []
    page.on('console', msg => consoleMessages.push(`[${msg.type()}] ${msg.text()}`))
    const errors: string[] = []
    page.on('pageerror', err => errors.push(err.message))

    await page.goto('http://localhost:5612/basic-data/customer')
    
    // Wait for page to load
    await page.waitForLoadState('networkidle', { timeout: 15000 }).catch(() => {})
    await page.waitForSelector('table', { timeout: 10000 }).catch(() => {})
    
    console.log('=== Console Messages ===')
    consoleMessages.forEach(m => console.log(m))
    console.log('=== Page Errors ===')
    errors.forEach(e => console.log(e))
    
    // Check page title
    const title = await page.title()
    console.log(`Page title: ${title}`)
    
    // Check if table exists
    const tableExists = await page.locator('table').count() > 0
    console.log(`Table exists: ${tableExists}`)
    
    // Check for error messages in the page
    const errorMsgs = await page.locator('.el-message, .el-message-box__message').all()
    console.log(`Error messages count: ${errorMsgs.length}`)
    for (const msg of errorMsgs) {
      console.log(`Error message: ${await msg.textContent()}`)
    }
    
    // Screenshot
    await page.screenshot({ path: 'test-customer-page.png', fullPage: true })
    console.log('Screenshot saved: test-customer-page.png')
  })
})

test.describe('8D 报告页面', () => {
  test('页面加载', async ({ page }) => {
    const consoleMessages: string[] = []
    page.on('console', msg => consoleMessages.push(`[${msg.type()}] ${msg.text()}`))
    const errors: string[] = []
    page.on('pageerror', err => errors.push(err.message))

    await page.goto('http://localhost:5612/complaints/d8')
    
    await page.waitForLoadState('networkidle', { timeout: 15000 }).catch(() => {})
    await page.waitForSelector('table', { timeout: 10000 }).catch(() => {})
    
    console.log('=== Console Messages ===')
    consoleMessages.forEach(m => console.log(m))
    console.log('=== Page Errors ===')
    errors.forEach(e => console.log(e))
    
    const title = await page.title()
    console.log(`Page title: ${title}`)
    
    const tableExists = await page.locator('table').count() > 0
    console.log(`Table exists: ${tableExists}`)
    
    const errorMsgs = await page.locator('.el-message, .el-message-box__message').all()
    console.log(`Error messages count: ${errorMsgs.length}`)
    for (const msg of errorMsgs) {
      console.log(`Error message: ${await msg.textContent()}`)
    }
    
    await page.screenshot({ path: 'test-d8-page.png', fullPage: true })
    console.log('Screenshot saved: test-d8-page.png')
  })
})
