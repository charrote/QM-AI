import { ref, computed } from 'vue'

export function usePagination(initialPage = 1, initialSize = 20) {
  const currentPage = ref(initialPage)
  const pageSize = ref(initialSize)
  const total = ref(0)
  const loading = ref(false)

  const totalPages = computed(() => Math.ceil(total.value / pageSize.value))

  function resetPage() {
    currentPage.value = 1
  }

  function goToPage(page: number) {
    currentPage.value = page
  }

  return {
    currentPage,
    pageSize,
    total,
    loading,
    totalPages,
    resetPage,
    goToPage,
  }
}
