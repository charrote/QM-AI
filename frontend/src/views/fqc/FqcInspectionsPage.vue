<script setup lang="ts">
import { ref, onMounted, reactive, computed } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import {
  Search, Plus, DocumentChecked, Edit, Check,
  Refresh, Box, Setting,
} from '@element-plus/icons-vue'
import { inspectionApi, batchApi } from '@/api/fqc'
import { inspectionPlanApi } from '@/api/inspectionPlan'
import type { FqcInspection, FqcInspectionDetail, CreateFqcInspection, SubmitFqcInspection, FqcInspectionItemSubmit } from '@/types/fqc'
import type { ProductBatch } from '@/types/fqc'
import type { PagedRequest, PagedResult } from '@/types/basicData'
import { FQC_CONCLUSION_OPTIONS, FQC_INSPECTION_TYPE_OPTIONS, FQC_CONCLUSION_MAP } from '@/types/fqc'

defineOptions({ name: 'FqcInspectionsPage' })

// ─── 状态 ─────────────────────────────────────────
const loading = ref(false)
const list = ref<FqcInspection[]>([])
const total = ref(0)
const query = reactive<PagedRequest>({ page: 1, pageSize: 20, keyword: '', status: '' })
const detailVisible = ref(false)
const detail = ref<FqcInspectionDetail | null>(null)
const createVisible = ref(false)
const submitVisible = ref(false)
const submitId = ref(0)
const batches = ref<ProductBatch[]>([])

// ─── 创建表单 ─────────────────────────────────────
const createForm = reactive<CreateFqcInspection>({
  batchId: 0,
  inspectionType: 'full',
  sampleSize: 0,
  ac: 0,
  re: 0,
})

// ─── 提交表单 ─────────────────────────────────────
const submitForm = reactive({
  totalChecked: 0,
  totalPass: 0,
  totalFail: 0,
  items: [] as FqcInspectionItemSubmit[],
})

// ─── 计算指标 ─────────────────────────────────────
const stats = computed(() => {
  const all = list.value
  return {
    total: all.length,
    pending: all.filter(i => i.conclusion === 'pending').length,
    passRate: all.length > 0
      ? Math.round(((all.filter(i => i.conclusion === 'qualified').length) / all.length) * 100)
      : 0,
  }
})

// ─── 获取列表 ─────────────────────────────────────
async function fetchList() {
  loading.value = true
  try {
    const res = await inspectionApi.list({ ...query })
    list.value = res.items
    total.value = res.total
  } catch { /* handled by interceptor */ }
  finally { loading.value = false }
}

// ─── 查看详情 ─────────────────────────────────────
async function openDetail(id: number) {
  try {
    detail.value = await inspectionApi.get(id)
    detailVisible.value = true
  } catch { /* */ }
}

// ─── 新建检验单 ─────────────────────────────────
async function openCreate() {
  createForm.batchId = 0
  createForm.inspectionType = 'full'
  createForm.sampleSize = 0
  createForm.ac = 0
  createForm.re = 0
  // 加载批次列表
  try {
    const res = await batchApi.list({ page: 1, pageSize: 100 })
    batches.value = res.items
  } catch { /* */ }
  createVisible.value = true
}

async function handleCreate() {
  if (!createForm.batchId) {
    ElMessage.warning('请选择批次')
    return
  }
  try {
    await inspectionApi.create(createForm)
    ElMessage.success('检验单创建成功')
    createVisible.value = false
    await fetchList()
  } catch { /* */ }
}

// ─── 提交检验结果 ─────────────────────────────────
async function openSubmit(id: number) {
  submitId.value = id
  try {
    const res = await inspectionApi.get(id)
    submitForm.totalChecked = res.totalChecked || res.sampleSize
    submitForm.totalPass = res.totalPass
    submitForm.totalFail = res.totalFail
    submitForm.items = (res.items || []).map(it => ({
      id: it.id,
      inspectionItemId: it.inspectionItemId,
      itemName: it.itemName,
      itemCode: it.itemCode,
      usl: it.usl,
      lsl: it.lsl,
      dataType: it.dataType,
      actualValue: it.actualValue,
      result: it.result,
      imageUrls: it.imageUrls,
    }))

    // Auto-load from plans if no items exist yet
    if (!submitForm.items || submitForm.items.length === 0) {
      try {
        const plans = await inspectionPlanApi.getByContext({
          inspectionType: 'FQC',
          productId: res.productId,
        })
        if (plans.length > 0) {
          const allItems = plans.flatMap(p => p.items)
          const seen = new Set<number>()
          const newItems: FqcInspectionItemSubmit[] = []
          for (const item of allItems) {
            if (!seen.has(item.inspectionItemId)) {
              seen.add(item.inspectionItemId)
              newItems.push({
                inspectionItemId: item.inspectionItemId,
                itemName: item.inspectionItemName,
                dataType: item.dataType,
                usl: item.usl ?? undefined,
                lsl: item.lsl ?? undefined,
                result: 'pending',
              })
            }
          }
          if (newItems.length > 0) {
            submitForm.items = newItems
          }
        }
      } catch (e) {
        console.error('加载检验计划失败', e)
      }
      // Fallback default item
      if (!submitForm.items || submitForm.items.length === 0) {
        submitForm.items.push({
          itemName: '外观检查',
          dataType: 'visual',
          result: 'pending',
        })
      }
    }
  } catch { return }
  submitVisible.value = true
}

async function handleSubmit() {
  try {
    // Validation
    if (submitForm.totalChecked === 0) {
      ElMessage.warning('已检数量不能为 0')
      return
    }
    if (submitForm.totalPass + submitForm.totalFail !== submitForm.totalChecked) {
      ElMessage.warning('合格数量 + 不合格数量 必须等于已检数量')
      return
    }
    if (!submitForm.items || submitForm.items.length === 0) {
      ElMessage.warning('请至少添加一项检验明细')
      return
    }

    await inspectionApi.submit(submitId.value, {
      totalChecked: submitForm.totalChecked,
      totalPass: submitForm.totalPass,
      totalFail: submitForm.totalFail,
      items: submitForm.items,
    })
    ElMessage.success('检验结果提交成功')
    submitVisible.value = false
    await fetchList()
  } catch { /* */ }
}

function addItem() {
  submitForm.items.push({
    itemName: '',
    dataType: 'numeric',
    result: 'pending',
  })
}

function removeItem(index: number) {
  submitForm.items.splice(index, 1)
}

function evaluateItem(item: FqcInspectionItemSubmit) {
  if (item.usl === undefined || item.lsl === undefined) return
  if (item.actualValue === undefined || item.actualValue === null || item.actualValue === '') {
    item.result = 'pending'
    return
  }
  const val = Number(item.actualValue)
  if (isNaN(val)) {
    item.result = 'pending'
    return
  }
  item.result = (val <= item.usl! && val >= item.lsl!) ? 'pass' : 'fail'
  recalcTotals()
}

function recalcTotals() {
  const checked = submitForm.items.length
  let pass = 0
  let fail = 0
  for (const item of submitForm.items) {
    if (item.result === 'pass') pass++
    else if (item.result === 'fail') fail++
  }
  submitForm.totalChecked = checked
  submitForm.totalPass = pass
  submitForm.totalFail = fail
}

// ─── 结论标签 ─────────────────────────────────────
function conclusionTag(type: string): string {
  const map: Record<string, string> = { pending: 'warning', qualified: 'success', unqualified: 'danger' }
  return map[type] || 'info'
}

function conclusionLabel(type: string): string {
  return FQC_CONCLUSION_MAP[type] || type
}

function inspectionTypeTag(type: string): string {
  return type === 'full' ? 'primary' : 'warning'
}

function inspectionTypeLabel(type: string): string {
  return type === 'full' ? '全检' : '抽检'
}

function conclusionResultLabel(result: string): string {
  return result === 'pass' ? '合格' : result === 'fail' ? '不合格' : '待检'
}

function conclusionResultType(result: string): string {
  return result === 'pass' ? 'success' : result === 'fail' ? 'danger' : 'info'
}

// ─── 检验方式选项 ─────────────────────────────────
const INSPECTION_TYPE_MAP: Record<string, string> = {
  full: '全检',
  sampling: '抽检',
}

onMounted(fetchList)
</script>

<template>
  <div class="page-container">
    <!-- Page Header -->
    <div class="page-header">
      <div class="page-header__icon-wrapper">
        <el-icon :size="28"><DocumentChecked /></el-icon>
      </div>
      <div class="page-header__info">
        <h1 class="page-header__title">FQC 成品检验</h1>
        <p class="page-header__subtitle">管理成品检验单，跟踪检验进度与判定结果</p>
      </div>
    </div>

    <!-- Summary Metrics -->
    <el-row :gutter="16" class="metrics-row">
      <el-col :span="8">
        <el-statistic title="检验单总数" :value="stats.total">
          <template #prefix>
            <el-icon color="var(--el-color-primary)"><DocumentChecked /></el-icon>
          </template>
        </el-statistic>
      </el-col>
      <el-col :span="8">
        <el-statistic title="待检验" :value="stats.pending">
          <template #prefix>
            <el-icon color="var(--el-color-warning)"><Edit /></el-icon>
          </template>
        </el-statistic>
      </el-col>
      <el-col :span="8">
        <el-statistic title="合格率" :value="stats.passRate" suffix="%">
          <template #prefix>
            <el-icon color="var(--el-color-success)"><Check /></el-icon>
          </template>
        </el-statistic>
      </el-col>
    </el-row>

    <!-- Source Banner -->
    <div class="source-banner">
      <el-icon :size="18" color="var(--el-color-info)"><DocumentChecked /></el-icon>
      <span class="source-banner__text">检验流程：</span>
      <span class="source-banner__flow">IPQC</span>
      <span class="source-banner__flow">→ FQC 成品检验</span>
      <span class="source-banner__flow">→ OQC 出货放行</span>
    </div>

    <!-- Action Bar -->
    <div class="action-bar">
      <div class="action-bar__left">
        <el-input
          v-model="query.keyword"
          placeholder="搜索检验单号/批次号"
          :prefix-icon="Search"
          clearable
          style="width: 260px"
          @clear="fetchList"
          @keyup.enter="fetchList"
        />
        <el-select
          v-model="query.status"
          placeholder="检验结论"
          clearable
          style="width: 140px"
          @change="fetchList"
        >
          <el-option v-for="opt in FQC_CONCLUSION_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
        </el-select>
        <el-button @click="fetchList">
          <el-icon><Refresh /></el-icon>
          刷新
        </el-button>
      </div>
      <div class="action-bar__right">
        <el-button type="primary" @click="openCreate">
          <el-icon><Plus /></el-icon>
          新建检验单
        </el-button>
      </div>
    </div>

    <!-- Data Card -->
    <div class="data-card">
      <el-table
        :data="list"
        v-loading="loading"
        :row-class-name="() => 'data-card__row'"
        style="width: 100%"
      >
        <el-table-column prop="inspectionNo" label="检验单号" min-width="160" show-overflow-tooltip />
        <el-table-column prop="batchCode" label="批次号" min-width="150" show-overflow-tooltip />
        <el-table-column label="检验方式" width="80" align="center">
          <template #default="{ row }">
            <el-tag :type="inspectionTypeTag(row.inspectionType)" size="small" effect="dark">
              {{ inspectionTypeLabel(row.inspectionType) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="sampleSize" label="样本量" width="80" align="center" show-overflow-tooltip />
        <el-table-column prop="totalChecked" label="已检" width="70" align="center" show-overflow-tooltip />
        <el-table-column prop="totalPass" label="合格" width="70" align="center" show-overflow-tooltip />
        <el-table-column prop="totalFail" label="不合格" width="80" align="center" show-overflow-tooltip />
        <el-table-column label="结论" width="90" align="center">
          <template #default="{ row }">
            <el-tag :type="conclusionTag(row.conclusion)" size="small" effect="dark">
              {{ conclusionLabel(row.conclusion) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="checkedAt" label="检验时间" min-width="150" show-overflow-tooltip />
        <el-table-column label="操作" width="160" fixed="right" align="center">
          <template #default="{ row }">
            <el-button link size="small" type="primary" @click.stop="openDetail(row.id)">
              <el-icon><Edit /></el-icon>
              详情
            </el-button>
            <el-button
              link
              size="small"
              type="primary"
              v-if="row.conclusion === 'pending'"
              @click.stop="openSubmit(row.id)"
            >
              <el-icon><Check /></el-icon>
              提交
            </el-button>
          </template>
        </el-table-column>
      </el-table>

      <!-- Pagination -->
      <div class="data-card__footer">
        <el-pagination
          v-model:current-page="query.page"
          v-model:page-size="query.pageSize"
          :total="total"
          :page-sizes="[10, 20, 50, 100]"
          :sizes-layout="'first, prev, pager, next'"
          :pager-count="7"
          layout="total, sizes, prev, pager, next, jumper"
          background
          @change="fetchList"
        />
      </div>
    </div>

    <!-- 详情抽屉 -->
    <el-drawer v-model="detailVisible" title="检验详情" size="640px" :with-header="true" destroy-on-close>
      <template v-if="detail">
        <!-- 头部信息 -->
        <div class="detail-header">
          <el-tag :type="conclusionTag(detail.conclusion)" size="large" effect="dark">
            {{ conclusionLabel(detail.conclusion) }}
          </el-tag>
          <span class="detail-inspection-no">{{ detail.inspectionNo }}</span>
        </div>

        <!-- 检验概览 -->
        <div class="detail-section">
          <div class="detail-section-title">
            <el-icon color="var(--el-color-primary)"><DocumentChecked /></el-icon>
            <span>检验概览</span>
          </div>
          <el-descriptions :column="2" border class="detail-descriptions">
            <el-descriptions-item label="批次号">{{ detail.batchCode }}</el-descriptions-item>
            <el-descriptions-item label="产品名称">{{ detail.productName }}</el-descriptions-item>
            <el-descriptions-item label="检验方式">
              <el-tag size="small" effect="plain">{{ inspectionTypeLabel(detail.inspectionType) }}</el-tag>
            </el-descriptions-item>
            <el-descriptions-item label="样本量/总数量">{{ detail.sampleSize }} / {{ detail.batchQuantity }}</el-descriptions-item>
            <el-descriptions-item label="合格/不合格">{{ detail.totalPass }} / {{ detail.totalFail }}</el-descriptions-item>
            <el-descriptions-item label="检验时间">{{ detail.checkedAt || '-' }}</el-descriptions-item>
          </el-descriptions>
        </div>

        <!-- 进度条 -->
        <div v-if="detail.totalChecked > 0 || detail.sampleSize > 0" class="detail-section">
          <div class="detail-section-title">
            <el-icon color="var(--el-color-success)"><Check /></el-icon>
            <span>检验进度</span>
          </div>
          <el-progress
            :percentage="detail.sampleSize > 0 ? Math.round((detail.totalChecked / detail.sampleSize) * 100) : 0"
            :stroke-width="16"
            :text-inside="true"
            :format="() => `${detail.totalChecked} / ${detail.sampleSize}`"
          />
        </div>

        <!-- 检验明细 -->
        <div class="detail-section">
          <div class="detail-section-title">
            <el-icon color="var(--el-color-info)"><Box /></el-icon>
            <span>检验明细</span>
          </div>
          <el-table :data="detail.items || []" border size="small" style="width: 100%">
            <el-table-column prop="itemName" label="项目名称" min-width="120" show-overflow-tooltip />
            <el-table-column prop="usl" label="规格上限 (USL)" width="110" align="center" show-overflow-tooltip />
            <el-table-column prop="lsl" label="规格下限 (LSL)" width="110" align="center" show-overflow-tooltip />
            <el-table-column prop="actualValue" label="实测值" width="100" align="center" show-overflow-tooltip />
            <el-table-column label="结果" width="80" align="center">
              <template #default="{ row }">
                <el-tag
                  :type="conclusionResultType(row.result)"
                  size="small"
                  effect="dark"
                >
                  {{ conclusionResultLabel(row.result) }}
                </el-tag>
              </template>
            </el-table-column>
          </el-table>
        </div>
      </template>
    </el-drawer>

    <!-- 新建检验单对话框 -->
    <el-dialog v-model="createVisible" title="新建成品检验单" width="560px" :close-on-click-modal="false" top="8vh">
      <div v-if="createVisible">
        <!-- 批次信息 -->
        <div class="dialog-section">
          <div class="dialog-section__title">
            <el-icon><Box /></el-icon>
            <span>批次信息</span>
          </div>
          <el-form :model="createForm" label-width="100px" label-position="left">
            <el-form-item label="批次" required>
              <el-select
                v-model="createForm.batchId"
                placeholder="请选择批次"
                style="width:100%"
                filterable
                clearable
              >
                <el-option
                  v-for="b in batches"
                  :key="b.id"
                  :label="`${b.batchCode} - ${b.productName}`"
                  :value="b.id"
                />
              </el-select>
            </el-form-item>
          </el-form>
        </div>

        <!-- 抽样参数 -->
        <div class="dialog-section">
          <div class="dialog-section__title">
            <el-icon><Setting /></el-icon>
            <span>抽样参数</span>
          </div>
          <el-form :model="createForm" label-width="100px" label-position="left">
            <el-form-item label="检验方式" required>
              <el-radio-group v-model="createForm.inspectionType">
                <el-radio
                  v-for="opt in FQC_INSPECTION_TYPE_OPTIONS"
                  :key="opt.value"
                  :value="opt.value"
                >
                  {{ opt.label }}
                </el-radio>
              </el-radio-group>
            </el-form-item>
            <template v-if="createForm.inspectionType === 'sampling'">
              <el-form-item label="样本量">
                <el-input-number v-model="createForm.sampleSize" :min="1" style="width:100%" />
              </el-form-item>
              <el-form-item label="合格判定 Ac">
                <el-input-number v-model="createForm.ac" :min="0" style="width:100%" />
              </el-form-item>
              <el-form-item label="不合格判定 Re">
                <el-input-number v-model="createForm.re" :min="1" style="width:100%" />
              </el-form-item>
            </template>
          </el-form>
        </div>
      </div>

      <template #footer>
        <el-button @click="createVisible = false">取消</el-button>
        <el-button type="primary" @click="handleCreate">
          <el-icon><Check /></el-icon>
          创建检验单
        </el-button>
      </template>
    </el-dialog>

    <!-- 提交检验结果对话框 -->
    <el-dialog v-model="submitVisible" title="提交检验结果" width="900px" :close-on-click-modal="false" top="5vh">
      <div v-if="submitVisible">
        <!-- 汇总统计 -->
        <div class="dialog-section">
          <div class="dialog-section__title">
            <el-icon color="var(--el-color-primary)"><DocumentChecked /></el-icon>
            <span>检验汇总</span>
          </div>
          <el-row :gutter="12" class="submit-stats">
            <el-col :span="8">
              <div class="stat-item">
                <div class="stat-item__value" :class="submitForm.totalChecked === 0 ? 'stat-item__value--warning' : ''">
                  {{ submitForm.totalChecked }}
                </div>
                <div class="stat-item__label">已检数量</div>
              </div>
            </el-col>
            <el-col :span="8">
              <div class="stat-item">
                <div class="stat-item__value stat-item__value--success">
                  {{ submitForm.totalPass }}
                </div>
                <div class="stat-item__label">合格数量</div>
              </div>
            </el-col>
            <el-col :span="8">
              <div class="stat-item">
                <div class="stat-item__value stat-item__value--danger">
                  {{ submitForm.totalFail }}
                </div>
                <div class="stat-item__label">不合格数量</div>
              </div>
            </el-col>
          </el-row>
          <el-progress
            v-if="submitForm.totalChecked > 0"
            :percentage="submitForm.totalChecked > 0 ? Math.round(((submitForm.totalPass + submitForm.totalFail) / submitForm.totalChecked) * 100) : 0"
            :stroke-width="8"
            status="success"
          />
        </div>

        <!-- 检验明细 -->
        <div class="dialog-section">
          <div class="dialog-section__title">
            <el-icon><Box /></el-icon>
            <span>检验明细</span>
          </div>
          <el-table :data="submitForm.items" border size="small" style="width: 100%; margin-bottom: 12px">
            <el-table-column label="项目名称" min-width="140">
              <template #default="{ row }">
                <el-input v-model="row.itemName" placeholder="项目名称" size="small" />
              </template>
            </el-table-column>
            <el-table-column label="USL" width="110">
              <template #default="{ row }">
                <el-input-number
                  v-model="row.usl"
                  :precision="2"
                  :step="0.1"
                  :controls="false"
                  size="small"
                  style="width: 100%"
                  @change="evaluateItem(row)"
                />
              </template>
            </el-table-column>
            <el-table-column label="LSL" width="110">
              <template #default="{ row }">
                <el-input-number
                  v-model="row.lsl"
                  :precision="2"
                  :step="0.1"
                  :controls="false"
                  size="small"
                  style="width: 100%"
                  @change="evaluateItem(row)"
                />
              </template>
            </el-table-column>
            <el-table-column label="实测值" width="120">
              <template #default="{ row }">
                <el-input-number
                  v-model="row.actualValue"
                  :precision="2"
                  :step="0.1"
                  :controls="false"
                  size="small"
                  style="width: 100%"
                  @change="evaluateItem(row)"
                />
              </template>
            </el-table-column>
            <el-table-column label="结果" width="90" align="center">
              <template #default="{ row }">
                <el-select v-model="row.result" size="small" style="width: 100%">
                  <el-option label="合格" value="pass" />
                  <el-option label="不合格" value="fail" />
                  <el-option label="待检" value="pending" />
                </el-select>
              </template>
            </el-table-column>
            <el-table-column label="操作" width="60" align="center" fixed="right">
              <template #default="{ $index }">
                <el-button
                  link
                  size="small"
                  type="danger"
                  @click="removeItem($index)"
                >
                  删除
                </el-button>
              </template>
            </el-table-column>
          </el-table>
          <el-button size="small" @click="addItem" type="primary" plain>
            <el-icon><Plus /></el-icon>
            添加项目
          </el-button>
        </div>
      </div>

      <template #footer>
        <el-button @click="submitVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSubmit">
          <el-icon><Check /></el-icon>
          提交结果
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.page-container {
  display: flex;
  flex-direction: column;
  height: 100%;
  gap: 16px;
}

/* ─── Page Header ──────────────────── */
.page-header {
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 0 4px;
}

.page-header__icon-wrapper {
  width: 44px;
  height: 44px;
  border-radius: 10px;
  background: var(--el-color-primary-light-9, #ecf5ff);
  color: var(--el-color-primary, #409eff);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.page-header__info {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.page-header__title {
  margin: 0;
  font-size: 20px;
  font-weight: 600;
  color: var(--text-primary, #303133);
  line-height: 1.3;
}

.page-header__subtitle {
  margin: 0;
  font-size: 13px;
  color: var(--text-secondary, #909399);
  line-height: 1.4;
}

/* ─── Source Banner ────────────────── */
.source-banner {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
  padding: 10px 16px;
  background: var(--el-color-info-light-9, #ecf5ff);
  border-radius: 8px;
  font-size: 13px;
  color: var(--el-text-secondary, #909399);
}

.source-banner__text {
  font-weight: 500;
  color: var(--el-text-regular, #606266);
}

.source-banner__flow {
  font-weight: 500;
  color: var(--el-color-primary, #409eff);
}

/* ─── Action Bar ───────────────────── */
.action-bar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  flex-wrap: wrap;
}

.action-bar__left {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

.action-bar__right {
  display: flex;
  gap: 8px;
}

/* ─── Data Card ────────────────────── */
.data-card {
  flex: 1;
  display: flex;
  flex-direction: column;
  background: var(--bg-card, #fff);
  border-radius: 8px;
  box-shadow: var(--shadow-sm, 0 1px 2px rgba(0,0,0,0.06));
  overflow: hidden;
}

.data-card__row {
  transition: background-color 0.2s;
}

.data-card__row:hover {
  background-color: var(--el-fill-color-light, #f5f7fa) !important;
}

.data-card__footer {
  display: flex;
  justify-content: flex-end;
  padding: 12px 16px;
  border-top: 1px solid var(--el-border-color-lighter, #ebeef5);
}

/* ─── Detail Drawer ────────────────── */
.detail-header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 16px;
}

.detail-inspection-no {
  font-size: 18px;
  font-weight: 600;
  color: var(--text-primary, #303133);
}

.detail-descriptions {
  margin-bottom: 16px;
}

.detail-section {
  margin-bottom: 20px;
}

.detail-section-title {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 14px;
  font-weight: 600;
  color: var(--text-primary, #303133);
  margin-bottom: 12px;
  padding-bottom: 8px;
  border-bottom: 1px solid var(--el-border-color-lighter, #ebeef5);
}

/* ─── Dialog Sections ──────────────── */
.dialog-section {
  margin-bottom: 20px;
}

.dialog-section__title {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 13px;
  font-weight: 600;
  color: var(--text-primary, #303133);
  margin-bottom: 12px;
  padding-bottom: 8px;
  border-bottom: 1px solid var(--el-border-color-lighter, #ebeef5);
}

.dialog-section__title .el-icon {
  color: var(--el-color-primary, #409eff);
}

/* ─── Metrics Row ──────────────────── */
.metrics-row {
  margin-bottom: 0;
}

.metrics-row .el-statistic {
  margin-right: 16px;
}

/* ─── Submit Stats ─────────────────── */
.submit-stats {
  margin-bottom: 12px;
}

.stat-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
  padding: 12px;
  background: var(--el-fill-color-lighter, #f2f3f5);
  border-radius: 6px;
}

.stat-item__value {
  font-size: 24px;
  font-weight: 700;
  color: var(--el-color-primary, #409eff);
  line-height: 1.2;
}

.stat-item__value--success {
  color: var(--el-color-success, #67c23a);
}

.stat-item__value--danger {
  color: var(--el-color-danger, #f56c6c);
}

.stat-item__value--warning {
  color: var(--el-color-warning, #e6a23c);
}

.stat-item__label {
  font-size: 12px;
  color: var(--text-secondary, #909399);
}
</style>
