<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { NAlert, NButton, NFormItem, NInput, NSelect, NSpin } from 'naive-ui'
import type { EChartsOption } from 'echarts'
import { errorMessage } from '@/api'
import { getAnalysis, type AnalysisResults, type AnalysisSeries, type CaseAnalysis } from '@/analysisApi'
import type { CaseFilter } from '@/casesApi'
import AnalysisChart from '@/components/AnalysisChart.vue'
import { getFormOptions, type Option } from '@/reviewApi'
import { turkishFilter } from '@/search'

// Analiz (R38, part 2): status charts for the cases the case list's filters select. The filters live in the address
// with the same names as on Vakalar, so a link moves between the two with the same selection.
const route = useRoute()
const router = useRouter()

function numberParam(name: string) {
  const value = Number(route.query[name])
  return Number.isFinite(value) && value > 0 ? value : null
}
const text = ref(typeof route.query.text === 'string' ? route.query.text : '')
const categoryId = ref<number | null>(numberParam('categoryId'))
const year = ref<number | null>(numberParam('year'))
const month = ref<number | null>(numberParam('month'))
const cityId = ref<number | null>(numberParam('cityId'))

const filter = computed<CaseFilter>(() => ({
  text: text.value.trim() || null,
  categoryId: categoryId.value,
  year: year.value,
  month: month.value,
  cityId: cityId.value,
}))
const query = computed(() => {
  const q: Record<string, string> = {}
  for (const [key, value] of Object.entries(filter.value)) if (value !== null && value !== undefined) q[key] = String(value)
  return q
})

const categories = ref<Option[]>([])
const cities = ref<Option[]>([])
getFormOptions()
  .then((o) => {
    categories.value = o.categories
    cities.value = o.cities
  })
  .catch(() => {
    // the other filters still work
  })
const toOptions = (list: Option[]) => list.map((o) => ({ label: o.name, value: o.id }))
const yearOptions = Array.from({ length: new Date().getFullYear() - 2018 }, (_, i) => {
  const y = new Date().getFullYear() - i
  return { label: String(y), value: y }
})
const monthNames = ['Ocak', 'Şubat', 'Mart', 'Nisan', 'Mayıs', 'Haziran', 'Temmuz', 'Ağustos', 'Eylül', 'Ekim', 'Kasım', 'Aralık']
const monthOptions = monthNames.map((label, i) => ({ label, value: i + 1 }))

const data = ref<CaseAnalysis | null>(null)
const loading = ref(false)
const loadError = ref('')
let timer: ReturnType<typeof setTimeout> | undefined

async function load() {
  loading.value = true
  loadError.value = ''
  try {
    data.value = await getAnalysis(filter.value)
  } catch (error) {
    loadError.value = errorMessage(error, 'Grafikler yüklenemedi.')
  } finally {
    loading.value = false
  }
}
void load()
watch(filter, () => {
  clearTimeout(timer)
  timer = setTimeout(() => {
    void router.replace({ query: query.value })
    void load()
  }, 300)
})

// What the charts show, in words: drawn under every chart and in its saved picture.
const scope = computed(() => {
  const parts: string[] = []
  const d = data.value
  if (year.value) parts.push(month.value ? `${monthNames[month.value - 1]} ${year.value}` : String(year.value))
  else if (d?.years.length) parts.push(d.years.length > 1 ? `${d.years[0]}–${d.years[d.years.length - 1]}` : String(d.years[0]))
  else if (month.value) parts.push(monthNames[month.value - 1])
  const category = categories.value.find((c) => c.id === categoryId.value)
  parts.push(category ? category.name : 'tüm kategoriler')
  const city = cities.value.find((c) => c.id === cityId.value)
  if (city) parts.push(city.name)
  if (filter.value.text) parts.push(`“${filter.value.text}”`)
  return parts.join(' · ')
})

// Colours: the app's teal first; categories and results keep fixed colours on every chart.
const palette = ['#0b5e5e', '#c2410c', '#4f6f85', '#ca8a04', '#7e5a9b', '#2f855a', '#b83280', '#64748b', '#9ca3af']
const categoryColors: Record<string, string> = { İşletme: '#0b5e5e', Genel: '#4f6f85', Dayanışma: '#ca8a04' }
const otherColor = '#9ca3af'
const fmt = (n: number) => n.toLocaleString('tr-TR')
// ECharts groups digits with commas; Turkish uses dots (2.100).
const numberAxis = { type: 'value' as const, minInterval: 1, axisLabel: { formatter: (v: number) => fmt(v) } }
const numberLabel = (position: 'top' | 'right') => ({ show: true, position, formatter: (p: { value: unknown }) => fmt(Number(p.value)) })
const percent = (part: number, whole: number) => (whole ? Math.round((part / whole) * 100) : 0)
const monthLabel = (m: string) => `${monthNames[Number(m.slice(5)) - 1].slice(0, 3)} ${m.slice(0, 4)}`

function seriesColors(series: AnalysisSeries[]) {
  return series.map((s, i) => (s.name === 'Diğer' ? otherColor : palette[i % palette.length]))
}

const monthly = computed<EChartsOption>(() => {
  const d = data.value!
  return {
    color: d.protestsByCategory.map((s) => categoryColors[s.name] ?? otherColor).concat('#111827'),
    xAxis: { type: 'category', data: d.months.map(monthLabel), axisLabel: { hideOverlap: true } },
    // The axes are named in the legend (the right one carries the line), so their names do not crowd it.
    yAxis: [numberAxis, { ...numberAxis, splitLine: { show: false } }],
    series: [
      ...d.protestsByCategory.map((s) => ({ name: s.name, type: 'bar' as const, stack: 'p', data: s.values, barMaxWidth: 18 })),
      { name: 'Eylemi olan vaka (sağ eksen)', type: 'line' as const, yAxisIndex: 1, data: d.activeCases, symbol: 'none', lineStyle: { width: 1.5 } },
    ],
  }
})

function byYear(series: AnalysisSeries[], kind: 'bar' | 'line'): EChartsOption {
  const d = data.value!
  return {
    color: seriesColors(series),
    xAxis: { type: 'category', data: d.years.map(String) },
    yAxis: numberAxis,
    series: series.map((s) =>
      kind === 'bar'
        ? { name: s.name, type: 'bar' as const, stack: 'y', data: s.values, barMaxWidth: 36 }
        : { name: s.name, type: 'line' as const, data: s.values, symbolSize: 6 },
    ),
  }
}

const confederations = computed<EChartsOption>(() => {
  const rows = [...data.value!.confederations].reverse()
  return {
    color: ['#0b5e5e'],
    legend: { show: false },
    tooltip: { trigger: 'axis', axisPointer: { type: 'shadow' } },
    grid: { left: 8, right: 48, top: 44, bottom: 40, containLabel: true },
    xAxis: numberAxis,
    yAxis: { type: 'category', data: rows.map((r) => r.name), axisLabel: { width: 240, overflow: 'truncate' } },
    series: [{ type: 'bar', data: rows.map((r) => r.count), barMaxWidth: 18, label: numberLabel('right') }],
  }
})

// Results among the cases whose result is known; how many are known is in the group's label.
function results(rows: AnalysisResults[]): EChartsOption {
  const known = (r: AnalysisResults) => r.full + r.partial + r.zero
  const shown = [...rows].filter((r) => known(r) > 0).reverse()
  const part = (key: 'full' | 'partial' | 'zero', name: string) => ({
    name,
    type: 'bar' as const,
    stack: 'r',
    barMaxWidth: 20,
    data: shown.map((r) => ({ value: percent(r[key], known(r)), count: r[key] })),
    label: { show: true, formatter: (p: { value: unknown }) => (Number(p.value) >= 8 ? `%${p.value}` : '') },
  })
  return {
    color: ['#2f855a', '#ca8a04', '#c2410c'],
    tooltip: {
      trigger: 'item',
      formatter: (p: unknown) => {
        const item = p as { seriesName: string; name: string; value: number; data: { count: number } }
        return `${item.name}<br/>${item.seriesName}: ${fmt(item.data.count)} vaka (%${item.value})`
      },
    },
    grid: { left: 8, right: 24, top: 70, bottom: 40, containLabel: true },
    xAxis: { type: 'value', max: 100, axisLabel: { formatter: '%{value}' } },
    yAxis: {
      type: 'category',
      data: shown.map((r) => `${r.group} (bilinen ${fmt(known(r))})`),
      axisLabel: { width: 260, overflow: 'truncate' },
    },
    series: [part('full', 'Tam Kazanım'), part('partial', 'Yarım Kazanım'), part('zero', 'Sıfır Kazanım')],
  }
}

const durations = computed<EChartsOption>(() => ({
  color: ['#0b5e5e'],
  legend: { show: false },
  tooltip: { trigger: 'axis', axisPointer: { type: 'shadow' } },
  xAxis: { type: 'category', data: data.value!.durations.map((d) => d.name), axisLabel: { interval: 0 } },
  yAxis: numberAxis,
  series: [{ type: 'bar', data: data.value!.durations.map((d) => d.count), barMaxWidth: 48, label: numberLabel('top') }],
}))

const completeness = computed<EChartsOption>(() => {
  const rows = data.value!.completeness
  return {
    color: ['#c2410c', '#4f6f85', '#ca8a04'],
    tooltip: { trigger: 'axis', valueFormatter: (v: unknown) => `%${v}` },
    xAxis: { type: 'category', data: rows.map((r) => String(r.year)) },
    yAxis: { type: 'value', max: 100, axisLabel: { formatter: '%{value}' } },
    series: [
      { name: 'Sonucu bilinmeyen vaka', type: 'line', data: rows.map((r) => percent(r.unknownResult, r.cases)), symbolSize: 6 },
      { name: 'Sendika kaydı olmayan vaka', type: 'line', data: rows.map((r) => percent(r.noUnion, r.cases)), symbolSize: 6 },
      { name: 'Yeri girilmemiş eylem', type: 'line', data: rows.map((r) => percent(r.noLocation, r.protests)), symbolSize: 6 },
    ],
  }
})

const empty = computed(() => data.value !== null && data.value.totals.protests === 0)
</script>

<template>
  <div class="head">
    <div>
      <h1 class="heading">Analiz</h1>
      <p class="description">Seçilen vakaların ve eylemlerin durumu. Süzgeçler Vakalar sayfasındakilerle aynıdır; her grafik, başlığı ve kaynak satırıyla görsel olarak indirilebilir.</p>
    </div>
    <router-link :to="{ path: '/cases', query }" class="to-cases">Bu vakaları listele →</router-link>
  </div>

  <div class="filters">
    <n-form-item label="Ara" :show-feedback="false" class="wide">
      <n-input v-model:value="text" clearable placeholder="Vaka, şirket ya da kurum adı" />
    </n-form-item>
    <n-form-item label="Kategori" :show-feedback="false">
      <n-select v-model:value="categoryId" :options="toOptions(categories)" clearable placeholder="Tümü" />
    </n-form-item>
    <n-form-item label="Yıl" :show-feedback="false">
      <n-select v-model:value="year" :options="yearOptions" clearable placeholder="Tümü" />
    </n-form-item>
    <n-form-item label="Ay" :show-feedback="false">
      <n-select v-model:value="month" :options="monthOptions" clearable placeholder="Tümü" />
    </n-form-item>
    <n-form-item label="İl" :show-feedback="false">
      <n-select v-model:value="cityId" :options="toOptions(cities)" filterable :filter="turkishFilter" clearable placeholder="Tümü" />
    </n-form-item>
  </div>

  <n-alert v-if="loadError" type="error" :show-icon="false" class="state">
    {{ loadError }} <n-button size="small" @click="load">Tekrar dene</n-button>
  </n-alert>
  <div v-else-if="!data" class="state"><n-spin /></div>
  <p v-else-if="empty" class="state muted">Bu süzgeçlerle eylem bulunamadı.</p>

  <template v-else>
    <div class="totals" :class="{ dim: loading }">
      <div><b>{{ fmt(data.totals.cases) }}</b><span>vaka</span></div>
      <div><b>{{ fmt(data.totals.protests) }}</b><span>eylem</span></div>
      <div><b>{{ fmt(data.totals.provinces) }}</b><span>il</span></div>
      <div>
        <b>%{{ percent(data.totals.knownResults, data.totals.cases) }}</b>
        <span>sonucu bilinen vaka ({{ fmt(data.totals.knownResults) }})</span>
      </div>
    </div>

    <div class="grid" :class="{ dim: loading }">
      <analysis-chart class="span" title="Aylara göre eylemler" :scope="scope" :option="monthly" :height="380" />
      <analysis-chart title="Vaka nedenleri" :scope="`${scope} · vaka sayısı, yıla göre`" :option="byYear(data.reasonsByYear, 'line')" />
      <analysis-chart title="Eylem türleri" :scope="`${scope} · eylem sayısı`" :option="byYear(data.protestTypesByYear, 'bar')" />
      <analysis-chart title="İstihdam türleri" :scope="`${scope} · vaka sayısı`" :option="byYear(data.employmentByYear, 'bar')" />
      <analysis-chart
        title="Sendikalar: konfederasyonlara göre"
        :scope="`${scope} · vaka sayısı`"
        :option="confederations"
        :height="Math.max(260, 70 + data.confederations.length * 28)"
      />
      <analysis-chart title="Sonuçlar: sendika kaydına göre" :scope="`${scope} · sonucu bilinen vakalar`" :option="results(data.resultsByUnion)" :height="240" />
      <analysis-chart
        title="Sonuçlar: istihdam türüne göre"
        :scope="`${scope} · sonucu bilinen vakalar`"
        :option="results(data.resultsByEmployment)"
        :height="Math.max(260, 90 + data.resultsByEmployment.length * 34)"
      />
      <analysis-chart title="Vakaların süresi" :scope="`${scope} · ilk eylemden son eyleme`" :option="durations" />
      <analysis-chart title="Eksik bilgi" :scope="`${scope} · yıla göre`" :option="completeness" />
    </div>
  </template>
</template>

<style scoped>
.head {
  display: flex;
  flex-wrap: wrap;
  align-items: flex-end;
  justify-content: space-between;
  gap: 8px 24px;
  margin-bottom: 12px;
}
.heading {
  margin: 0;
  font-size: 30px;
}
.description {
  margin: 6px 0 0;
  color: var(--rs-muted);
  font-size: 15px;
  max-width: 760px;
}
.to-cases {
  color: var(--rs-accent);
  font-size: 14px;
}
.filters {
  display: grid;
  grid-template-columns: minmax(220px, 2fr) repeat(4, minmax(140px, 1fr));
  gap: 12px;
  background: var(--rs-surface);
  border: 1px solid var(--rs-border);
  border-radius: 10px;
  padding: 12px 16px;
  margin-bottom: 16px;
}
@media (max-width: 900px) {
  .filters {
    grid-template-columns: 1fr 1fr;
  }
  .filters .wide {
    grid-column: 1 / -1;
  }
}
.state {
  padding: 32px 16px;
}
.muted {
  color: var(--rs-muted);
}
.totals {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 12px;
  margin-bottom: 16px;
}
.totals div {
  background: var(--rs-surface);
  border: 1px solid var(--rs-border);
  border-radius: 10px;
  padding: 12px 16px;
  display: flex;
  flex-direction: column;
}
.totals b {
  font-size: 26px;
  font-weight: 600;
  font-variant-numeric: tabular-nums;
}
.totals span {
  color: var(--rs-muted);
  font-size: 13px;
}
@media (max-width: 700px) {
  .totals {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}
.grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 16px;
}
.grid .span {
  grid-column: 1 / -1;
}
@media (max-width: 1000px) {
  .grid {
    grid-template-columns: minmax(0, 1fr);
  }
}
.dim {
  opacity: 0.6;
  transition: opacity 0.15s;
}
</style>
