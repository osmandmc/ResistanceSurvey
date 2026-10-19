<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { NAlert, NButton, NDatePicker, NEmpty, NFormItem, NSelect, NSpin, NTabPane, NTabs } from 'naive-ui'
import { ApiError } from '@/api'
import DismissDialog from '@/components/DismissDialog.vue'
import { formatDate } from '@/format'
import { getInbox, type InboxItem } from '@/inboxApi'

const route = useRoute()
const router = useRouter()

// Filters, kept in the address (?month=2026-10&source=evrensel) so the review screen can come back to them.
// The month picker works with a timestamp; the API takes "yyyy-MM".
function initialMonth() {
  const fromQuery = typeof route.query.month === 'string' ? /^(\d{4})-(\d{2})$/.exec(route.query.month) : null
  const now = new Date()
  return fromQuery
    ? new Date(Number(fromQuery[1]), Number(fromQuery[2]) - 1, 1).getTime()
    : new Date(now.getFullYear(), now.getMonth(), 1).getTime()
}
const month = ref<number>(initialMonth())
const source = ref<string>(typeof route.query.source === 'string' ? route.query.source : '')
const sourceOptions = [
  { label: 'Tümü', value: '' },
  { label: 'Evrensel', value: 'evrensel' },
  { label: 'Kızılbayrak', value: 'kizilbayrak' },
]
const monthParam = computed(() => {
  const date = new Date(month.value)
  return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}`
})

// Until AI drafts exist (phase 3), every open article needs a manual review.
const tab = ref<'draft' | 'manual'>('manual')

const items = ref<InboxItem[]>([])
const next = ref<string | null>(null)
const loading = ref(false)
const loadingMore = ref(false)
const loadError = ref('')

async function load(more = false) {
  if (more) {
    loadingMore.value = true
  } else {
    loading.value = true
    items.value = []
    next.value = null
  }
  loadError.value = ''
  try {
    const page = await getInbox(monthParam.value, source.value || null, more ? next.value : null)
    items.value = more ? [...items.value, ...page.items] : page.items
    next.value = page.next
  } catch (error) {
    loadError.value = errorText(error, 'Haberler yüklenemedi.')
  } finally {
    loading.value = false
    loadingMore.value = false
  }
}

const filterQuery = computed(() => ({ month: monthParam.value, ...(source.value ? { source: source.value } : {}) }))

watch(
  [monthParam, source],
  () => {
    void router.replace({ query: filterQuery.value })
    void load()
  },
  { immediate: true },
)

const manualCount = computed(() => `${items.value.length}${next.value ? '+' : ''}`)

// Dismissing happens in DismissDialog; a handled article leaves the list.
const dismissTarget = ref<InboxItem | null>(null)

function onHandled(articleId: string) {
  items.value = items.value.filter((i) => i.articleId !== articleId)
  dismissTarget.value = null
}

function errorText(error: unknown, fallback: string) {
  return error instanceof ApiError && error.status === 502 ? error.message : fallback
}
</script>

<template>
  <div class="page-head">
    <div>
      <h1 class="heading">Gelen Kutusu</h1>
      <p class="description">Karar bekleyen haberler. İşlenen haberler buradan kalkar.</p>
    </div>
    <div class="filters">
      <n-form-item label="Ay" label-placement="top" :show-feedback="false">
        <n-date-picker v-model:value="month" type="month" :clearable="false" format="MMMM yyyy" />
      </n-form-item>
      <n-form-item label="Kaynak" label-placement="top" :show-feedback="false">
        <n-select v-model:value="source" :options="sourceOptions" class="source-select" />
      </n-form-item>
    </div>
  </div>

  <section class="panel">
    <n-tabs v-model:value="tab" type="line" animated class="tabs">
      <n-tab-pane name="draft" tab="Taslak hazır · 0">
        <div class="state">
          <n-empty description="Yapay zekâ taslakları henüz yok; bu özellik bir sonraki aşamada gelecek." />
        </div>
      </n-tab-pane>

      <n-tab-pane name="manual" :tab="`Elle incelenmeli · ${manualCount}`">
        <div v-if="loading" class="state"><n-spin /></div>

        <div v-else-if="loadError" class="state">
          <n-alert type="error" :show-icon="false">
            {{ loadError }}
            <n-button size="small" class="retry" @click="load()">Tekrar dene</n-button>
          </n-alert>
        </div>

        <div v-else-if="items.length === 0" class="state">
          <n-empty description="Bu ay için karar bekleyen haber yok." />
        </div>

        <template v-else>
          <div class="table-wrap">
            <table class="inbox-table">
              <thead>
                <tr>
                  <th>Başlık</th>
                  <th>Kaynak</th>
                  <th>Tarih</th>
                  <th><span class="visually-hidden">İşlemler</span></th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="item in items" :key="item.articleId">
                  <td class="title">
                    <a :href="item.url" target="_blank" rel="noopener">{{ item.title || item.url }}</a>
                  </td>
                  <td>{{ item.sourceName }}</td>
                  <td class="nowrap">{{ formatDate(item.publishedAt) }}</td>
                  <td class="actions">
                    <router-link :to="{ path: `/review/${encodeURIComponent(item.articleId)}`, query: filterQuery }" custom v-slot="{ navigate }">
                      <n-button type="primary" size="small" @click="navigate">Aç</n-button>
                    </router-link>
                    <n-button size="small" @click="dismissTarget = item">İlgisiz</n-button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
          <div v-if="next" class="more">
            <n-button :loading="loadingMore" @click="load(true)">Daha fazla yükle</n-button>
          </div>
        </template>
      </n-tab-pane>
    </n-tabs>
  </section>

  <dismiss-dialog :article="dismissTarget" @handled="onHandled" @close="dismissTarget = null" />
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
.filters {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
}
.source-select {
  min-width: 160px;
}
.panel {
  background: var(--rs-surface);
  border: 1px solid var(--rs-border);
  border-radius: 10px;
}
.tabs :deep(.n-tabs-nav) {
  padding: 0 16px;
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
.inbox-table {
  width: 100%;
  min-width: 720px;
  border-collapse: collapse;
}
.inbox-table th {
  text-align: left;
  font-size: 12px;
  letter-spacing: 0.06em;
  text-transform: uppercase;
  color: var(--rs-muted);
  font-weight: 600;
  padding: 12px 16px;
  border-bottom: 1px solid var(--rs-border);
}
.inbox-table td {
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
