<script setup lang="ts">
import { computed } from 'vue'
import '@/charts'
import VChart from 'vue-echarts'
import type { EChartsOption } from 'echarts'

// One chart of the Analiz page (R38). The title and the source line are drawn inside the chart, so "Görsel indir"
// saves a picture that says what it shows and where it comes from.
const props = defineProps<{
  title: string
  /** The filters in words, e.g. "2019–2026 · İşletme". */
  scope: string
  option: EChartsOption
  height?: number
}>()
/** A click on a bar, a point or a map region (ECharts' event: name, value, data …). */
const emit = defineEmits<{ select: [params: { name: string; data?: unknown }] }>()

const full = computed<EChartsOption>(() => ({
  textStyle: { fontFamily: "'IBM Plex Sans', system-ui, sans-serif" },
  title: { text: props.title, left: 8, top: 4, textStyle: { fontSize: 15, fontWeight: 600, color: '#111827' } },
  toolbox: {
    right: 8,
    top: 0,
    feature: { saveAsImage: { title: 'Görsel indir', name: props.title, pixelRatio: 2, backgroundColor: '#ffffff' } },
  },
  graphic: [
    {
      type: 'text',
      left: 8,
      bottom: 4,
      style: { text: `Kaynak: Direniş Takip · ${props.scope}`, fill: '#6b7280', fontSize: 11 },
    },
  ],
  tooltip: { trigger: 'axis', confine: true, valueFormatter: (v) => (typeof v === 'number' ? v.toLocaleString('tr-TR') : String(v)) },
  legend: { type: 'scroll', top: 30, left: 8, right: 40, textStyle: { fontSize: 12 } },
  grid: { left: 48, right: 24, top: 70, bottom: 48, containLabel: true },
  ...props.option,
}))
</script>

<template>
  <section class="chart" :aria-label="title">
    <v-chart :option="full" autoresize :style="{ height: `${height ?? 340}px` }" @click="(p: { name: string; data?: unknown }) => emit('select', p)" />
  </section>
</template>

<style scoped>
.chart {
  background: var(--rs-surface);
  border: 1px solid var(--rs-border);
  border-radius: 10px;
  padding: 8px 6px 4px;
  min-width: 0;
}
</style>
