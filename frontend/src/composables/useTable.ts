import { ref, reactive } from 'vue'
import type { Ref } from 'vue'

interface SearchField {
  prop: string
  label: string
  type?: 'input' | 'select' | 'date' | 'daterange'
  options?: { label: string; value: string | number }[]
  placeholder?: string
  clearable?: boolean
}

export function useTable(searchFields: SearchField[] = []) {
  const searchParams = reactive<Record<string, any>>({})
  const showSearch = ref(true)

  // Initialize search fields
  searchFields.forEach(field => {
    searchParams[field.prop] = field.type === 'daterange' ? [] : ''
  })

  function resetSearch() {
    searchFields.forEach(field => {
      searchParams[field.prop] = field.type === 'daterange' ? [] : ''
    })
  }

  return {
    searchParams,
    showSearch,
    resetSearch,
  }
}
