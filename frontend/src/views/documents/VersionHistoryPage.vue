<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { Refresh, Download, List } from '@element-plus/icons-vue'
import { documentApi } from '@/api/document'
import type { DocumentVersion } from '@/types/document'

defineOptions({ name: 'VersionHistoryPage' })

// ─── State ──────────────────────────────────────────────
const searchDocId = ref('')
const versions = ref<DocumentVersion[]>([])
const tableLoading = ref(false)

// ─── Helpers ────────────────────────────────────────────
function formatDate(d?: string) {
  if (!d) return '-'
  return new Date(d).toLocaleString('zh-CN')
}

// ─── CRUD ───────────────────────────────────────────────
async function loadVersions() {
  tableLoading.value = true
  try {
    if (!searchDocId.value) {
      versions.value = []
      tableLoading.value = false
      return
    }
    const docId = Number(searchDocId.value)
    if (isNaN(docId) || docId <= 0) {
      ElMessage.warning('请输入有效的文档ID')
      tableLoading.value = false
      return
    }
    versions.value = await documentApi.getVersions(docId)
  } catch (e) {
    console.error('Failed to load versions', e)
    ElMessage.error('加载版本历史失败')
  } finally {
    tableLoading.value = false
  }
}

function handleSearch() {
  loadVersions()
}

// ─── Download ───────────────────────────────────────────
function downloadVersion(row: DocumentVersion) {
  const url = row.minioKey ? `/api/v1/m12/documents/download?key=${encodeURIComponent(row.minioKey)}` : ''
  if (url) {
    window.open(url, '_blank')
  } else {
    ElMessage.warning('该版本暂无可下载的文件')
  }
}

onMounted(() => {
  // No auto-load; wait for user to search
})
</script>

<template>
  <div class="page-container">
    <!-- Page Header -->
    <div class="page-header">
      <div class="page-header__main">
        <el-icon class="page-header__icon" :size="28"><Refresh /></el-icon>
        <div class="page-header__text">
          <h2 class="page-header__title">版本历史</h2>
          <p class="page-header__subtitle">文件版本变更历史追踪</p>
        </div>
      </div>
    </div>

    <!-- Search Toolbar -->
    <div class="toolbar-row">
      <el-input
        v-model="searchDocId"
        placeholder="输入文档ID查询版本历史"
        clearable
        style="width: 320px"
        @keyup.enter="handleSearch"
      />
      <el-button type="primary" :icon="List" @click="handleSearch">查询</el-button>
      <el-button :icon="Refresh" @click="loadVersions">刷新</el-button>
    </div>

    <!-- Data Table Card -->
    <div class="data-card">
      <el-table :data="versions" stripe v-loading="tableLoading" style="width: 100%">
        <el-table-column prop="documentId" label="文档ID" width="110" align="center" />
        <el-table-column prop="version" label="版本" width="80" align="center" />
        <el-table-column prop="changeDescription" label="变更说明" min-width="250" show-overflow-tooltip />
        <el-table-column prop="createdBy" label="创建人" width="110" align="center" />
        <el-table-column prop="createdAt" label="创建时间" width="190">
          <template #default="{ row }">{{ formatDate(row.createdAt) }}</template>
        </el-table-column>
        <el-table-column label="操作" width="120" fixed="right" align="center">
          <template #default="{ row }">
            <el-button link size="small" type="primary" :icon="Download" @click="downloadVersion(row)">下载</el-button>
          </template>
        </el-table-column>
      </el-table>

      <div v-if="!tableLoading && versions.length === 0 && searchDocId" class="empty-state">
        <el-icon :size="40" color="#909399"><List /></el-icon>
        <p>暂无版本历史</p>
      </div>
    </div>
  </div>
</template>

<style scoped>
.page-container {
  display: flex;
  flex-direction: column;
  height: 100%;
  gap: 16px;
}

/* Page Header */
.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  background: var(--el-bg-color);
  border-radius: var(--radius-lg, 8px);
  padding: 16px 20px;
  border: 1px solid var(--el-border-color-lighter);
}
.page-header__main { display: flex; align-items: center; gap: 12px; }
.page-header__icon { color: var(--el-color-primary); flex-shrink: 0; }
.page-header__title { margin: 0; font-size: 20px; font-weight: 600; color: var(--el-text-color-primary); line-height: 1.2; }
.page-header__subtitle { margin: 4px 0 0; font-size: 13px; color: var(--el-text-color-secondary); }
.page-header__actions { display: flex; gap: 8px; }

/* Toolbar */
.toolbar-row {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

/* Data Card */
.data-card {
  background: var(--el-bg-color);
  border-radius: var(--radius-lg, 8px);
  border: 1px solid var(--el-border-color-lighter);
  overflow: hidden;
}
.data-card :deep(.el-table th.el-table__cell) {
  background: var(--el-fill-color-light) !important;
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  padding: 40px 0;
  color: var(--el-text-color-secondary);
}
</style>