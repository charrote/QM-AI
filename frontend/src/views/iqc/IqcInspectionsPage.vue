<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { inspectionApi, receiptApi } from '@/api/iqc'
import { inspectionPlanApi } from '@/api/inspectionPlan'
import { Document, Search, Refresh, Edit, Delete, DataAnalysis, Loading, ScaleToOriginal } from '@element-plus/icons-vue'
import type {
  IqcInspection, IqcInspectionDetail, CreateIqcInspection, SubmitIqcInspection,
} from '@/types/iqc'
import {
  IQC_INSPECTION_RESULT_OPTIONS, SAMPLING_LEVEL_OPTIONS,
} from '@/types/iqc'
import SamplingPlanCalculator from '@/components/SamplingPlanCalculator.vue'

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
const submitDrawerVisible = ref(false)
const loadingPlanItems = ref(false)

// Sampling (shared component)
const samplingDrawerVisible = ref(false)

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
  submitDrawerVisible.value = true

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
            paramId: item.inspectionItemId,
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
    submitDrawerVisible.value = false
    await loadInspections()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '提交失败')
  }
}

onMounted(async () => {
  await loadInspections()
})
</script>

<template>
  <div class="iqc-container">
    <!-- Page Header -->
    <div class="page-header-banner page-header-banner--primary">
      <div class="page-header-banner-main">
        <div class="page-header-banner-icon">
          <el-icon :size="28"><Document /></el-icon>
        </div>
        <div class="page-header-banner-text">
          <h2 class="page-header-banner-title">检验单管理</h2>
          <span class="page-header-banner-subtitle">查看来料检验记录与提交检验结果</span>
        </div>
      </div>
    </div>

    <!-- Content Area -->
    <div class="iqc-content">
      <div class="data-card">
        <div class="data-card__header">
          <span class="data-card__title">检验记录</span>
          <div class="data-card__toolbar">
            <el-input
              v-model="searchKeyword"
              placeholder="搜索检验单号..."
              clearable
              size="small"
              :prefix-icon="Search"
              style="width: 220px"
              @keyup.enter="loadInspections"
            />
            <el-button size="small" @click="loadInspections">
              <el-icon><Refresh /></el-icon>刷新
            </el-button>
            <el-button size="small" @click="samplingDrawerVisible = true">
              <el-icon><ScaleToOriginal /></el-icon>抽样计算器
            </el-button>
          </div>
        </div>
        <el-table
          :data="inspections"
          border
          stripe
          size="small"
          class="data-card__table"
        >
          <el-table-column type="index" label="序号" width="55" fixed class-name="index-cell" />
          <el-table-column prop="inspectionNo" label="检验单号" min-width="180" show-overflow-tooltip />
          <el-table-column prop="receiptNo" label="来源单号" min-width="150" show-overflow-tooltip />
          <el-table-column prop="sampleSize" label="样本量" width="70" align="right" />
          <el-table-column label="Ac/Re" width="70" align="center">
            <template #default="{ row }">{{ row.ac }}/{{ row.re }}</template>
          </el-table-column>
          <el-table-column prop="defectQty" label="不合格数" width="80" align="right" />
          <el-table-column prop="samplingLevel" label="水平" width="60" />
          <el-table-column label="结果" width="80" align="center">
            <template #default="{ row }">
              <el-tag :type="calculateResultColor(row.result)" size="small" effect="plain" round>
                {{ statusLabel(row.result, IQC_INSPECTION_RESULT_OPTIONS) }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="inspector" label="检验员" width="90" />
          <el-table-column prop="inspectedAt" label="检验时间" min-width="140">
            <template #default="{ row }">{{ formatDate(row.inspectedAt) }}</template>
          </el-table-column>
          <el-table-column label="操作" width="160" fixed="right" align="center">
            <template #default="{ row }">
              <el-button size="small" type="primary" link @click.stop="viewInspectionDetail(row)">详情</el-button>
              <el-button
                v-if="row.result === 'pending'"
                size="small" type="success" link @click.stop="openSubmitInspection(row)"
              >提交结果</el-button>
            </template>
          </el-table-column>
        </el-table>
        <div class="data-card__footer">
          <div class="data-card__pagination">
            <el-pagination
              v-model:current-page="page"
              v-model:page-size="pageSize"
              :total="total"
              :page-sizes="[10, 20, 50, 100]"
              layout="total, sizes, prev, pager, next, jumper"
              @size-change="loadInspections"
              @current-change="loadInspections"
            />
          </div>
        </div>
      </div>
    </div>
    <!-- ================================================================== -->
    <!-- Drawers -->
    <!-- ================================================================== -->

    <!-- 抽样计算器抽屉 -->
    <SamplingPlanCalculator mode="drawer" v-model="samplingDrawerVisible" />

    <!-- Drawer: 检验单详情 -->
    <el-drawer
      v-model="inspectionDetailVisible"
      size="560px"
      direction="rtl"
      :close-on-click-modal="false"
    >
      <template #header>
        <div class="drawer-header">
          <div class="drawer-header-icon">
            <el-icon :size="18"><Document /></el-icon>
          </div>
          <div class="drawer-header-text">
            <span class="drawer-title">检验单详情</span>
            <span class="drawer-subtitle">{{ inspectionDetail?.inspectionNo || '' }}</span>
          </div>
          <el-tag v-if="inspectionDetail" :type="calculateResultColor(inspectionDetail.result)" effect="dark" round>
            {{ statusLabel(inspectionDetail.result, IQC_INSPECTION_RESULT_OPTIONS) }}
          </el-tag>
        </div>
      </template>

      <template v-if="inspectionDetail">
        <!-- Basic Info -->
        <div class="drawer-section">
          <div class="drawer-section-header">
            <el-icon class="drawer-section-icon"><Document /></el-icon>
            <span>基本信息</span>
          </div>
          <el-descriptions :column="2" border size="default">
            <el-descriptions-item label="来源">{{ inspectionDetail.receiptNo }}</el-descriptions-item>
            <el-descriptions-item label="供应商">
              <span class="desc-highlight">{{ inspectionDetail.supplierName }}</span>
            </el-descriptions-item>
            <el-descriptions-item label="物料">
              <span class="desc-highlight">{{ inspectionDetail.productName }}</span>
            </el-descriptions-item>
            <el-descriptions-item label="抽样方案">
              n={{ inspectionDetail.sampleSize }}, Ac={{ inspectionDetail.ac }}, Re={{ inspectionDetail.re }}
            </el-descriptions-item>
            <el-descriptions-item label="结果">
              <el-tag :type="calculateResultColor(inspectionDetail.result)" size="small" round>
                {{ statusLabel(inspectionDetail.result, IQC_INSPECTION_RESULT_OPTIONS) }}
              </el-tag>
            </el-descriptions-item>
            <el-descriptions-item label="检验员">{{ inspectionDetail.inspector }}</el-descriptions-item>
          </el-descriptions>
        </div>

        <!-- Inspection Items -->
        <div class="drawer-section">
          <div class="drawer-section-header">
            <el-icon class="drawer-section-icon"><DataAnalysis /></el-icon>
            <span>检验项目</span>
            <el-tag type="info" size="small" effect="plain">{{ inspectionDetail.items?.length || 0 }} 项</el-tag>
          </div>
          <el-table :data="inspectionDetail.items || []" stripe size="small">
            <el-table-column prop="itemName" label="项目" min-width="120" show-overflow-tooltip />
            <el-table-column prop="measuredValue" label="实测值" width="90" align="right" />
            <el-table-column label="规格" width="130">
              <template #default="{ row }">
                {{ row.lsl ?? '-' }} ~ {{ row.usl ?? '-' }}
              </template>
            </el-table-column>
            <el-table-column label="结果" width="70" align="center">
              <template #default="{ row }">
                <el-tag :type="row.result === 'pass' ? 'success' : 'danger'" size="small" round>
                  {{ row.result === 'pass' ? 'OK' : 'NG' }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column prop="remark" label="备注" show-overflow-tooltip />
          </el-table>
        </div>
      </template>
    </el-drawer>

    <!-- Drawer: 提交检验结果 -->
    <el-drawer
      v-model="submitDrawerVisible"
      size="720px"
      direction="rtl"
      :close-on-click-modal="false"
    >
      <template #header>
        <div class="drawer-header">
          <div class="drawer-header-icon">
            <el-icon :size="18"><Edit /></el-icon>
          </div>
          <div class="drawer-header-text">
            <span class="drawer-title">提交检验结果</span>
            <span class="drawer-subtitle">填写检验项目实测值与判定</span>
          </div>
        </div>
      </template>

      <div class="dialog-section">
        <div class="drawer-section-header">
          <el-icon class="drawer-section-icon"><Edit /></el-icon>
          <span>检验员信息</span>
        </div>
        <el-form label-width="80px">
          <el-form-item label="检验员">
            <el-input v-model="submitInspector" placeholder="请输入检验员姓名" style="width: 260px" />
          </el-form-item>
        </el-form>
      </div>

      <div class="dialog-section">
        <div class="drawer-section-header">
          <el-icon class="drawer-section-icon"><DataAnalysis /></el-icon>
          <span>检验项目</span>
        </div>
        <div v-if="loadingPlanItems" style="text-align: center; padding: 16px 0; color: var(--el-text-color-secondary);">
          <el-icon class="is-loading" :size="20"><Loading /></el-icon>
          <span style="margin-left: 8px;">正在根据检验计划加载项目...</span>
        </div>
        <div v-else class="submit-list">
          <div v-for="(item, index) in submitItems" :key="index" class="submit-row">
            <span class="submit-index">{{ index + 1 }}</span>
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
              controls-position="right"
            />
            <el-select v-model="item.result" size="small" style="width: 90px">
              <el-option label="合格" value="pass" />
              <el-option label="不合格" value="fail" />
              <el-option label="待定" value="pending" />
            </el-select>
            <el-input v-model="item.remark" placeholder="备注" size="small" style="width: 110px" />
            <el-button link size="small" type="danger" @click="removeSubmitItem(index)">
              <el-icon><Delete /></el-icon>
            </el-button>
          </div>
        </div>
        <el-button size="small" @click="addSubmitItem" style="margin-top: 8px" :disabled="loadingPlanItems">+ 添加项目</el-button>
      </div>

      <template #footer>
        <div style="display: flex; gap: 8px; justify-content: flex-end;">
          <el-button size="small" @click="submitDrawerVisible = false">取消</el-button>
          <el-button size="small" type="primary" @click="submitInspection" :loading="loadingPlanItems">提交判定</el-button>
        </div>
      </template>
    </el-drawer>
  </div>
</template>
