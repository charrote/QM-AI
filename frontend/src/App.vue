<script setup lang="ts">
import { onMounted } from 'vue'
import { useAuthStore } from '@/stores/authStore'
import { useAppStore } from '@/stores/appStore'

const authStore = useAuthStore()
const appStore = useAppStore()

onMounted(() => {
  authStore.initFromStorage()
  appStore.applyTheme()
})
</script>

<template>
  <router-view />
</template>
