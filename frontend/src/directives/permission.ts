import type { Directive, DirectiveBinding } from 'vue'
import { useAuthStore } from '@/stores/authStore'

/**
 * v-permission 按钮级权限指令
 * 用法: v-permission="'system:user'"
 * 无权限时移除 DOM 元素
 */
export const permission: Directive = {
  mounted(el: HTMLElement, binding: DirectiveBinding<string>) {
    const authStore = useAuthStore()
    const required = binding.value
    if (required && !authStore.hasPermission(required)) {
      el.parentNode?.removeChild(el)
    }
  },
}
