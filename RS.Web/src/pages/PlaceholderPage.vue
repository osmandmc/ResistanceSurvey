<script setup lang="ts">
import { useRoute } from 'vue-router'
import { NEmpty } from 'naive-ui'

// Stands in for a screen until it is built; heading and description come from the route.
const route = useRoute()
</script>

<template>
  <h1 class="heading">{{ route.meta.title }}</h1>
  <p class="description">{{ route.meta.description }}</p>
  <div class="panel">
    <n-empty description="Bu ekran hazırlanıyor." />
  </div>
</template>

<style scoped>
.heading {
  margin: 0;
  font-size: 30px;
}
.description {
  margin: 6px 0 20px;
  color: var(--rs-muted);
  font-size: 16px;
}
.panel {
  background: var(--rs-surface);
  border: 1px solid var(--rs-border);
  border-radius: 10px;
  padding: 48px 16px;
}
</style>
