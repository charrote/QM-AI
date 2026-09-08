import type { SpcControlChart, SpcAnalysisReport, SpcDataPoint } from '@/types/spc'

// ─── 图型元数据 ─────────────────────────────────────────────────
export interface ChartSeries {
  chartType: string
  isAttribute: boolean
  categories: string[]
  mainTitle: string
  mainSeriesName: string
  mainValues: number[]
  mainLimits: { cl: number; ucl: number; lcl: number }
  mainViolationIndices: number[]
  subTitle: string | null
  subSeriesName: string | null
  subValues: (number | null)[] | null
  subLimits: { cl: number; ucl: number; lcl: number } | null
  subViolationIndices: number[]
  // 属性图额外信息：每点 [样本量, 不良/缺陷数]
  attrPoints?: { n: number; d: number }[]
}

const MAIN_TITLE: Record<string, string> = {
  Xbar_R: 'X̄ 均值控制图',
  Xbar_S: 'X̄ 均值控制图',
  I_MR: 'I 单值控制图',
  P: 'P 不良率控制图',
  C: 'C 缺陷数控制图',
  U: 'U 缺陷密度控制图',
}

const MAIN_SERIES: Record<string, string> = {
  Xbar_R: 'X̄',
  Xbar_S: 'X̄',
  I_MR: 'I',
  P: 'P',
  C: 'C',
  U: 'U',
}

const SUB_TITLE: Record<string, string> = {
  Xbar_R: 'R 极差控制图',
  Xbar_S: 'S 标准差控制图',
  I_MR: 'MR 移动极差控制图',
}

const SUB_SERIES: Record<string, string> = {
  Xbar_R: 'R',
  Xbar_S: 'S',
  I_MR: 'MR',
}

function parseValues(iv?: string): number[] {
  if (!iv) return []
  try {
    const arr = JSON.parse(iv)
    return Array.isArray(arr) ? arr.map(Number) : []
  } catch {
    return []
  }
}

function isAttributeType(t: string): boolean {
  return t === 'P' || t === 'C' || t === 'U'
}

// Sample std dev (n-1 denominator) — used by Xbar-S sub chart
function sampleStdDev(v: number[]): number {
  if (v.length < 2) return 0
  const m = v.reduce((a, b) => a + b, 0) / v.length
  const sq = v.reduce((a, b) => a + (b - m) ** 2, 0)
  return Math.sqrt(sq / (v.length - 1))
}

// 主图 plotted 值
function mainPlotted(chartType: string, dp: SpcDataPoint): number {
  const v = parseValues(dp.individualValues)
  if (chartType === 'P' || chartType === 'U') {
    const n = v[0] || 0
    const d = v[1] || 0
    return n > 0 ? d / n : 0
  }
  if (chartType === 'C') {
    return v[1] || 0 // [_, defectCount] -> d
  }
  // 计量图 / I-MR: subgroupMean
  return dp.subgroupMean != null ? dp.subgroupMean : (v.length ? v[0] : 0)
}

// 辅图 plotted 值（属性图为 null）
function subPlotted(chartType: string, dps: SpcDataPoint[], i: number): number | null {
  if (chartType === 'Xbar_R') return dps[i].subgroupRange ?? 0
  if (chartType === 'Xbar_S') {
    // Prefer stored std dev; fall back to computing from raw values (robust to legacy seed)
    if (dps[i].subgroupStdDev != null) return dps[i].subgroupStdDev
    return sampleStdDev(parseValues(dps[i].individualValues))
  }
  if (chartType === 'I_MR') {
    if (i === 0) return null
    const prev = parseValues(dps[i - 1].individualValues)[0] ?? 0
    const cur = parseValues(dps[i].individualValues)[0] ?? 0
    return Math.abs(cur - prev)
  }
  return null
}

// 超控制限的 index（用于辅图 R/S/MR 的判异标记）
function overLimitIndices(values: (number | null)[], limits: { ucl: number; lcl: number }): number[] {
  const out: number[] = []
  values.forEach((v, i) => {
    if (v != null && (v > limits.ucl || v < limits.lcl)) out.push(i)
  })
  return out
}

export function useSpcChartSeries() {
  function buildSeries(chart: SpcControlChart, report: SpcAnalysisReport): ChartSeries {
    const chartType = chart.chartType
    const isAttribute = isAttributeType(chartType)
    const dps = (report.dataPoints || []).slice().sort((a, b) => a.subgroupIndex - b.subgroupIndex)

    const categories = dps.map((_, i) => `#${i + 1}`)
    const mainValues = dps.map(dp => mainPlotted(chartType, dp))

    const cl = report.controlLimits
    const mainLimits = {
      cl: cl?.clXbar ?? chart.cl ?? 0,
      ucl: cl?.uclXbar ?? chart.ucl ?? 0,
      lcl: cl?.lclXbar ?? chart.lcl ?? 0,
    }

    // 主图违规 index（后端 8 条规则，去重）
    const mainViolationIndices = Array.from(new Set((report.violations || []).map(v => v.index)))

    // 属性图：提取 [n, d] 信息用于 tooltip
    let attrPoints: { n: number; d: number }[] | undefined
    if (isAttribute) {
      attrPoints = dps.map(dp => {
        const v = parseValues(dp.individualValues)
        return { n: v[0] || 0, d: v[1] || 0 }
      })
    }

    // 辅图（仅计量图）
    let subTitle: string | null = null
    let subSeriesName: string | null = null
    let subValues: (number | null)[] | null = null
    let subLimits: { cl: number; ucl: number; lcl: number } | null = null
    let subViolationIndices: number[] = []
    if (!isAttribute) {
      subTitle = SUB_TITLE[chartType] || null
      subSeriesName = SUB_SERIES[chartType] || null
      subValues = dps.map((_, i) => subPlotted(chartType, dps, i))
      subLimits = {
        cl: cl?.clR ?? 0,
        ucl: cl?.uclR ?? 0,
        lcl: cl?.lclR ?? 0,
      }
      subViolationIndices = overLimitIndices(subValues, subLimits)
    }

    return {
      chartType,
      isAttribute,
      categories,
      mainTitle: MAIN_TITLE[chartType] || '控制图',
      mainSeriesName: MAIN_SERIES[chartType] || 'X̄',
      mainValues,
      mainLimits,
      mainViolationIndices,
      subTitle,
      subSeriesName,
      subValues,
      subLimits,
      subViolationIndices,
      attrPoints,
    }
  }

  return { buildSeries }
}
