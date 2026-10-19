<script setup lang="ts">
import { ref } from 'vue'
import { NButton, NCard, NSelect, NSpace } from 'naive-ui'

// Placeholder until the real screens (R4c onward); shows the theme is wired up.
const sample = ref<string[]>(['İş bırakma'])
const options = ['İş bırakma', 'Oturma eylemi', 'Basın açıklaması'].map((label) => ({ label, value: label }))
</script>

<template>
  <header class="bar">Direniş Takip</header>
  <main class="page">
    <n-card title="Yeni arayüz hazırlanıyor">
      <n-space vertical size="large">
        <p>Bu sayfa yeni arayüzün iskeletidir. Ekranlar sırayla buraya eklenecek.</p>
        <n-select v-model:value="sample" :options="options" multiple aria-label="Örnek seçim" />
        <n-button type="primary">Örnek düğme</n-button>
      </n-space>
    </n-card>
  </main>
</template>

<style scoped>
.bar {
  background: var(--rs-brand);
  color: #fff;
  font-weight: 700;
  font-size: 18px;
  padding: 16px 24px;
}
.page {
  max-width: 960px;
  margin: 0 auto;
  padding: 32px 16px;
}
</style>
