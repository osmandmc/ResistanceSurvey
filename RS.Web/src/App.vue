<script setup lang="ts">
import { NConfigProvider, NDialogProvider, NMessageProvider, dateTrTR, trTR } from 'naive-ui'
import { themeOverrides } from './theme'
</script>

<template>
  <n-config-provider :theme-overrides="themeOverrides" :locale="trTR" :date-locale="dateTrTR">
    <n-message-provider>
      <n-dialog-provider>
        <router-view />
      </n-dialog-provider>
    </n-message-provider>
  </n-config-provider>
</template>
