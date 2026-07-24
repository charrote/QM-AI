<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { supplierScoreApi } from '@/api/iqc'
import { supplierApi } from '@/api/basicData'
import type { SupplierScore, UpdateSupplierScore } from '@/types/iqc'
import { Shop, Search, Refresh } from '@element-plus/icons-vue'

defineOptions({ name: 'IqcSuppliersPage' })

const supplierScore = ref<SupplierScore | null>(null)
const scoreSupplierId = ref<number>(0)
const supplierOptions = ref<Array<{ value: number; label: string }>>([])

function formatDate(d?: string) {
  if (!d) return '-'
  return new Date(d).toLocaleString('zh-CN')
}

async function loadSuppliers() {
  try {
    const res = await supplierApi.list({ page: 1, pageSize: 200 })
    supplierOptions.value = res.items.map((s: any) => ({ value: s.id, label: `${s.code} - ${s.name}` }))
  } catch (e) {
    console.error('Failed to load suppliers', e)
  }
}

async function loadSupplierScore() {
  if (!scoreSupplierId.value) return
  try {
    supplierScore.value = await supplierScoreApi.get(scoreSupplierId.value)
  } catch {
    supplierScore.value = null
  }
}

async function updateSupplierScore() {
  if (!scoreSupplierId.value || !supplierScore.value) return
  try {
    await supplierScoreApi.update(scoreSupplierId.value, {
      score: supplierScore.value.score,
      grade: supplierScore.value.grade,
      evaluation: supplierScore.value.evaluation,
    })
    ElMessage.success('供应商评分已更新')
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '更新失败')
  }
}

onMounted(async () => {
  await loadSuppliers()
})
</script>

<template>
  <div class="page-container">
    <!-- Page Header -->
    <div class="page-header">
      <div class="page-header-left">
        <el-icon class="page-header-icon"><Shop /></el-icon>
        <div class="page-header-text">
          <h1>供应商评价</h1>
          <p>来料供应商质量评价与管理</p>
        </div>
      </div>
      <div class="page-header-right">
        <el-button :icon="Refresh" circle @click="loadSupplierScore" title="刷新" />
      </div>
    </div>

    <!-- Toolbar -->
    <div class="toolbar-row">
      <div class="toolbar-left">
        <el-select
          v-model="scoreSupplierId"
          placeholder="搜索供应商..."
          filterable
          clearable
          :prefix-icon="Search"
          style="width: 300px"
          @change="loadSupplierScore"
          @clear="supplierScore = null"
        >
          <el-option
            v-for="opt in supplierOptions"
            :key="opt.value"
            :label="opt.label"
            :value="opt.value"
          />
        </el-select>
      </div>
    </div>

    <!-- Score Card -->
    <div v-if="supplierScore" class="score-card" :class="`grade-${supplierScore.grade}`">
      <div class="data-card">
        <div class="data-card-header">
          <span class="data-card-title">
            <el-icon><Shop /></el-icon>
            供应商评分 - {{ supplierScore.supplierName }}
          </span>
          <el-tag :type="supplierScore.grade === 'A' ? 'success' : supplierScore.grade === 'B' ? 'primary' : supplierScore.grade === 'C' ? 'warning' : 'danger'" size="default" effect="dark">
            {{ supplierScore.grade }} 级供应商
          </el-tag>
        </div>
        <el-form label-width="120px" style="padding: 20px">
          <el-divider content-position="left">评分信息</el-divider>
          <el-row :gutter="24">
            <el-col :span="12">
              <el-form-item label="综合评分">
                <el-input-number v-model="supplierScore.score" :min="0" :max="100" :precision="2" style="width: 100%" controls-position="right" />
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item label="评级">
                <el-select v-model="supplierScore.grade" style="width: 100%">
                  <el-option label="A 级 (优秀)" value="A" />
                  <el-option label="B 级 (良好)" value="B" />
                  <el-option label="C 级 (合格)" value="C" />
                  <el-option label="D 级 (不合格)" value="D" />
                </el-select>
              </el-form-item>
            </el-col>
          </el-row>
          <el-form-item label="评估日期">
            <span class="date-display">{{ formatDate(supplierScore.scoreDate) }}</span>
          </el-form-item>
          <el-divider content-position="left">评估意见</el-divider>
          <el-form-item label="">
            <el-input v-model="supplierScore.evaluation" type="textarea" :rows="4" placeholder="请输入评估意见..." />
          </el-form-item>
        </el-form>
        <div class="score-card-footer">
          <el-button type="primary" @click="updateSupplierScore">保存评分</el-button>
        </div>
      </div>
    </div>

    <div v-else-if="scoreSupplierId" class="empty-state">
      <p>暂无评分数据，提交检验后将自动生成评分</p>
    </div>
    <div v-else class="empty-state">
      <p>请选择供应商查看评价信息</p>
    </div>
  </div>
</template>

<style scoped>
.page-container { display: flex; flex-direction: column; height: 100%; }

/* ─── Page Header ─────────────────────────────── */
.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 20px;
  padding-bottom: 16px;
  border-bottom: 1px solid var(--el-border-color-lighter);
}
.page-header-left {
  display: flex;
  align-items: center;
  gap: 14px;
}
.page-header-icon {
  width: 44px;
  height: 44px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--el-color-success-light-9);
  border-radius: 10px;
  color: var(--el-color-success);
  font-size: 22px;
}
.page-header-text h1 {
  margin: 0;
  font-size: 22px;
  font-weight: 700;
  color: var(--el-text-color-primary);
  line-height: 1.3;
}
.page-header-text p {
  margin: 2px 0 0;
  font-size: 13px;
  color: var(--el-text-color-secondary);
}
.page-header-right {
  display: flex;
  align-items: center;
  gap: 12px;
}

/* ─── Toolbar ─────────────────────────────────── */
.toolbar-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 16px;
  flex-wrap: wrap;
}
.toolbar-left {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

/* ─── Score Card ──────────────────────────────── */
.score-card {
  background: #fff;
  border-radius: 10px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.06);
  overflow: hidden;
  margin-bottom: 16px;
}
.score-card-footer {
  display: flex;
  justify-content: flex-end;
  padding: 14px 20px;
  border-top: 1px solid var(--el-border-color-lighter);
  background: var(--el-fill-color-blank);
}
.date-display {
  color: var(--el-text-color-regular);
  font-size: 14px;
}

.empty-state {
  text-align: center;
  padding: 60px 20px;
  color: var(--el-text-color-secondary);
  background: #fff;
  border-radius: 10px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.06);
}

/* Grade-specific theming */
.grade-A { border-left: 4px solid var(--el-color-success); }
.grade-B { border-left: 4px solid var(--el-color-primary); }
.grade-C { border-left: 4px solid var(--el-color-warning); }
.grade-D { border-left: 4px solid var(--el-color-danger); }
</style>
