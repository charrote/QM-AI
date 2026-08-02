<script setup lang="ts">
import { ref, onMounted, reactive, computed } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import {
  Search, Plus, DocumentChecked, Edit, Check,
  Refresh, Box, Setting, Delete,
} from '@element-plus/icons-vue'
import { inspectionApi, batchApi } from '@/api/fqc'
import { inspectionPlanApi } from '@/api/inspectionPlan'
import type { FqcInspection, FqcInspectionDetail, CreateFqcInspection, SubmitFqcInspection, FqcInspectionItemSubmit } from '@/types/fqc'
import type { ProductBatch } from '@/types/fqc'
import type { PagedRequest, PagedResult } from '@/types/basicData'
import RightPanel from '@/components/layout/RightPanel.vue'
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

// ─── Helpers ──────────────────────────────────────
function formatDate(d?: string) {
  if (!d) return '-'
  return new Date(d).toLocaleString('zh-CN')
}

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

onMounted(fetchList)
</script>

<template>
  <div class="page-container">
    <!-- Page Header Banner -->
    <div class="page-header-banner page-header-banner--primary">
      <div class="page-header-banner-main">
        <div class="page-header-banner-icon">
          <el-icon :size="28"><DocumentChecked /></el-icon>
        </div>
        <div class="page-header-banner-text">
          <h2 class="page-header-banner-title">FQC 成品检验</h2>
          <span class="page-header-banner-subtitle">管理成品检验单，跟踪检验进度与判定结果</span>
        </div>
      </div>
    </div>

    <!-- Content Area -->
    <div class="iqc-content">
      <!-- Stats Bar -->
      <div v-if="list.length > 0" class="stats-bar">
        <div class="stat-item stat-pending">
          <div class="stat-accent"></div>
          <div class="stat-content">
            <div class="stat-value">{{ stats.total }}</div>
            <div class="stat-label">检验单总数</div>
          </div>
        </div>
        <div class="stat-item stat-inspecting">
          <div class="stat-accent"></div>
          <div class="stat-content">
            <div class="stat-value">{{ stats.pending }}</div>
            <div class="stat-label">待检验</div>
          </div>
        </div>
        <div class="stat-item stat-qualified">
          <div class="stat-accent"></div>
          <div class="stat-content">
            <div class="stat-value">{{ stats.passRate }}%</div>
            <div class="stat-label">合格率</div>
          </div>
        </div>
      </div>

      <!-- Data Card with Toolbar -->
      <div class="data-card">
        <div class="data-card__header">
          <span class="data-card__title">
            检验清单
            <el-tag v-if="total" type="info" size="small">{{ total }} 条</el-tag>
          </span>
          <div class="data-card__actions">
            <el-input
              v-model="query.keyword"
              placeholder="搜索检验单号/批次号"
              clearable
              size="small"
              :prefix-icon="Search"
              style="width: 220px"
              @keyup.enter="fetchList"
            />
            <el-select
              v-model="query.status"
              clearable
              placeholder="检验结论"
              size="small"
              style="width: 120px"
              @change="fetchList"
            >
              <el-option v-for="opt in FQC_CONCLUSION_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
            </el-select>
            <el-button size="small" @click="fetchList">
              <el-icon><Refresh /></el-icon>刷新
            </el-button>
            <el-button type="primary" size="small" @click="openCreate">
              <el-icon><Plus /></el-icon>新建检验单
            </el-button>
          </div>
        </div>

        <el-table
          :data="list"
          border
          stripe
          v-loading="loading"
          @row-click="row => openDetail(row.id)"
          style="width: 100%"
          size="small"
          class="data-card__table"
        >
          <el-table-column type="index" label="序号" width="55" fixed class-name="index-cell" />
          <el-table-column prop="inspectionNo" label="检验单号" min-width="160" show-overflow-tooltip />
          <el-table-column prop="batchCode" label="批次号" min-width="150" show-overflow-tooltip />
          <el-table-column label="检验方式" width="80" align="center">
            <template #default="{ row }">
              <el-tag :type="inspectionTypeTag(row.inspectionType)" size="small" effect="plain">
                {{ inspectionTypeLabel(row.inspectionType) }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="sampleSize" label="样本量" width="80" align="center" />
          <el-table-column prop="totalChecked" label="已检" width="70" align="center" />
          <el-table-column prop="totalPass" label="合格" width="70" align="center" />
          <el-table-column prop="totalFail" label="不合格" width="70" align="center" />
          <el-table-column label="结论" width="90" align="center">
            <template #default="{ row }">
              <el-tag :type="conclusionTag(row.conclusion)" size="small" effect="plain" round>
                {{ conclusionLabel(row.conclusion) }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="checkedAt" label="检验时间" min-width="140">
            <template #default="{ row }">{{ formatDate(row.checkedAt) }}</template>
          </el-table-column>
          <el-table-column label="操作" width="140" fixed="right" align="center">
            <template #default="{ row }">
              <el-button size="small" type="primary" link @click.stop="openDetail(row.id)">详情</el-button>
              <el-button
                v-if="row.conclusion === 'pending'"
                size="small" type="success" link @click.stop="openSubmit(row.id)"
              >提交</el-button>
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
            layout="total, sizes, prev, pager, next, jumper"
            @size-change="fetchList"
            @current-change="fetchList"
          />
        </div>
      </div>
    </div>

    <!-- ================================================================== -->
    <!-- Drawers -->
    <!-- ================================================================== -->

    <!-- RightPanel: 检验详情 -->
    <RightPanel v-model:visible="detailVisible" title="检验详情" :width="640" :show-close="true">
      <template #body>
        <template v-if="detail">
          <!-- 基本信息 -->
          <div class="drawer-section">
            <div class="drawer-section-header">
              <el-icon class="drawer-section-icon"><DocumentChecked /></el-icon>
              <span>基本信息</span>
            </div>
            <el-descriptions :column="2" border size="default">
              <el-descriptions-item label="批次号">{{ detail.batchCode }}</el-descriptions-item>
              <el-descriptions-item label="产品名称">
                <span class="desc-highlight">{{ detail.productName }}</span>
              </el-descriptions-item>
              <el-descriptions-item label="检验方式">
                <el-tag size="small" effect="plain">{{ inspectionTypeLabel(detail.inspectionType) }}</el-tag>
              </el-descriptions-item>
              <el-descriptions-item label="样本量/总数量">
                {{ detail.sampleSize }} / {{ detail.batchQuantity }}
              </el-descriptions-item>
              <el-descriptions-item label="合格/不合格">
                {{ detail.totalPass }} / {{ detail.totalFail }}
              </el-descriptions-item>
              <el-descriptions-item label="检验时间">
                {{ detail.checkedAt || '-' }}
              </el-descriptions-item>
            </el-descriptions>
          </div>

          <!-- 进度条 -->
          <div v-if="detail.totalChecked > 0 || detail.sampleSize > 0" class="drawer-section">
            <div class="drawer-section-header">
              <el-icon class="drawer-section-icon"><Check /></el-icon>
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
          <div class="drawer-section">
            <div class="drawer-section-header">
              <el-icon class="drawer-section-icon"><Box /></el-icon>
              <span>检验明细</span>
              <el-tag type="info" size="small" effect="plain">{{ (detail.items || []).length }} 项</el-tag>
            </div>
            <el-table :data="detail.items || []" stripe size="small">
              <el-table-column prop="itemName" label="项目名称" min-width="120" show-overflow-tooltip />
              <el-table-column label="规格" width="160" align="center">
                <template #default="{ row }">
                  {{ row.usl ?? '-' }} ~ {{ row.lsl ?? '-' }}
                </template>
              </el-table-column>
              <el-table-column prop="actualValue" label="实测值" width="100" align="center" />
              <el-table-column label="结果" width="90" align="center">
                <template #default="{ row }">
                  <el-tag :type="conclusionResultType(row.result)" size="small" round>
                    {{ conclusionResultLabel(row.result) }}
                  </el-tag>
                </template>
              </el-table-column>
            </el-table>
          </div>
        </template>
      </template>
    </RightPanel>

    <!-- RightPanel: 新建检验单 -->
    <RightPanel v-model:visible="createVisible" title="新建成品检验单" :width="560">
      <template #body>
        <!-- 批次信息 -->
        <div class="dialog-section">
          <div class="dialog-section-header">
            <el-icon class="dialog-section-icon"><Box /></el-icon>
            <span>批次信息</span>
          </div>
          <el-form :model="createForm" label-width="90px">
            <el-form-item label="批次" required>
              <el-select
                v-model="createForm.batchId"
                placeholder="请选择批次"
                style="width: 100%"
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
          <div class="dialog-section-header">
            <el-icon class="dialog-section-icon"><Setting /></el-icon>
            <span>抽样参数</span>
          </div>
          <el-form :model="createForm" label-width="90px">
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
              <el-row :gutter="16">
                <el-col :span="8">
                  <el-form-item label="样本量">
                    <el-input-number v-model="createForm.sampleSize" :min="1" style="width: 100%" controls-position="right" />
                  </el-form-item>
                </el-col>
                <el-col :span="8">
                  <el-form-item label="Ac">
                    <el-input-number v-model="createForm.ac" :min="0" style="width: 100%" controls-position="right" />
                  </el-form-item>
                </el-col>
                <el-col :span="8">
                  <el-form-item label="Re">
                    <el-input-number v-model="createForm.re" :min="1" style="width: 100%" controls-position="right" />
                  </el-form-item>
                </el-col>
              </el-row>
            </template>
          </el-form>
        </div>
      </template>
      <template #footer>
        <div style="display: flex; gap: 8px; justify-content: flex-end;">
          <el-button size="small" @click="createVisible = false">取消</el-button>
          <el-button size="small" type="primary" @click="handleCreate">
            <el-icon><Check /></el-icon>
            创建检验单
          </el-button>
        </div>
      </template>
    </RightPanel>

    <!-- RightPanel: 提交检验结果 -->
    <RightPanel v-model:visible="submitVisible" title="提交检验结果" :width="720">
      <template #body>
        <!-- 汇总统计 -->
        <div class="dialog-section">
          <div class="drawer-section-header">
            <el-icon class="drawer-section-icon"><DocumentChecked /></el-icon>
            <span>检验汇总</span>
          </div>
          <el-row :gutter="16" class="submit-stats">
            <el-col :span="8">
              <div class="submit-stat">
                <div class="submit-stat__value" :class="submitForm.totalChecked === 0 ? 'submit-stat__value--warning' : ''">
                  {{ submitForm.totalChecked }}
                </div>
                <div class="submit-stat__label">已检数量</div>
              </div>
            </el-col>
            <el-col :span="8">
              <div class="submit-stat">
                <div class="submit-stat__value submit-stat__value--success">
                  {{ submitForm.totalPass }}
                </div>
                <div class="submit-stat__label">合格数量</div>
              </div>
            </el-col>
            <el-col :span="8">
              <div class="submit-stat">
                <div class="submit-stat__value submit-stat__value--danger">
                  {{ submitForm.totalFail }}
                </div>
                <div class="submit-stat__label">不合格数量</div>
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
          <div class="drawer-section-header">
            <el-icon class="drawer-section-icon"><Box /></el-icon>
            <span>检验明细</span>
          </div>
          <div class="submit-list">
            <div v-for="(item, index) in submitForm.items" :key="index" class="submit-row">
              <span class="submit-index">{{ index + 1 }}</span>
              <el-input v-model="item.itemName" placeholder="项目名称" size="small" style="width: 140px" />
              <el-input-number
                v-model="item.usl"
                :precision="2"
                :step="0.1"
                :controls="false"
                size="small"
                style="width: 100px"
                placeholder="USL"
                controls-position="right"
              />
              <el-input-number
                v-model="item.lsl"
                :precision="2"
                :step="0.1"
                :controls="false"
                size="small"
                style="width: 100px"
                placeholder="LSL"
                controls-position="right"
              />
              <el-input-number
                v-model="item.actualValue"
                :precision="2"
                :step="0.1"
                :controls="false"
                size="small"
                style="width: 100px"
                placeholder="实测值"
                controls-position="right"
              />
              <el-select v-model="item.result" size="small" style="width: 90px">
                <el-option label="合格" value="pass" />
                <el-option label="不合格" value="fail" />
                <el-option label="待检" value="pending" />
              </el-select>
              <el-button link size="small" type="danger" @click="removeItem(index)">
                <el-icon><Delete /></el-icon>
              </el-button>
            </div>
          </div>
          <el-button size="small" @click="addItem" style="margin-top: 8px">
            <el-icon><Plus /></el-icon>添加项目
          </el-button>
        </div>
      </template>
      <template #footer>
        <div style="display: flex; gap: 8px; justify-content: flex-end;">
          <el-button size="small" @click="submitVisible = false">取消</el-button>
          <el-button size="small" type="primary" @click="handleSubmit">
            <el-icon><Check /></el-icon>
            提交结果
          </el-button>
        </div>
      </template>
    </RightPanel>
  </div>
</template>

<style scoped>
/* ─── Submit Stats ─────────────────── */
.submit-stats {
  margin-bottom: 12px;
}

.submit-stat {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
  padding: 12px;
  background: var(--el-fill-color-lighter, #f2f3f5);
  border-radius: 6px;
}

.submit-stat__value {
  font-size: 24px;
  font-weight: 700;
  color: var(--el-color-primary, #409eff);
  line-height: 1.2;
}

.submit-stat__value--success {
  color: var(--el-color-success, #67c23a);
}

.submit-stat__value--danger {
  color: var(--el-color-danger, #f56c6c);
}

.submit-stat__value--warning {
  color: var(--el-color-warning, #e6a23c);
}

.submit-stat__label {
  font-size: 12px;
  color: var(--text-secondary, #909399);
}

/* ─── Table Link Buttons ───────────── */
.data-card__table .el-button.is-link {
  padding: 0 4px;
  border: none !important;
  box-shadow: none !important;
  background: transparent !important;
}

.data-card__table .el-button.is-link:hover,
.data-card__table .el-button.is-link:focus,
.data-card__table .el-button.is-link:focus-visible,
.data-card__table .el-button.is-link:active {
  border: none !important;
  box-shadow: none !important;
  background: transparent !important;
  outline: none;
}
</style>
