import { ref, reactive, computed } from 'vue'
import { ElMessage } from 'element-plus'
import { samplingPlanApi } from '@/api/iqc'
import type { SamplingPlan, SamplingPlanRequest } from '@/types/iqc'

export interface UseSamplingPlanOptions {
  /** 默认表单值 */
  defaults?: Partial<SamplingPlanRequest>
}

/**
 * 封装 GB/T 2828.1 抽样方案计算逻辑
 *
 * 使用方式：
 *   const { form, result, loading, calculate, reset } = useSamplingPlan()
 *   // 在 template 中 v-model 绑定 form，展示 result
 */
export function useSamplingPlan(options: UseSamplingPlanOptions = {}) {
  const {
    defaults = { lotSize: 100, samplingLevel: 'II', aqlValue: 1.0 },
  } = options

  const form = reactive<SamplingPlanRequest>({
    lotSize: defaults.lotSize ?? 100,
    samplingLevel: defaults.samplingLevel ?? 'II',
    aqlValue: defaults.aqlValue ?? 1.0,
  })

  const result = ref<SamplingPlan | null>(null)
  const loading = ref(false)

  async function calculate() {
    loading.value = true
    result.value = null
    try {
      result.value = await samplingPlanApi.calculate({
        lotSize: form.lotSize,
        samplingLevel: form.samplingLevel,
        aqlValue: form.aqlValue,
      })
    } catch {
      ElMessage.error('抽样方案计算失败')
    } finally {
      loading.value = false
    }
  }

  function reset() {
    form.lotSize = defaults.lotSize ?? 100
    form.samplingLevel = defaults.samplingLevel ?? 'II'
    form.aqlValue = defaults.aqlValue ?? 1.0
    result.value = null
  }

  return {
    form,
    result,
    loading,
    calculate,
    reset,
  }
}
