import { defineStore } from 'pinia'
import { ref } from 'vue'
import { organizationApi } from '@/api/organization'
import type { OrganizationTreeNode } from '@/types/organization'

/**
 * 企业组织上下文 Store
 * 管理当前选中的组织层级，供所有质量数据页面做数据过滤
 */
export const useOrgStore = defineStore('org', () => {
  // 组织树
  const orgTree = ref<OrganizationTreeNode[]>([])
  // 扁平列表（含层级路径信息）
  const orgList = ref<{ id: number; code: string; name: string; level: string; levelLabel: string; parentId: number | null; path: string }[]>([])
  // 当前选中的组织ID
  const selectedOrgId = ref<number | null>(null)
  // 当前选中的组织名称
  const selectedOrgName = ref('')

  const LEVEL_LABEL: Record<string, string> = {
    group: '集团',
    company: '公司',
    workshop: '车间',
    line: '产线',
  }

  /** 加载组织树并构建扁平列表 */
  async function loadOrgTree() {
    try {
      const tree = await organizationApi.tree()
      orgTree.value = tree
      buildFlatList(tree, '')
    } catch {
      // ignore
    }
  }

  /** 递归构建扁平列表 */
  function buildFlatList(nodes: OrganizationTreeNode[], parentPath: string) {
    for (const node of nodes) {
      const path = parentPath ? `${parentPath} / ${node.name}` : node.name
      orgList.value.push({
        id: node.id,
        code: node.code,
        name: node.name,
        level: node.level,
        levelLabel: LEVEL_LABEL[node.level] || node.level,
        parentId: node.parentId,
        path,
      })
      if (node.children?.length) {
        buildFlatList(node.children, path)
      }
    }
  }

  /** 选择组织 */
  function selectOrg(id: number | null, name: string) {
    selectedOrgId.value = id
    selectedOrgName.value = name
  }

  /** 重置选择 */
  function clearSelection() {
    selectedOrgId.value = null
    selectedOrgName.value = ''
  }

  return {
    orgTree,
    orgList,
    selectedOrgId,
    selectedOrgName,
    loadOrgTree,
    selectOrg,
    clearSelection,
  }
})
