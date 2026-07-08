<script setup lang="ts">
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'
import { useAppStore } from '@/stores/appStore'

const authStore = useAuthStore()
const appStore = useAppStore()
const router = useRouter()

authStore.setOnSessionExpired(() => {
  router.push('/login')
})

onMounted(() => {
  authStore.initFromStorage()
  appStore.applyTheme()
})
</script>

<template>
  <router-view />
</template>
