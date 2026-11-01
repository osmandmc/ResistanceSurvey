<script setup lang="ts">
import { computed, ref } from 'vue'
import type { EChartsOption } from 'echarts'
import type { AnalysisProvince } from '@/analysisApi'
import { provinceCodes } from '@/charts'
import AnalysisChart from '@/components/AnalysisChart.vue'

// The map of the Analiz page (R36): provinces shaded by protests or cases, darker for more. A click on a province (or
// on its row) selects it as the page's province filter; a second click clears it. The shapes: '@/charts'.

const props = defineProps<{ rows: AnalysisProvince[]; scope: string; selectedCityId: number | null }>()
const emit = defineEmits<{ select: [cityId: number | null] }>()

const measure = ref<'protests' | 'cases'>('protests')
const unit = computed(() => (measure.value === 'protests' ? 'eylem' : 'vaka'))
const fmt = (n: number) => n.toLocaleString('tr-TR')

const byId = computed(() => new Map(props.rows.map((r) => [r.cityId, r])))
const value = (r: AnalysisProvince | undefined) => (r ? (measure.value === 'protests' ? r.protests : r.cases) : 0)
const ranked = computed(() => [...props.rows].sort((a, b) => value(b) - value(a)))
const top = computed(() => ranked.value.slice(0, 12))
const max = computed(() => Math.max(0, ...props.rows.map(value)))

// Light to dark teal: the darker, the more. The steps follow the largest value, so a filtered view still uses the range.
const shades = ['#d5ebeb', '#a9d4d4', '#74b5b5', '#3f9393', '#167272', '#0b5252', '#06302f']
// With fewer steps than shades, the steps still run from light to dark.
const shade = (i: number, count: number) =>
  shades[count === 1 ? shades.length - 2 : Math.round((i * (shades.length - 1)) / (count - 1))]
const steps = computed(() => {
  // Always from 1, so every province with a record gets a colour; then the round numbers up to the largest value.
  const nice = [2, 5, 10, 20, 50, 100, 200, 500, 1000, 2000, 5000]
  return [1, ...nice.filter((n) => n <= max.value).slice(-(shades.length - 1))]
})

const option = computed<EChartsOption>(() => {
  const s = steps.value
  const pieces = [
    { value: 0, label: 'Kayıt yok', color: '#eceff1' },
    ...s.map((from, i) => {
      const to = i + 1 < s.length ? s[i + 1] - 1 : undefined
      const label = to === undefined ? `${fmt(from)}+` : from === to ? fmt(from) : `${fmt(from)}–${fmt(to)}`
      return { min: from, ...(to === undefined ? {} : { max: to }), label, color: shade(i, s.length) }
    }),
  ]
  return {
    legend: { show: false },
    tooltip: {
      trigger: 'item',
      formatter: (p: unknown) => {
        const item = p as { data?: { province: string; protests: number; cases: number } }
        if (!item.data) return ''
        return `<strong>${item.data.province}</strong><br/>${fmt(item.data.protests)} eylem · ${fmt(item.data.cases)} vaka`
      },
    },
    visualMap: {
      type: 'piecewise',
      pieces,
      orient: 'horizontal',
      left: 8,
      bottom: 26,
      itemWidth: 14,
      itemHeight: 10,
      textStyle: { fontSize: 11, color: '#4b5563' },
    },
    series: [
      {
        type: 'map',
        map: 'turkey',
        aspectScale: 0.78, // cos 39°: Türkiye's middle latitude
        top: 56,
        bottom: 64,
        left: 8,
        right: 8,
        selectedMode: false,
        showLegendSymbol: false,
        itemStyle: { borderColor: '#ffffff', borderWidth: 0.6 },
        emphasis: { label: { show: false }, itemStyle: { areaColor: '#ca8a04' } },
        data: provinceCodes.map((id) => {
          const row = byId.value.get(id)
          const selected = props.selectedCityId === id
          return {
            name: String(id),
            value: value(row),
            province: row?.name ?? '',
            protests: row?.protests ?? 0,
            cases: row?.cases ?? 0,
            ...(selected ? { itemStyle: { borderColor: '#ca8a04', borderWidth: 2 } } : {}),
          }
        }),
      },
    ],
  }
})

function choose(cityId: number) {
  emit('select', props.selectedCityId === cityId ? null : cityId)
}
</script>

<template>
  <div class="map-row">
    <analysis-chart
      class="map"
      title="İllere göre"
      :scope="`${scope} · ${unit} sayısı · sınırlar: Natural Earth`"
      :option="option"
      :height="480"
      @select="(p) => choose(Number(p.name))"
    />
    <aside class="side" aria-label="İllere göre sıralama">
      <div class="seg" role="group" aria-label="Sayılan">
        <button type="button" :aria-pressed="measure === 'protests'" @click="measure = 'protests'">Eylem</button>
        <button type="button" :aria-pressed="measure === 'cases'" @click="measure = 'cases'">Vaka</button>
      </div>
      <p class="hint">Bir ile tıklayınca bütün grafikler o ile göre süzülür. Birden çok ilde yapılan eylem her ilde sayılır.</p>
      <ol class="list">
        <li v-for="r in top" :key="r.cityId">
          <button type="button" :class="{ active: selectedCityId === r.cityId }" @click="choose(r.cityId)">
            <span class="name">{{ r.name }}</span>
            <span class="num">{{ fmt(value(r)) }}</span>
            <i :style="{ width: `${max ? (value(r) / max) * 100 : 0}%` }"></i>
          </button>
        </li>
      </ol>
      <p v-if="ranked.length > top.length" class="more">ve {{ ranked.length - top.length }} il daha</p>
    </aside>
  </div>
</template>

<style scoped>
.map-row {
  display: grid;
  grid-template-columns: minmax(0, 1fr) 280px;
  gap: 16px;
}
@media (max-width: 1000px) {
  .map-row {
    grid-template-columns: minmax(0, 1fr);
  }
}
.side {
  background: var(--rs-surface);
  border: 1px solid var(--rs-border);
  border-radius: 10px;
  padding: 14px 16px;
  display: flex;
  flex-direction: column;
  gap: 10px;
  min-width: 0;
}
.seg {
  display: inline-flex;
  align-self: flex-start;
  gap: 2px;
  background: var(--rs-ground);
  border-radius: 8px;
  padding: 3px;
}
.seg button {
  font: inherit;
  font-size: 14px;
  border: 0;
  background: transparent;
  color: var(--rs-muted);
  padding: 4px 14px;
  border-radius: 6px;
  cursor: pointer;
}
.seg button[aria-pressed='true'] {
  background: var(--rs-surface);
  color: var(--rs-ink);
  font-weight: 600;
  box-shadow: 0 1px 2px rgb(0 0 0 / 0.12);
}
.hint,
.more {
  margin: 0;
  font-size: 13px;
  color: var(--rs-muted);
}
.list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 4px;
}
.list button {
  width: 100%;
  font: inherit;
  font-size: 14px;
  text-align: left;
  border: 0;
  background: transparent;
  color: var(--rs-ink);
  padding: 4px 6px;
  border-radius: 6px;
  cursor: pointer;
  display: grid;
  grid-template-columns: minmax(0, 1fr) auto;
  gap: 2px 8px;
}
.list button:hover,
.list button.active {
  background: var(--rs-ground);
}
.list button.active .name {
  font-weight: 600;
}
.list .num {
  font-variant-numeric: tabular-nums;
}
.list i {
  grid-column: 1 / -1;
  height: 4px;
  border-radius: 2px;
  background: var(--rs-accent);
  opacity: 0.75;
}
</style>
