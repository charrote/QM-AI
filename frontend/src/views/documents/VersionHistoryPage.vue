<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { documentApi } from '@/api/document'
import { Download } from '@element-plus/icons-vue'
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
    <!-- Toolbar -->
    <div class="toolbar-row">
      <el-input
        v-model="searchDocId"
        placeholder="输入文档ID查询版本历史"
        clearable
        style="width: 300px"
        @keyup.enter="handleSearch"
      />
      <el-button type="primary" @click="handleSearch">查询</el-button>
    </div>

    <!-- Table -->
    <el-table :data="versions" stripe v-loading="tableLoading" style="width: 100%" size="small">
      <el-table-column prop="documentId" label="文档ID" width="100" />
      <el-table-column prop="version" label="版本" width="80" />
      <el-table-column prop="changeDescription" label="变更说明" min-width="200" show-overflow-tooltip />
      <el-table-column prop="createdBy" label="创建人" width="100" />
      <el-table-column prop="createdAt" label="创建时间" width="180">
        <template #default="{ row }">{{ formatDate(row.createdAt) }}</template>
      </el-table-column>
      <el-table-column label="操作" width="100" fixed="right">
        <template #default="{ row }">
          <el-button link size="small" type="primary" :icon="Download" @click="downloadVersion(row)">下载</el-button>
        </template>
      </el-table-column>
    </el-table>

    <div v-if="!tableLoading && versions.length === 0 && searchDocId" style="text-align:center;padding:40px;color:#909399">
      暂无版本历史
    </div>
  </div>
</template>

<style scoped>
.page-container {
  display: flex;
  flex-direction: column;
  height: 100%;
}
.toolbar-row {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 12px;
  flex-wrap: wrap;
}
</style>
