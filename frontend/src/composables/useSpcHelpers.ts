import { CHART_TYPE_MAP, CPK_GRADE_TYPE, ANOVA_SOURCE_MAP, WESTERN_ELECTRIC_RULES, SOURCE_TYPE_OPTIONS } from '@/types/spc'

export function useSpcHelpers() {
  function formatDate(d?: string): string {
    if (!d) return '-'
    return new Date(d).toLocaleString('zh-CN')
  }

  function formatNumber(v?: number, digits = 4): string {
    if (v == null) return '-'
    return v.toFixed(digits)
  }

  function cpkGradeType(grade?: string): string {
    return grade ? CPK_GRADE_TYPE[grade] || 'info' : 'info'
  }

  function chartTypeLabel(type?: string): string {
    return type ? (CHART_TYPE_MAP[type] || type) : ''
  }

  function anovaSourceLabel(source?: string): string {
    return source ? (ANOVA_SOURCE_MAP[source] || source) : ''
  }

  function sourceTypeLabel(type: string): string {
    return SOURCE_TYPE_OPTIONS.find(o => o.value === type)?.label || type
  }

  function getRuleName(ruleNum: number): string {
    return WESTERN_ELECTRIC_RULES.find(r => r.ruleNumber === ruleNum)?.ruleName || `规则 ${ruleNum}`
  }

  function violationCount(violations: { ruleNumber: number }[] | undefined, ruleNum: number): number {
    return violations?.filter(v => v.ruleNumber === ruleNum).length || 0
  }

  return {
    formatDate,
    formatNumber,
    cpkGradeType,
    chartTypeLabel,
    anovaSourceLabel,
    sourceTypeLabel,
    getRuleName,
    violationCount,
  }
}
