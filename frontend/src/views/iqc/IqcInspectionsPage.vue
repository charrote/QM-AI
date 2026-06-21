<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { inspectionApi, receiptApi, samplingPlanApi } from '@/api/iqc'
import { inspectionPlanApi } from '@/api/inspectionPlan'
import { ScaleToOriginal, Loading } from '@element-plus/icons-vue'
import type {
  IqcInspection, IqcInspectionDetail, CreateIqcInspection, SubmitIqcInspection,
  SamplingPlan, SamplingPlanRequest,
} from '@/types/iqc'
import {
  IQC_INSPECTION_RESULT_OPTIONS, SAMPLING_LEVEL_OPTIONS,
} from '@/types/iqc'

defineOptions({ name: 'IqcInspectionsPage' })

const searchKeyword = ref('')
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)

const inspections = ref<IqcInspection[]>([])
const inspectionDetail = ref<IqcInspectionDetail | null>(null)
const inspectionDetailVisible = ref(false)

// Submit
const submitItems = ref<Array<{
  inspectionItemId?: number; paramId?: number; itemName?: string; measuredValue?: number;
  usl?: number; lsl?: number; result: string; defectCodeId?: number; remark?: string
}>>([])
const submitInspector = ref('')
const submitInspectionId = ref<number>(0)
const submitDialogVisible = ref(false)
const loadingPlanItems = ref(false)

// Sampling
const samplingPlanResult = ref<SamplingPlan | null>(null)
const samplingPlanForm = reactive<SamplingPlanRequest>({
  lotSize: 100, samplingLevel: 'II', aqlValue: 1.0
})

function formatDate(d?: string) {
  if (!d) return '-'
  return new Date(d).toLocaleString('zh-CN')
}

function calculateResultColor(result: string) {
  switch (result) {
    case 'pass': return 'success'
    case 'fail': return 'danger'
    case 'qualified': return 'success'
    case 'unqualified': return 'danger'
    case 'anomaly': return 'warning'
    default: return 'info'
  }
}

const statusLabel = (status: string, options: any[]) => {
  const opt = options.find((o: any) => o.value === status)
  return opt?.label || status
}

async function loadInspections() {
  try {
    const res = await inspectionApi.list({
      page: page.value, pageSize: pageSize.value, keyword: searchKeyword.value
    })
    inspections.value = res.items
    total.value = res.total
  } catch (e) {
    console.error('Failed to load inspections', e)
  }
}

async function viewInspectionDetail(row: IqcInspection) {
  try {
    inspectionDetail.value = await inspectionApi.get(row.id)
    inspectionDetailVisible.value = true
  } catch (e) {
    ElMessage.error('加载检验单详情失败')
  }
}

async function openSubmitInspection(row: IqcInspection) {
  if (row.result !== 'pending') {
    ElMessage.warning('该检验单已提交')
    return
  }
  submitInspectionId.value = row.id
  submitInspector.value = ''
  submitItems.value = []
  loadingPlanItems.value = true
  submitDialogVisible.value = true

  try {
    // 1. 获取来料登记的 productId + supplierId
    const receipt = await receiptApi.get(row.receiptId)

    // 2. 根据业务上下文匹配检验计划
    const plans = await inspectionPlanApi.getByContext({
      inspectionType: 'IQC',
      productId: receipt.productId,
      supplierId: receipt.supplierId,
    })

    // 3. 从匹配的计划中提取检验项目
    if (plans.length > 0) {
      const allItems = plans.flatMap(p => p.items)
      // 去重（同一项目出现在多个计划时只保留一次）
      const seen = new Set<number>()
      for (const item of allItems) {
        if (!seen.has(item.inspectionItemId)) {
          seen.add(item.inspectionItemId)
          submitItems.value.push({
            inspectionItemId: item.inspectionItemId,
            itemName: `${item.inspectionItemCode} - ${item.inspectionItemName}`,
            usl: item.usl ?? undefined,
            lsl: item.lsl ?? undefined,
            measuredValue: undefined,
            result: 'pending',
          })
        }
      }
    }

    // 4. 没有匹配计划时，给一个默认项
    if (submitItems.value.length === 0) {
      submitItems.value.push({
        itemName: '外观检查', result: 'pending',
        measuredValue: undefined, usl: undefined, lsl: undefined,
      })
    }
  } catch (e) {
    console.error('加载检验计划失败', e)
    ElMessage.warning('未能加载检验计划，请手动添加检验项目')
    submitItems.value = [{
      itemName: '外观检查', result: 'pending',
      measuredValue: undefined, usl: undefined, lsl: undefined,
    }]
  } finally {
    loadingPlanItems.value = false
  }
}

function addSubmitItem() {
  submitItems.value.push({
    itemName: '', result: 'pending', measuredValue: undefined,
    usl: undefined, lsl: undefined
  })
}

function removeSubmitItem(index: number) {
  submitItems.value.splice(index, 1)
}

async function submitInspection() {
  if (submitItems.value.length === 0) {
    ElMessage.warning('请至少填写一个检验项目')
    return
  }
  try {
    await inspectionApi.submit(submitInspectionId.value, {
      items: submitItems.value.map(it => ({
        ...it,
        result: it.result === 'pass' ? 'pass' : it.result === 'fail' ? 'fail' : 'pending',
      })),
      inspector: submitInspector.value || undefined,
    })
    ElMessage.success('检验结果已提交')
    submitDialogVisible.value = false
    await loadInspections()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '提交失败')
  }
}

async function calculateSamplingPlan() {
  try {
    samplingPlanResult.value = await samplingPlanApi.calculate(samplingPlanForm)
  } catch (e) {
    ElMessage.error('抽样方案计算失败')
  }
}

onMounted(async () => {
  await loadInspections()
  calculateSamplingPlan()
})
</script>

<template>
  <div class="page-container">
    <div class="toolbar-row">
      <el-input
        v-model="searchKeyword"
        placeholder="搜索检验单号..."
        clearable
        style="width: 300px"
        @keyup.enter="loadInspections"
      />
      <el-button @click="loadInspections">刷新</el-button>
    </div>

    <el-table :data="inspections" stripe style="width: 100%" size="small">
      <el-table-column prop="inspectionNo" label="检验单号" width="180" />
      <el-table-column prop="receiptNo" label="来源单号" width="150" />
      <el-table-column prop="sampleSize" label="样本量" width="70" />
      <el-table-column label="Ac/Re" width="70">
        <template #default="{ row }">{{ row.ac }}/{{ row.re }}</template>
      </el-table-column>
      <el-table-column prop="defectQty" label="不合格数" width="80" />
      <el-table-column prop="samplingLevel" label="水平" width="60" />
      <el-table-column label="结果" width="80">
        <template #default="{ row }">
          <el-tag :type="calculateResultColor(row.result)" size="small" effect="plain">
            {{ statusLabel(row.result, IQC_INSPECTION_RESULT_OPTIONS) }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="inspector" label="检验员" width="90" />
      <el-table-column prop="inspectedAt" label="检验时间" width="160">
        <template #default="{ row }">{{ formatDate(row.inspectedAt) }}</template>
      </el-table-column>
      <el-table-column label="操作" width="180" fixed="right">
        <template #default="{ row }">
          <el-button link size="small" type="primary" @click="viewInspectionDetail(row)">详情</el-button>
          <el-button
            v-if="row.result === 'pending'"
            link size="small" type="success"
            @click="openSubmitInspection(row)"
          >提交结果</el-button>
        </template>
      </el-table-column>
    </el-table>

    <div class="pagination-row">
      <el-pagination
        v-model:current-page="page"
        v-model:page-size="pageSize"
        :total="total"
        layout="total, prev, pager, next"
        size="small"
        @current-change="loadInspections"
      />
    </div>

    <div class="sampling-panel">
      <el-collapse>
        <el-collapse-item name="sampling">
          <template #title>
            <el-icon style="vertical-align: middle"><ScaleToOriginal /></el-icon>
            <span style="vertical-align: middle">GB/T 2828.1 抽样方案计算器</span>
          </template>
          <el-row :gutter="16" style="margin-bottom: 8px">
            <el-col :span="6">
              <el-form-item label="批量" size="small">
                <el-input-number v-model="samplingPlanForm.lotSize" :min="1" :max="500000" style="width: 100%" />
              </el-form-item>
            </el-col>
            <el-col :span="6">
              <el-form-item label="检验水平" size="small">
                <el-select v-model="samplingPlanForm.samplingLevel" style="width: 100%">
                  <el-option v-for="opt in SAMPLING_LEVEL_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
                </el-select>
              </el-form-item>
            </el-col>
            <el-col :span="6">
              <el-form-item label="AQL 值" size="small">
                <el-input-number v-model="samplingPlanForm.aqlValue" :min="0.01" :step="0.1" :precision="2" style="width: 100%" />
              </el-form-item>
            </el-col>
            <el-col :span="6" style="display: flex; align-items: flex-start; padding-top: 2px">
              <el-button type="primary" size="small" @click="calculateSamplingPlan">计算</el-button>
            </el-col>
          </el-row>
          <div v-if="samplingPlanResult" class="sampling-result">
            <el-tag>字母代码: {{ samplingPlanResult.sampleCode }}</el-tag>
            <el-tag type="success">样本量: {{ samplingPlanResult.sampleSize }}</el-tag>
            <el-tag type="warning">Ac: {{ samplingPlanResult.ac }}</el-tag>
            <el-tag type="danger">Re: {{ samplingPlanResult.re }}</el-tag>
          </div>
        </el-collapse-item>
      </el-collapse>
    </div>

    <!-- Drawer: 检验单详情 -->
    <el-drawer
      v-model="inspectionDetailVisible"
      :title="`检验单: ${inspectionDetail?.inspectionNo || ''}`"
      size="500px"
    >
      <template v-if="inspectionDetail">
        <el-descriptions :column="2" size="small" border style="margin-bottom: 16px">
          <el-descriptions-item label="来源">{{ inspectionDetail.receiptNo }}</el-descriptions-item>
          <el-descriptions-item label="供应商">{{ inspectionDetail.supplierName }}</el-descriptions-item>
          <el-descriptions-item label="物料">{{ inspectionDetail.productName }}</el-descriptions-item>
          <el-descriptions-item label="抽样方案">
            n={{ inspectionDetail.sampleSize }}, Ac={{ inspectionDetail.ac }}, Re={{ inspectionDetail.re }}
          </el-descriptions-item>
          <el-descriptions-item label="结果">
            <el-tag :type="calculateResultColor(inspectionDetail.result)" size="small">
              {{ statusLabel(inspectionDetail.result, IQC_INSPECTION_RESULT_OPTIONS) }}
            </el-tag>
          </el-descriptions-item>
          <el-descriptions-item label="检验员">{{ inspectionDetail.inspector }}</el-descriptions-item>
        </el-descriptions>

        <h4 style="margin-bottom: 8px">检验项目</h4>
        <el-table :data="inspectionDetail.items || []" size="small" stripe>
          <el-table-column prop="itemName" label="项目" min-width="120" />
          <el-table-column prop="measuredValue" label="实测值" width="90" />
          <el-table-column label="规格" width="130">
            <template #default="{ row }">
              {{ row.lsl ?? '-' }} ~ {{ row.usl ?? '-' }}
            </template>
          </el-table-column>
          <el-table-column label="结果" width="70">
            <template #default="{ row }">
              <el-tag :type="row.result === 'pass' ? 'success' : 'danger'" size="small">
                {{ row.result === 'pass' ? 'OK' : 'NG' }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="remark" label="备注" show-overflow-tooltip />
        </el-table>
      </template>
    </el-drawer>

    <!-- Dialog: 提交检验结果 -->
    <el-dialog
      v-model="submitDialogVisible"
      title="提交检验结果"
      width="720px"
      :close-on-click-modal="false"
    >
      <el-form label-width="100px" size="small">
        <el-form-item label="检验员">
          <el-input v-model="submitInspector" placeholder="检验员姓名" style="width: 200px" />
        </el-form-item>
        <el-form-item label="检验项目">
          <div v-if="loadingPlanItems" class="text-center py-4">
            <el-icon class="is-loading" :size="20"><Loading /></el-icon>
            <span class="ml-2 text-gray-400">正在根据检验计划加载项目...</span>
          </div>
          <div v-else class="submit-items">
            <div v-for="(item, index) in submitItems" :key="index" class="submit-item-row">
              <el-input v-model="item.itemName" placeholder="项目名称" size="small" style="width: 160px" />
              <el-tooltip v-if="item.usl != null || item.lsl != null" :content="`规格: [${item.lsl ?? '-'}, ${item.usl ?? '-'}]`">
                <el-tag size="small" type="info" effect="plain" style="min-width: 80px; text-align: center">
                  {{ item.lsl ?? '-' }} ~ {{ item.usl ?? '-' }}
                </el-tag>
              </el-tooltip>
              <el-input-number
                v-model="item.measuredValue"
                :precision="4"
                :step="0.1"
                size="small"
                style="width: 130px"
                placeholder="实测值"
              />
              <el-select v-model="item.result" size="small" style="width: 90px">
                <el-option label="合格" value="pass" />
                <el-option label="不合格" value="fail" />
                <el-option label="待定" value="pending" />
              </el-select>
              <el-input v-model="item.remark" placeholder="备注" size="small" style="width: 110px" />
              <el-button link size="small" type="danger" @click="removeSubmitItem(index)">删除</el-button>
            </div>
          </div>
          <el-button size="small" @click="addSubmitItem" style="margin-top: 8px" :disabled="loadingPlanItems">+ 添加项目</el-button>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="submitDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="submitInspection" :loading="loadingPlanItems">提交判定</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.page-container { display: flex; flex-direction: column; height: 100%; }
.toolbar-row { display: flex; align-items: center; gap: 8px; margin-bottom: 12px; flex-wrap: wrap; }
.pagination-row { display: flex; justify-content: flex-end; padding: 12px 0; }
.sampling-panel { margin-top: 8px; }
.sampling-result { display: flex; gap: 8px; align-items: center; flex-wrap: wrap; }
.submit-items { display: flex; flex-direction: column; gap: 8px; }
.submit-item-row { display: flex; align-items: center; gap: 6px; }
.text-center { text-align: center; }
.py-4 { padding-top: 16px; padding-bottom: 16px; }
.ml-2 { margin-left: 8px; }
.text-gray-400 { color: #909399; }
</style>
