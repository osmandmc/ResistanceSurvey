<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { NEmpty, NFormItem, NInput, NSelect, NSpin, NTag } from 'naive-ui'
import { formatDate } from '@/format'
import { searchCases, type CaseOption, type Option } from '@/reviewApi'
import CompanyPicker from './CompanyPicker.vue'

/**
 * Finds an existing case: the company leads (all its cases, also as main company of a subcontractor); free text
 * matches name, description and company; city and period narrow it down. Ongoing cases come first.
 */
const props = defineProps<{
  cities: Option[]
  /** Server message for the choice, if any. */
  error?: string
}>()

const resistanceId = defineModel<number | null>({ required: true })
/** The company filter; set from outside when the case form found existing cases of a company. */
const companyId = defineModel<number | null>('companyId', { default: null })
const companyName = defineModel<string | null>('companyName', { default: null })

const text = ref('')
const cityId = ref<number | null>(null)
const period = ref<'3m' | '6m' | '1y' | 'all'>('all')
const periodOptions = [
  { label: 'Tüm zamanlar', value: 'all' },
  { label: 'Son 3 ay', value: '3m' },
  { label: 'Son 6 ay', value: '6m' },
  { label: 'Son 1 yıl', value: '1y' },
]
const cityOptions = computed(() => props.cities.map((c) => ({ label: c.name, value: c.id })))

function since() {
  if (period.value === 'all') return null
  const d = new Date()
  d.setMonth(d.getMonth() - { '3m': 3, '6m': 6, '1y': 12 }[period.value])
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
}

const results = ref<CaseOption[]>([])
const searching = ref(false)
let timer: ReturnType<typeof setTimeout> | undefined

watch(
  [companyId, text, cityId, period],
  () => {
    clearTimeout(timer)
    timer = setTimeout(async () => {
      searching.value = true
      try {
        results.value = await searchCases({ query: text.value.trim(), companyId: companyId.value, cityId: cityId.value, since: since() })
      } catch {
        results.value = []
      } finally {
        searching.value = false
      }
    }, 300)
  },
  { immediate: true },
)

function describe(c: CaseOption) {
  const parts: string[] = []
  if (c.companyName) parts.push(c.companyName)
  if (c.cities.length) parts.push(c.cities.join(', '))
  parts.push(`${c.protestCount} eylem`)
  if (c.lastProtestDate) parts.push(`son eylem ${formatDate(c.lastProtestDate)}`)
  return parts.join(' · ')
}
</script>

<template>
  <div class="finder">
    <div class="filters">
      <n-form-item label="Şirket" label-placement="top" :show-feedback="false" class="company">
        <company-picker v-model="companyId" input-id="case-company" :selected-name="companyName" placeholder="Şirkete göre" />
      </n-form-item>
      <n-form-item label="Vaka adı veya açıklama" label-placement="top" :show-feedback="false">
        <n-input v-model:value="text" clearable placeholder="Ara" :input-props="{ id: 'case-text' }" />
      </n-form-item>
      <n-form-item label="İl" label-placement="top" :show-feedback="false">
        <n-select v-model:value="cityId" :options="cityOptions" filterable clearable placeholder="Tümü" />
      </n-form-item>
      <n-form-item label="Son eylem" label-placement="top" :show-feedback="false">
        <n-select v-model:value="period" :options="periodOptions" />
      </n-form-item>
    </div>

    <div class="results" role="radiogroup" aria-label="Vakalar">
      <div v-if="searching" class="state"><n-spin size="small" /></div>
      <n-empty v-else-if="results.length === 0" description="Bu ölçütlere uyan vaka yok." class="state" />
      <template v-else>
        <button
          v-for="c in results"
          :key="c.id"
          type="button"
          role="radio"
          :aria-checked="resistanceId === c.id"
          class="result"
          :class="{ selected: resistanceId === c.id }"
          @click="resistanceId = c.id"
        >
          <span class="result-head">
            <span class="result-name">{{ c.name || `Vaka #${c.id}` }}</span>
            <n-tag v-if="c.ongoing" size="small" type="success" :bordered="false">devam ediyor</n-tag>
          </span>
          <span class="result-meta">{{ describe(c) }}</span>
        </button>
      </template>
    </div>
    <span v-if="props.error" class="error">{{ props.error }}</span>
  </div>
</template>

<style scoped>
.finder {
  display: flex;
  flex-direction: column;
  gap: 10px;
}
.filters {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
  gap: 0 12px;
}
.company {
  grid-column: 1 / -1;
}
.results {
  display: flex;
  flex-direction: column;
  gap: 6px;
  max-height: 340px;
  overflow: auto;
}
.state {
  padding: 16px;
  display: flex;
  justify-content: center;
}
.result {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: 2px;
  text-align: left;
  font: inherit;
  padding: 10px 12px;
  border: 1px solid #d1d5db;
  border-radius: 8px;
  background: var(--rs-surface);
  cursor: pointer;
  color: var(--rs-ink);
}
.result:hover {
  background: #f7f8f9;
}
.result.selected {
  border-color: var(--rs-accent);
  background: #f0f8f7;
}
.result-head {
  display: flex;
  align-items: center;
  gap: 8px;
}
.result-name {
  font-weight: 600;
}
.result-meta {
  font-size: 13px;
  color: var(--rs-muted);
}
.error {
  font-size: 13px;
  color: #b42318;
}
</style>
