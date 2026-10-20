<script setup lang="ts">
import { NResult } from 'naive-ui'
</script>

<template>
  <n-result status="404" title="Sayfa bulunamadı" description="Aradığınız adres bu uygulamada yok.">
    <template #footer>
      <router-link to="/inbox">Gelen Kutusu’na dön</router-link>
    </template>
  </n-result>
</template>
