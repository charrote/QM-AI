<script setup lang="ts">
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'

defineOptions({ name: 'FQC' })

const router = useRouter()
onMounted(() => {
  router.replace('/fqc/inspections')
})
</script>

<template>
  <div />
</template>
