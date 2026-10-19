<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { NAlert, NButton, NCheckbox, NEmpty, NFormItem, NInput, NPagination, NSelect, NSpin, NTag } from 'naive-ui'
import { ApiError } from '@/api'
import { exportUrl, listCases, type CaseFilter, type CaseListItem } from '@/casesApi'
import CompanyPicker from '@/components/CompanyPicker.vue'
import { formatDate } from '@/format'
import { getFormOptions, type Option } from '@/reviewApi'

const route = useRoute()
const router = useRouter()

// Filters live in the address (?companyId=…&year=2026&page=2), so coming back from a case keeps them.
function numberParam(name: string) {
  const value = Number(route.query[name])
  return Number.isFinite(value) && value > 0 ? value : null
}
const text = ref(typeof route.query.text === 'string' ? route.query.text : '')
const companyId = ref<number | null>(numberParam('companyId'))
const mainCompanyId = ref<number | null>(numberParam('mainCompanyId'))
const categoryId = ref<number | null>(numberParam('categoryId'))
const year = ref<number | null>(numberParam('year'))
const month = ref<number | null>(numberParam('month'))
const cityId = ref<number | null>(numberParam('cityId'))
const hasNote = ref<'any' | 'yes' | 'no'>(route.query.hasNote === 'true' ? 'yes' : route.query.hasNote === 'false' ? 'no' : 'any')
const ongoingOnly = ref(route.query.ongoingOnly === 'true')
const page = ref(numberParam('page') ?? 1)

const filter = computed<CaseFilter>(() => ({
  text: text.value.trim() || null,
  companyId: companyId.value,
  mainCompanyId: mainCompanyId.value,
  categoryId: categoryId.value,
  year: year.value,
  month: month.value,
  cityId: cityId.value,
  hasNote: hasNote.value === 'any' ? null : hasNote.value === 'yes',
  ongoingOnly: ongoingOnly.value,
}))

// Lists for the filters.
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
const noteOptions = [
  { label: 'Tümü', value: 'any' },
  { label: 'Notu olanlar', value: 'yes' },
  { label: 'Notu olmayanlar', value: 'no' },
]

// The list.
const items = ref<CaseListItem[]>([])
const total = ref(0)
const pageSize = ref(25)
const loading = ref(false)
const loadError = ref('')
let timer: ReturnType<typeof setTimeout> | undefined

async function load() {
  loading.value = true
  loadError.value = ''
  try {
    const result = await listCases(filter.value, page.value)
    items.value = result.items
    total.value = result.total
    pageSize.value = result.pageSize
  } catch (error) {
    loadError.value = error instanceof ApiError ? error.message : 'Vakalar yüklenemedi.'
  } finally {
    loading.value = false
  }
}

function syncAddress() {
  const query: Record<string, string> = {}
  for (const [key, value] of Object.entries(filter.value)) {
    if (value !== null && value !== undefined && value !== '' && value !== false) query[key] = String(value)
  }
  if (page.value > 1) query.page = String(page.value)
  void router.replace({ query })
}

// A changed filter starts again at page 1; typing waits a moment before searching.
watch(filter, () => {
  clearTimeout(timer)
  timer = setTimeout(() => {
    if (page.value !== 1) {
      page.value = 1 // the page watcher loads
    } else {
      syncAddress()
      void load()
    }
  }, 300)
})
watch(page, () => {
  syncAddress()
  void load()
})
void load()

function clearFilters() {
  text.value = ''
  companyId.value = null
  mainCompanyId.value = null
  categoryId.value = null
  year.value = null
  month.value = null
  cityId.value = null
  hasNote.value = 'any'
  ongoingOnly.value = false
}

const download = computed(() => exportUrl(filter.value))
const pageCount = computed(() => Math.max(1, Math.ceil(total.value / pageSize.value)))

function open(item: CaseListItem) {
  void router.push(`/cases/${item.id}`)
}
</script>

<template>
  <div class="page-head">
    <div>
      <h1 class="heading">Vakalar</h1>
      <p class="description">Direnişler ve eylemleri. Bir vakayı açmak için satıra tıklayın.</p>
    </div>
    <div class="head-actions">
      <n-button tag="a" :href="download">Dışa Aktar</n-button>
    </div>
  </div>

  <section class="panel filters" aria-label="Filtreler">
    <n-form-item label="Ara" label-placement="top" :show-feedback="false" class="wide">
      <n-input v-model:value="text" clearable placeholder="Vaka adı, açıklama, şirket veya kurum" />
    </n-form-item>
    <n-form-item label="Şirket" label-placement="top" :show-feedback="false">
      <company-picker v-model="companyId" input-id="filter-company" placeholder="Tümü" />
    </n-form-item>
    <n-form-item label="Ana Şirket" label-placement="top" :show-feedback="false">
      <company-picker v-model="mainCompanyId" input-id="filter-main-company" placeholder="Tümü" />
    </n-form-item>
    <n-form-item label="Vaka Niteliği" label-placement="top" :show-feedback="false">
      <n-select v-model:value="categoryId" :options="toOptions(categories)" filterable clearable placeholder="Tümü" />
    </n-form-item>
    <n-form-item label="Eylem Yılı" label-placement="top" :show-feedback="false">
      <n-select v-model:value="year" :options="yearOptions" clearable placeholder="Tümü" />
    </n-form-item>
    <n-form-item label="Eylem Ayı" label-placement="top" :show-feedback="false">
      <n-select v-model:value="month" :options="monthOptions" clearable placeholder="Tümü" />
    </n-form-item>
    <n-form-item label="İl" label-placement="top" :show-feedback="false">
      <n-select v-model:value="cityId" :options="toOptions(cities)" filterable clearable placeholder="Tümü" />
    </n-form-item>
    <n-form-item label="Kişisel Notlar" label-placement="top" :show-feedback="false">
      <n-select v-model:value="hasNote" :options="noteOptions" />
    </n-form-item>
    <div class="filter-end">
      <n-checkbox v-model:checked="ongoingOnly">Yalnızca devam edenler</n-checkbox>
      <n-button quaternary @click="clearFilters">Temizle</n-button>
    </div>
  </section>

  <section class="panel">
    <div v-if="loading" class="state"><n-spin /></div>
    <div v-else-if="loadError" class="state">
      <n-alert type="error" :show-icon="false">
        {{ loadError }}
        <n-button size="small" class="retry" @click="load">Tekrar dene</n-button>
      </n-alert>
    </div>
    <div v-else-if="items.length === 0" class="state">
      <n-empty description="Bu ölçütlere uyan vaka yok." />
    </div>
    <template v-else>
      <p class="count">{{ total }} vaka</p>
      <div class="table-wrap">
        <table class="cases">
          <thead>
            <tr>
              <th>Vaka</th>
              <th>Şirket / Kurum</th>
              <th>Nitelik</th>
              <th>İller</th>
              <th class="num">Eylem</th>
              <th>Son eylem</th>
              <th class="num">Haber</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="item in items" :key="item.id" tabindex="0" @click="open(item)" @keydown.enter="open(item)">
              <td class="name">
                <router-link :to="`/cases/${item.id}`" @click.stop>{{ item.name || `Vaka #${item.id}` }}</router-link>
                <n-tag v-if="item.ongoing" size="small" type="success" :bordered="false">devam ediyor</n-tag>
              </td>
              <td>
                {{ item.target || '—' }}
                <span v-if="item.mainCompanyName" class="sub">Ana şirket: {{ item.mainCompanyName }}</span>
              </td>
              <td>{{ item.categoryName || '—' }}</td>
              <td>{{ item.cities.join(', ') || '—' }}</td>
              <td class="num">{{ item.protestCount }}</td>
              <td class="nowrap">{{ formatDate(item.lastProtestDate) }}</td>
              <td class="num">{{ item.articleCount }}</td>
            </tr>
          </tbody>
        </table>
      </div>
      <div v-if="pageCount > 1" class="pager">
        <n-pagination v-model:page="page" :page-count="pageCount" />
      </div>
    </template>
  </section>
</template>

<style scoped>
.page-head {
  display: flex;
  flex-wrap: wrap;
  align-items: flex-end;
  justify-content: space-between;
  gap: 16px;
  margin-bottom: 20px;
}
.heading {
  margin: 0;
  font-size: 30px;
}
.description {
  margin: 6px 0 0;
  color: var(--rs-muted);
  font-size: 16px;
}
.panel {
  background: var(--rs-surface);
  border: 1px solid var(--rs-border);
  border-radius: 10px;
  margin-bottom: 20px;
}
.filters {
  padding: 16px;
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 12px 16px;
}
.wide {
  grid-column: 1 / -1;
}
.filter-end {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: 8px;
  padding-bottom: 4px;
}
.state {
  padding: 40px 16px;
}
.retry {
  margin-left: 12px;
}
.count {
  margin: 0;
  padding: 12px 16px 0;
  color: var(--rs-muted);
  font-size: 14px;
}
.table-wrap {
  overflow-x: auto;
}
.cases {
  width: 100%;
  min-width: 860px;
  border-collapse: collapse;
}
.cases th {
  text-align: left;
  font-size: 12px;
  letter-spacing: 0.06em;
  text-transform: uppercase;
  color: var(--rs-muted);
  font-weight: 600;
  padding: 12px 16px;
  border-bottom: 1px solid var(--rs-border);
}
.cases td {
  padding: 12px 16px;
  border-bottom: 1px solid #eef0f2;
  font-size: 15px;
  vertical-align: top;
}
.cases tbody tr {
  cursor: pointer;
}
.cases tbody tr:hover,
.cases tbody tr:focus {
  background: #f7f8f9;
  outline: none;
}
.name a {
  color: var(--rs-ink);
  font-weight: 600;
  text-decoration: none;
  margin-right: 8px;
}
.name a:hover {
  text-decoration: underline;
}
.sub {
  display: block;
  font-size: 13px;
  color: var(--rs-muted);
}
.num {
  text-align: right;
}
.nowrap {
  white-space: nowrap;
}
.pager {
  display: flex;
  justify-content: center;
  padding: 16px;
}
</style>
