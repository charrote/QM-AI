# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: ai-quality-pages.spec.ts >> 登录页面检查 >> 登录页面可正常加载
- Location: tests/ai-quality-pages.spec.ts:168:3

# Error details

```
Error: page.goto: net::ERR_CONNECTION_REFUSED at http://localhost:5610/login
Call log:
  - navigating to "http://localhost:5610/login", waiting until "networkidle"

```

# Test source

```ts
  71  |       await page.waitForTimeout(500)
  72  | 
  73  |       // 2. 注入模拟认证信息到 localStorage
  74  |       await page.evaluate(() => {
  75  |         localStorage.setItem('qm-ai-token', 'mock-test-token-12345')
  76  |         localStorage.setItem('qm-ai-refresh-token', 'mock-refresh-token')
  77  |         localStorage.setItem('qm-ai-user', JSON.stringify({
  78  |           id: 1,
  79  |           username: 'admin',
  80  |           displayName: '管理员',
  81  |           role: '管理员',
  82  |         }))
  83  |       })
  84  | 
  85  |       // 3. 清除错误监听器，重新开始收集
  86  |       const { errors, cleanup } = await setupErrorListeners(page, name)
  87  | 
  88  |       // 4. 导航到目标页面
  89  |       console.log(`\n--- 访问: ${name} (${path}) ---`)
  90  |       await page.goto(path, { waitUntil: 'networkidle', timeout: 20000 })
  91  |       await page.waitForTimeout(2000)
  92  | 
  93  |       // 5. 截图
  94  |       const safeName = name.replace(/[/\\s]/g, '_')
  95  |       await page.screenshot({ path: join(SCREENSHOTS_DIR, `${safeName}.png`), fullPage: true })
  96  | 
  97  |       // 6. 检查页面标题
  98  |       const title = await page.title()
  99  |       console.log(`  页面标题: ${title}`)
  100 | 
  101 |       // 7. 检查是否成功渲染
  102 |       const hasContent = await page.$('.page-container') || await page.$('.ipqc-content') ||
  103 |         await page.$('.page-header-banner') || await page.$('.el-page-header') ||
  104 |         await page.$('.el-card') || await page.$('.data-card')
  105 |       if (hasContent) {
  106 |         console.log('  ✅ 页面主体已渲染')
  107 |       } else {
  108 |         console.log('  ⚠️ 未检测到预期的页面主体元素')
  109 |       }
  110 | 
  111 |       // 8. 检查错误
  112 |       if (errors.length > 0) {
  113 |         console.log(`  ❌ 发现 ${errors.length} 个错误:`)
  114 |         for (const err of errors) {
  115 |           console.log(`    ${err}`)
  116 |         }
  117 |       } else {
  118 |         console.log('  ✅ 无错误')
  119 |       }
  120 | 
  121 |       allResults.push({ name, errors, passed: errors.length === 0 })
  122 |       cleanup()
  123 |     })
  124 |   }
  125 | 
  126 |   test('结果汇总', async ({}) => {
  127 |     console.log('\n\n============================')
  128 |     console.log('   AI 质量分析页面检查汇总')
  129 |     console.log('============================')
  130 |     let passCount = 0
  131 |     let failCount = 0
  132 |     for (const r of allResults) {
  133 |       if (r.passed) {
  134 |         console.log(`  ✅ ${r.name}: 通过`)
  135 |         passCount++
  136 |       } else {
  137 |         console.log(`  ❌ ${r.name}: 失败 (${r.errors.length} 个错误)`)
  138 |         failCount++
  139 |         for (const err of r.errors) {
  140 |           console.log(`     ${err}`)
  141 |         }
  142 |       }
  143 |     }
  144 |     console.log(`\n  总计: ${allResults.length} 个页面, 通过 ${passCount}, 失败 ${failCount}`)
  145 |     console.log('============================\n')
  146 | 
  147 |     if (failCount > 0) {
  148 |       const reportPath = join(SCREENSHOTS_DIR, 'error-report.txt')
  149 |       let report = 'AI 质量分析页面错误报告\n'
  150 |       report += '='.repeat(50) + '\n\n'
  151 |       for (const r of allResults) {
  152 |         if (!r.passed) {
  153 |           report += `${r.name}:\n`
  154 |           for (const err of r.errors) {
  155 |             report += `  ${err}\n`
  156 |           }
  157 |           report += '\n'
  158 |         }
  159 |       }
  160 |       fs.writeFileSync(reportPath, report)
  161 |       console.log(`详细错误报告已保存至: ${reportPath}`)
  162 |     }
  163 |   })
  164 | })
  165 | 
  166 | // ─── 测试: 登录页面 ─────────────────────────────────────────────────
  167 | test.describe('登录页面检查', () => {
  168 |   test('登录页面可正常加载', async ({ page }) => {
  169 |     const { errors, cleanup } = await setupErrorListeners(page, '登录页面')
  170 | 
> 171 |     await page.goto('/login', { waitUntil: 'networkidle', timeout: 15000 })
      |                ^ Error: page.goto: net::ERR_CONNECTION_REFUSED at http://localhost:5610/login
  172 |     await page.waitForTimeout(1000)
  173 | 
  174 |     const title = await page.title()
  175 |     console.log(`  登录页面标题: ${title}`)
  176 | 
  177 |     const hasForm = await page.$('.form-container')
  178 |     console.log(hasForm ? '  ✅ 登录表单已渲染' : '  ⚠️ 登录表单未渲染')
  179 | 
  180 |     if (errors.length > 0) {
  181 |       console.log(`  ❌ 发现 ${errors.length} 个错误:`)
  182 |       for (const err of errors) console.log(`    ${err}`)
  183 |     } else {
  184 |       console.log('  ✅ 登录页面无错误')
  185 |     }
  186 |     cleanup()
  187 |   })
  188 | })
  189 | 
```