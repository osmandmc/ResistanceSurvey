<script setup lang="ts">
import { verdictText, verdictType, type AiVerdict } from '@/relevance'
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { NAlert, NButton, NDatePicker, NEmpty, NFormItem, NInput, NSelect, NSpin, NTag } from 'naive-ui'
import { errorMessage } from '@/api'
import { getArchive, minSearchLength, type ArchiveEvent, type ArchiveFilter, type ArchiveItem, type ArchiveState } from '@/archiveApi'
import { formatDate, formatDateTime } from '@/format'

const route = useRoute()
const router = useRouter()

// Filters, kept in the address (?q=…&month=2026-10&source=evrensel&filter=failed). No month = all months.
function text(name: string) {
  return typeof route.query[name] === 'string' ? (route.query[name] as string) : ''
}
function initialMonth() {
  const match = /^(\d{4})-(\d{2})$/.exec(text('month'))
  return match ? new Date(Number(match[1]), Number(match[2]) - 1, 1).getTime() : null
}
const month = ref<number | null>(initialMonth())
const source = ref(text('source'))
const filter = ref<ArchiveFilter>((['all', 'open', 'handled', 'failed', 'aiNotRelevant', 'aiDraft'] as const).find((f) => f === text('filter')) ?? 'all')
const searchInput = ref(text('q'))
// The search runs on Enter or "Ara", not on every key: each search reads the whole archive.
const search = ref(searchInput.value.trim())

const sourceOptions = [
  { label: 'Tümü', value: '' },
  { label: 'Evrensel', value: 'evrensel' },
  { label: 'Kızılbayrak', value: 'kizilbayrak' },
]
const filterOptions: { label: string; value: ArchiveFilter }[] = [
  { label: 'Tümü', value: 'all' },
  { label: 'Gelen kutusunda', value: 'open' },
  { label: 'İşlendi (vaka ya da ilgisiz)', value: 'handled' },
  { label: 'Hata', value: 'failed' },
  { label: 'AI: ilgisiz bulduğu', value: 'aiNotRelevant' },
  { label: 'AI: taslak hazırladığı', value: 'aiDraft' },
]
const monthParam = computed(() => {
  if (month.value === null) return null
  const date = new Date(month.value)
  return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}`
})

const searchError = computed(() =>
  searchInput.value.trim().length > 0 && searchInput.value.trim().length < minSearchLength
    ? `En az ${minSearchLength} karakter yazın.`
    : '',
)
function runSearch() {
  if (!searchError.value) search.value = searchInput.value.trim()
}
function clearSearch() {
  searchInput.value = ''
  search.value = ''
}
const isLinkSearch = computed(() => /^https?:\/\//i.test(search.value))

const items = ref<ArchiveItem[]>([])
const next = ref<string | null>(null)
const loading = ref(false)
const loadingMore = ref(false)
const loadError = ref('')
const expanded = ref(new Set<string>())

async function load(more = false) {
  if (more) {
    loadingMore.value = true
  } else {
    loading.value = true
    items.value = []
    next.value = null
    expanded.value = new Set()
  }
  loadError.value = ''
  try {
    const query = { month: monthParam.value, source: source.value || null, search: search.value || null, filter: filter.value }
    const page = await getArchive(query, more ? next.value : null)
    items.value = more ? [...items.value, ...page.items] : page.items
    next.value = page.next
    // A link search finds one article: show its history straight away.
    if (!more && isLinkSearch.value && page.items.length === 1) expanded.value = new Set([page.items[0].articleId])
  } catch (error) {
    loadError.value = errorMessage(error, 'Haberler yüklenemedi.')
  } finally {
    loading.value = false
    loadingMore.value = false
  }
}

watch(
  [monthParam, source, filter, search],
  () => {
    const query: Record<string, string> = {}
    if (search.value) query.q = search.value
    if (monthParam.value) query.month = monthParam.value
    if (source.value) query.source = source.value
    if (filter.value !== 'all') query.filter = filter.value
    void router.replace({ query })
    void load()
  },
  { immediate: true },
)

function toggle(articleId: string) {
  const set = new Set(expanded.value)
  if (set.has(articleId)) set.delete(articleId)
  else set.add(articleId)
  expanded.value = set
}

const stateLabel: Record<ArchiveState, { text: string; type: 'default' | 'info' | 'success' | 'warning' | 'error' }> = {
  Processing: { text: 'İşleniyor', type: 'default' },
  Failed: { text: 'Hata', type: 'error' },
  Open: { text: 'Gelen kutusunda', type: 'info' },
  Linked: { text: 'Vakaya bağlandı', type: 'success' },
  Dismissed: { text: 'İlgisiz', type: 'warning' },
}

const stepLabel: Record<string, string> = {
  Submitted: 'Alındı',
  Fetched: 'Sayfa indirildi',
  Parsed: 'Metin okundu',
  Relevance: 'İlgi kontrolü',
  Extracted: 'Bilgiler çıkarıldı',
  Validated: 'Doğrulandı',
}

function eventText(event: ArchiveEvent) {
  if (event.step === 'Outcome') return event.status === 'Digested' ? 'Vakaya bağlandı' : 'İlgisiz bulundu'
  const step = stepLabel[event.step] ?? event.step
  if (event.status === 'Failed') return `${step}: başarısız`
  if (event.status === 'Skipped') return `${step}: atlandı`
  if (event.answer && event.answer in verdictText) return `${step}: ${verdictText[event.answer as AiVerdict]}`
  return step
}

const emptyText = computed(() =>
  search.value ? 'Aramaya uyan haber yok.' : filter.value === 'failed' ? 'Hatalı haber yok.' : 'Bu seçimle haber yok.',
)
</script>

<template>
  <div class="page-head">
    <div>
      <h1 class="heading">Haber Arşivi</h1>
      <p class="description">Her durumdaki tüm haberler ve her haberin geçmişi. Bir bağlantı yapıştırarak o haberi bulabilirsiniz.</p>
    </div>
  </div>

  <div class="filters">
    <n-form-item label="Ara" label-placement="top" :show-feedback="!!searchError" :feedback="searchError" validation-status="warning" class="search">
      <n-input
        v-model:value="searchInput"
        placeholder="Başlıkta geçen sözcük ya da haber bağlantısı"
        clearable
        :input-props="{ id: 'archive-search' }"
        @keyup.enter="runSearch"
        @clear="clearSearch"
      />
      <n-button type="primary" class="search-button" @click="runSearch">Ara</n-button>
    </n-form-item>
    <n-form-item label="Ay" label-placement="top" :show-feedback="false">
      <n-date-picker v-model:value="month" type="month" clearable format="MMMM yyyy" placeholder="Tüm aylar" />
    </n-form-item>
    <n-form-item label="Kaynak" label-placement="top" :show-feedback="false">
      <n-select v-model:value="source" :options="sourceOptions" class="narrow-select" />
    </n-form-item>
    <n-form-item label="Durum" label-placement="top" :show-feedback="false">
      <n-select v-model:value="filter" :options="filterOptions" class="wide-select" />
    </n-form-item>
  </div>
  <p v-if="isLinkSearch" class="hint">Bağlantıyla aramada diğer seçimler dikkate alınmaz.</p>

  <section class="panel">
    <div v-if="loading" class="state"><n-spin /></div>

    <div v-else-if="loadError" class="state">
      <n-alert type="error" :show-icon="false">
        {{ loadError }}
        <n-button size="small" class="retry" @click="load()">Tekrar dene</n-button>
      </n-alert>
    </div>

    <div v-else-if="items.length === 0" class="state">
      <n-empty :description="emptyText" />
    </div>

    <template v-else>
      <div class="table-wrap">
        <table class="archive-table">
          <thead>
            <tr>
              <th>Başlık</th>
              <th>Kaynak</th>
              <th>Tarih</th>
              <th>Durum</th>
              <th>AI</th>
              <th><span class="visually-hidden">İşlemler</span></th>
            </tr>
          </thead>
          <tbody>
            <template v-for="item in items" :key="item.articleId">
              <tr>
                <td class="title">
                  <a :href="item.url" target="_blank" rel="noopener">{{ item.title || item.url }}</a>
                  <div v-if="item.cases.length" class="cases">
                    <router-link v-for="c in item.cases" :key="c.id" :to="`/cases/${c.id}`">{{ c.name }}</router-link>
                  </div>
                  <div v-else-if="item.reason" class="reason">{{ item.reason }}</div>
                </td>
                <td>{{ item.sourceName }}</td>
                <td class="nowrap">{{ formatDate(item.publishedAt) }}</td>
                <td class="nowrap">
                  <n-tag size="small" :type="stateLabel[item.state].type" :bordered="false">{{ stateLabel[item.state].text }}</n-tag>
                </td>
                <td class="ai-cell" :title="[item.ai?.reason, item.ai?.version, item.draft ? `Taslak: ${item.draft}` : null].filter(Boolean).join('\n') || undefined">
                  <n-tag v-if="item.ai" size="small" :type="verdictType[item.ai.verdict]" :bordered="false">{{ verdictText[item.ai.verdict] }}</n-tag>
                  <span v-else class="muted-text">—</span>
                  <n-tag v-if="item.draft" size="small" :bordered="false" class="draft-tag">taslak</n-tag>
                </td>
                <td class="actions">
                  <router-link v-if="item.state === 'Open'" :to="`/review/${encodeURIComponent(item.articleId)}`" custom v-slot="{ navigate }">
                    <n-button type="primary" size="small" @click="navigate">İncele</n-button>
                  </router-link>
                  <n-button size="small" :aria-expanded="expanded.has(item.articleId)" @click="toggle(item.articleId)">
                    {{ expanded.has(item.articleId) ? 'Geçmişi gizle' : 'Geçmiş' }}
                  </n-button>
                </td>
              </tr>
              <tr v-if="expanded.has(item.articleId)" class="timeline-row">
                <td colspan="6">
                  <ol class="timeline">
                    <li v-for="(event, i) in item.timeline" :key="i" :class="{ failed: event.status === 'Failed' }">
                      <span class="when">{{ formatDateTime(event.at) }}</span>
                      <span>{{ eventText(event) }}</span>
                      <span v-if="event.reason" class="reason">{{ event.reason }}</span>
                      <span v-if="event.version && (event.step === 'Relevance' || event.step === 'Extracted')" class="reason">{{ event.version }}</span>
                    </li>
                  </ol>
                </td>
              </tr>
            </template>
          </tbody>
        </table>
      </div>
      <div v-if="next" class="more">
        <n-button :loading="loadingMore" @click="load(true)">Daha fazla yükle</n-button>
      </div>
    </template>
  </section>
</template>

<style scoped>
.ai-cell {
  white-space: nowrap;
}
.ai-cell > * + * {
  margin-left: 4px;
}
.draft-tag {
  --n-color: #efeafb;
  --n-text-color: #5c3db0;
}
.muted-text {
  color: var(--rs-muted);
}
.page-head {
  margin-bottom: 16px;
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
.filters {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
  align-items: flex-start;
  margin-bottom: 8px;
}
.search {
  flex: 1 1 320px;
}
.search :deep(.n-form-item-blank) {
  display: flex;
  gap: 8px;
}
.search-button {
  flex: none;
}
.narrow-select {
  min-width: 140px;
}
.wide-select {
  min-width: 220px;
}
.hint {
  margin: 0 0 8px;
  font-size: 13px;
  color: var(--rs-muted);
}
.panel {
  margin-top: 12px;
  background: var(--rs-surface);
  border: 1px solid var(--rs-border);
  border-radius: 10px;
}
.state {
  padding: 40px 16px;
}
.retry {
  margin-left: 12px;
}
.table-wrap {
  overflow-x: auto;
}
.archive-table {
  width: 100%;
  min-width: 760px;
  border-collapse: collapse;
}
.archive-table th {
  text-align: left;
  font-size: 12px;
  letter-spacing: 0.06em;
  text-transform: uppercase;
  color: var(--rs-muted);
  font-weight: 600;
  padding: 12px 16px;
  border-bottom: 1px solid var(--rs-border);
}
.archive-table td {
  padding: 14px 16px;
  border-bottom: 1px solid #eef0f2;
  font-size: 15px;
  vertical-align: middle;
}
.title {
  font-weight: 500;
}
.title a {
  color: var(--rs-ink);
  text-decoration: none;
}
.title a:hover {
  text-decoration: underline;
}
.cases {
  display: flex;
  flex-wrap: wrap;
  gap: 4px 12px;
  margin-top: 4px;
  font-size: 13px;
  font-weight: 400;
}
.cases a {
  color: var(--rs-accent);
}
.reason {
  margin-top: 4px;
  font-size: 13px;
  font-weight: 400;
  color: var(--rs-muted);
}
.nowrap {
  white-space: nowrap;
}
.actions {
  white-space: nowrap;
  text-align: right;
}
.actions > * + * {
  margin-left: 8px;
}
.timeline-row td {
  background: #fafbfb;
  padding-top: 8px;
}
.timeline {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 6px;
  font-size: 14px;
}
.timeline li {
  display: flex;
  flex-wrap: wrap;
  gap: 4px 12px;
  align-items: baseline;
}
.timeline .when {
  min-width: 150px;
  color: var(--rs-muted);
  font-variant-numeric: tabular-nums;
}
.timeline .failed {
  color: #b42318;
}
.timeline .reason {
  margin: 0;
}
.more {
  display: flex;
  justify-content: center;
  padding: 16px;
}
.visually-hidden {
  position: absolute;
  width: 1px;
  height: 1px;
  overflow: hidden;
  clip: rect(0 0 0 0);
}
</style>
