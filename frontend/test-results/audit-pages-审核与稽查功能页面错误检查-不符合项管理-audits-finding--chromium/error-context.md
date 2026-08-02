# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: audit-pages.spec.ts >> 审核与稽查功能页面错误检查 >> 不符合项管理 (/audits/finding)
- Location: tests/audit-pages.spec.ts:339:5

# Error details

```
Error: page.goto: net::ERR_CONNECTION_REFUSED at http://localhost:5610/login
Call log:
  - navigating to "http://localhost:5610/login", waiting until "domcontentloaded"

```

# Test source

```ts
  245 |   // PUT /api/v1/m13/audits/findings/:id
  246 |   context.route(/\/api\/v1\/m13\/audits\/findings\/\d+\/?$/, async (route) => {
  247 |     const request = route.request()
  248 |     if (request.method() === 'PUT') {
  249 |       const data = JSON.parse(request.postData() || '{}')
  250 |       await route.fulfill({
  251 |         status: 200,
  252 |         contentType: 'application/json',
  253 |         body: JSON.stringify({ ...data, id: 1, updatedAt: new Date().toISOString() }),
  254 |       })
  255 |     } else {
  256 |       await route.continue()
  257 |     }
  258 |   })
  259 | 
  260 |   // POST /api/v1/m13/audits/findings/:id/verify
  261 |   context.route(/\/api\/v1\/m13\/audits\/findings\/\d+\/verify/, async (route) => {
  262 |     const data = JSON.parse(route.request().postData() || '{}')
  263 |     await route.fulfill({
  264 |       status: 200,
  265 |       contentType: 'application/json',
  266 |       body: JSON.stringify({ ...data, status: 'verified', updatedAt: new Date().toISOString() }),
  267 |     })
  268 |   })
  269 | 
  270 |   // GET /api/v1/m13/audits/:id
  271 |   context.route(/\/api\/v1\/m13\/audits\/\d+\/?$/, async (route) => {
  272 |     const request = route.request()
  273 |     const url = request.url()
  274 |     const match = url.match(/\/audits\/(\d+)/)
  275 |     if (!match) return await route.continue()
  276 |     const id = parseInt(match[1])
  277 |     const audit = MOCK_AUDITS.find(a => a.id === id)
  278 |     if (!audit) {
  279 |       await route.fulfill({ status: 404, contentType: 'application/json', body: JSON.stringify({ message: '审核不存在' }) })
  280 |       return
  281 |     }
  282 |     if (request.method() === 'GET') {
  283 |       await route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify(audit) })
  284 |     } else if (request.method() === 'PUT') {
  285 |       const data = JSON.parse(request.postData() || '{}')
  286 |       await route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify({ ...audit, ...data, updatedAt: new Date().toISOString() }) })
  287 |     } else if (request.method() === 'DELETE') {
  288 |       await route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify({ success: true }) })
  289 |     } else {
  290 |       await route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify(audit) })
  291 |     }
  292 |   })
  293 | }
  294 | 
  295 | // ─── 辅助: 注入认证 ─────────────────────────────────────────────────
  296 | async function injectAuth(page: Page) {
  297 |   await page.evaluate(() => {
  298 |     localStorage.setItem('qm-ai-token', 'mock-test-token-12345')
  299 |     localStorage.setItem('qm-ai-refresh-token', 'mock-refresh-token')
  300 |     localStorage.setItem('qm-ai-user', JSON.stringify({
  301 |       id: 1,
  302 |       username: 'admin',
  303 |       displayName: '管理员',
  304 |       role: '管理员',
  305 |     }))
  306 |   })
  307 | }
  308 | 
  309 | // ─── Mock 基础设施 API（避免 401 干扰）────────────────────────────────────────────────
  310 | function mockInfrastructureApis(context: BrowserContext) {
  311 |   // organizations/tree
  312 |   context.route(/\/api\/v1\/organizations\/tree/, async (route) => {
  313 |     await route.fulfill({
  314 |       status: 200,
  315 |       contentType: 'application/json',
  316 |       body: JSON.stringify([
  317 |         { id: 1, name: '总公司', level: 'company', path: '/' },
  318 |         { id: 2, name: '生产部', level: 'workshop', path: '/production' },
  319 |         { id: 3, name: '品质部', level: 'workshop', path: '/quality' },
  320 |       ]),
  321 |     })
  322 |   })
  323 | 
  324 |   // auth/refresh
  325 |   context.route(/\/api\/v1\/auth\/refresh/, async (route) => {
  326 |     await route.fulfill({
  327 |       status: 200,
  328 |       contentType: 'application/json',
  329 |       body: JSON.stringify({ token: 'mock-test-token-12345', refreshToken: 'mock-refresh-token' }),
  330 |     })
  331 |   })
  332 | }
  333 | 
  334 | // ─── 测试: 每个审核与稽查页面 ─────────────────────────────────────────────────
  335 | test.describe('审核与稽查功能页面错误检查', () => {
  336 |   const allResults: Array<{ name: string; path: string; errors: string[]; consoleErrors: string[]; pageErrors: string[]; httpErrors: string[]; passed: boolean }> = []
  337 | 
  338 |   for (const { path, name } of AUDIT_PAGES) {
  339 |     test(`${name} (${path})`, async ({ page, context }) => {
  340 |       // 注册 API Mock
  341 |       mockAuditApis(context)
  342 |       mockInfrastructureApis(context)
  343 | 
  344 |       // 先导航到登录页以设置 origin
> 345 |       await page.goto('/login', { waitUntil: 'domcontentloaded', timeout: 15000 })
      |                  ^ Error: page.goto: net::ERR_CONNECTION_REFUSED at http://localhost:5610/login
  346 |       await injectAuth(page)
  347 |       await page.waitForTimeout(300)
  348 | 
  349 |       // 清除错误监听，重新开始
  350 |       const { errors, consoleErrors, pageErrors, httpErrors, cleanup } = await setupErrorListeners(page, name)
  351 | 
  352 |       // 导航到目标页面
  353 |       console.log(`\n--- 访问: ${name} (${path}) ---`)
  354 |       await page.goto(path, { waitUntil: 'networkidle', timeout: 20000 })
  355 |       await page.waitForTimeout(3000)
  356 | 
  357 |       // 截图
  358 |       const safeName = name.replace(/[/\\s]/g, '_')
  359 |       await page.screenshot({ path: join(SCREENSHOTS_DIR, `${safeName}.png`), fullPage: true })
  360 | 
  361 |       // 检查页面标题
  362 |       const title = await page.title()
  363 |       console.log(`  页面标题: ${title}`)
  364 | 
  365 |       // 检查页面是否成功渲染
  366 |       const hasContent = await page.$('.page-container') || await page.$('.el-card') || await page.$('.data-card')
  367 |       if (hasContent) {
  368 |         console.log('  ✅ 页面主体已渲染')
  369 |       } else {
  370 |         console.log('  ⚠️ 未检测到预期的页面主体元素')
  371 |       }
  372 | 
  373 |       // 检查表格数据
  374 |       const tableRows = await page.$$('.el-table__row')
  375 |       console.log(`  表格数据: ${tableRows.length} 条`)
  376 | 
  377 |       // 检查错误
  378 |       if (errors.length > 0) {
  379 |         console.log(`  ❌ 发现 ${errors.length} 个错误:`)
  380 |         for (const err of errors) console.log(`    ${err}`)
  381 |       } else {
  382 |         console.log('  ✅ 无错误')
  383 |       }
  384 | 
  385 |       allResults.push({ name, path, errors, consoleErrors, pageErrors, httpErrors, passed: errors.length === 0 })
  386 |       cleanup()
  387 |     })
  388 |   }
  389 | 
  390 |   // ─── 测试: 带参数的详细页面 ─────────────────────────────────────────────────
  391 |   test('审核详情 (带ID参数)', async ({ page, context }) => {
  392 |     mockAuditApis(context)
  393 |     mockInfrastructureApis(context)
  394 |     await page.goto('/login', { waitUntil: 'domcontentloaded', timeout: 15000 })
  395 |     await injectAuth(page)
  396 |     await page.waitForTimeout(300)
  397 | 
  398 |     const { errors, cleanup } = await setupErrorListeners(page, '审核详情(id=1)')
  399 | 
  400 |     console.log('\n--- 访问: 审核详情(id=1) ---')
  401 |     await page.goto('/audits/detail?id=1', { waitUntil: 'networkidle', timeout: 20000 })
  402 |     await page.waitForTimeout(3000)
  403 | 
  404 |     const title = await page.title()
  405 |     console.log(`  页面标题: ${title}`)
  406 | 
  407 |     const descs = await page.$$('.el-descriptions-item')
  408 |     console.log(`  审核详情字段数: ${descs?.length || 0}`)
  409 | 
  410 |     const findingsTable = await page.$$('.el-table__row')
  411 |     console.log(`  不符合项表格: ${findingsTable.length} 条`)
  412 | 
  413 |     if (errors.length > 0) {
  414 |       console.log(`  ❌ 发现 ${errors.length} 个错误:`)
  415 |       for (const err of errors) console.log(`    ${err}`)
  416 |     } else {
  417 |       console.log('  ✅ 无错误')
  418 |     }
  419 | 
  420 |     allResults.push({ name: '审核详情(id=1)', path: '/audits/detail?id=1', errors, consoleErrors: [], pageErrors: [], httpErrors: [], passed: errors.length === 0 })
  421 |     cleanup()
  422 |   })
  423 | 
  424 |   test('不符合项管理 (带auditId参数)', async ({ page, context }) => {
  425 |     mockAuditApis(context)
  426 |     mockInfrastructureApis(context)
  427 |     await page.goto('/login', { waitUntil: 'domcontentloaded', timeout: 15000 })
  428 |     await injectAuth(page)
  429 |     await page.waitForTimeout(300)
  430 | 
  431 |     const { errors, cleanup } = await setupErrorListeners(page, '不符合项(auditId=1)')
  432 | 
  433 |     console.log('\n--- 访问: 不符合项(auditId=1) ---')
  434 |     await page.goto('/audits/finding?auditId=1', { waitUntil: 'networkidle', timeout: 20000 })
  435 |     await page.waitForTimeout(3000)
  436 | 
  437 |     const tableRows = await page.$$('.el-table__row')
  438 |     console.log(`  表格数据: ${tableRows.length} 条`)
  439 | 
  440 |     if (errors.length > 0) {
  441 |       console.log(`  ❌ 发现 ${errors.length} 个错误:`)
  442 |       for (const err of errors) console.log(`    ${err}`)
  443 |     } else {
  444 |       console.log('  ✅ 无错误')
  445 |     }
```