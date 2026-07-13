<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { supplierScoreApi } from '@/api/iqc'
import { supplierApi } from '@/api/basicData'
import type { SupplierScore, UpdateSupplierScore } from '@/types/iqc'

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
    <div class="toolbar-row">
      <el-select v-model="scoreSupplierId" placeholder="选择供应商" filterable style="width: 300px" @change="loadSupplierScore">
        <el-option
          v-for="opt in supplierOptions"
          :key="opt.value"
          :label="opt.label"
          :value="opt.value"
        />
      </el-select>
    </div>

    <div v-if="supplierScore" class="score-card">
      <el-card>
        <template #header>
          <div class="card-header">
            <span>供应商评分 - {{ supplierScore.supplierName }}</span>
            <el-button type="primary" size="small" @click="updateSupplierScore">保存评分</el-button>
          </div>
        </template>
        <el-form label-width="120px" >
          <el-form-item label="综合评分">
            <el-input-number v-model="supplierScore.score" :min="0" :max="100" :precision="2" style="width: 200px" />
            <el-tag :type="supplierScore.grade === 'A' ? 'success' : supplierScore.grade === 'B' ? 'primary' : supplierScore.grade === 'C' ? 'warning' : 'danger'" style="margin-left: 12px">
              {{ supplierScore.grade }} 级
            </el-tag>
          </el-form-item>
          <el-form-item label="评级">
            <el-select v-model="supplierScore.grade" style="width: 200px">
              <el-option label="A 级 (优秀)" value="A" />
              <el-option label="B 级 (良好)" value="B" />
              <el-option label="C 级 (合格)" value="C" />
              <el-option label="D 级 (不合格)" value="D" />
            </el-select>
          </el-form-item>
          <el-form-item label="评估日期">
            <span>{{ formatDate(supplierScore.scoreDate) }}</span>
          </el-form-item>
          <el-form-item label="评估意见">
            <el-input v-model="supplierScore.evaluation" type="textarea" :rows="3" />
          </el-form-item>
        </el-form>
      </el-card>
    </div>

    <div v-else-if="scoreSupplierId" class="empty-state">
      <p>暂无评分数据，提交检验后将自动生成评分</p>
    </div>
  </div>
</template>

<style scoped>
.page-container { display: flex; flex-direction: column; height: 100%; }
.toolbar-row { display: flex; align-items: center; gap: 8px; margin-bottom: 12px; flex-wrap: wrap; }
.score-card { margin-top: 12px; max-width: 600px; }
.card-header { display: flex; align-items: center; justify-content: space-between; }
.empty-state { text-align: center; padding: 40px; color: var(--el-text-color-secondary); }
</style>
