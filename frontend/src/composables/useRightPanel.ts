/**
 * useRightPanel — 右侧面板状态管理 composable
 *
 * 用法：
 *   const { visible, open, close } = useRightPanel()
 *
 *   // 模板中：
 *   <RightPanel v-model:visible="visible" title="新建XXX">
 *     ...
 *   </RightPanel>
 */
import { ref } from 'vue'

export function useRightPanel() {
  const visible = ref(false)

  function open() {
    visible.value = true
  }

  function close() {
    visible.value = false
  }

  return { visible, open, close }
}
