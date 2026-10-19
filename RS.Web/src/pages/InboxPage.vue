<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import {
  NAlert,
  NButton,
  NDatePicker,
  NEmpty,
  NFormItem,
  NInput,
  NModal,
  NSelect,
  NSpin,
  NTabPane,
  NTabs,
  useMessage,
} from 'naive-ui'
import { ApiError } from '@/api'
import { dismissArticle, getInbox, type InboxItem } from '@/inboxApi'

const message = useMessage()

// Filters. The month picker works with a timestamp; the API takes "yyyy-MM".
const now = new Date()
const month = ref<number>(new Date(now.getFullYear(), now.getMonth(), 1).getTime())
const source = ref<string>('')
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

watch([monthParam, source], () => load(), { immediate: true })

const manualCount = computed(() => `${items.value.length}${next.value ? '+' : ''}`)

const dateFormat = new Intl.DateTimeFormat('tr-TR', {
  day: 'numeric',
  month: 'short',
  year: 'numeric',
  timeZone: 'Europe/Istanbul',
})
function formatDate(value: string | null) {
  return value ? dateFormat.format(new Date(value)) : '—'
}

// Dismissing: a small dialog with an optional reason.
const dismissTarget = ref<InboxItem | null>(null)
const reason = ref('')
const dismissing = ref(false)

function askDismiss(item: InboxItem) {
  dismissTarget.value = item
  reason.value = ''
}

function removeItem(articleId: string) {
  items.value = items.value.filter((i) => i.articleId !== articleId)
}

async function confirmDismiss() {
  const item = dismissTarget.value
  if (!item) return
  dismissing.value = true
  try {
    const result = await dismissArticle(item.articleId, reason.value)
    removeItem(item.articleId)
    if (result.outcomeSent) {
      message.success('İlgisiz olarak işaretlendi.')
    } else {
      message.warning('İlgisiz olarak işaretlendi; işleme hattına daha sonra bildirilecek.')
    }
    dismissTarget.value = null
  } catch (error) {
    if (error instanceof ApiError && (error.status === 409 || error.status === 404)) {
      removeItem(item.articleId)
      message.warning(error.message)
      dismissTarget.value = null
    } else {
      message.error(errorText(error, 'İşaretlenemedi.'))
    }
  } finally {
    dismissing.value = false
  }
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
                  <td class="title">{{ item.title || item.url }}</td>
                  <td>{{ item.sourceName }}</td>
                  <td class="nowrap">{{ formatDate(item.publishedAt) }}</td>
                  <td class="actions">
                    <!-- Until the review screen exists (R6), "Aç" opens the original article. -->
                    <n-button tag="a" :href="item.url" target="_blank" rel="noopener" type="primary" size="small">
                      Aç
                    </n-button>
                    <n-button size="small" @click="askDismiss(item)">İlgisiz</n-button>
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

  <n-modal
    :show="dismissTarget !== null"
    preset="card"
    title="İlgisiz olarak işaretle"
    :style="{ width: 'min(520px, calc(100vw - 32px))' }"
    :mask-closable="!dismissing"
    @update:show="(show: boolean) => { if (!show && !dismissing) dismissTarget = null }"
  >
    <p class="dismiss-title">{{ dismissTarget?.title }}</p>
    <n-form-item label="Neden (isteğe bağlı)" label-placement="top" :show-feedback="false">
      <n-input
        v-model:value="reason"
        type="textarea"
        :autosize="{ minRows: 2, maxRows: 5 }"
        maxlength="500"
        placeholder="Örn. işçi eylemi değil, görüş yazısı"
      />
    </n-form-item>
    <template #footer>
      <div class="dialog-actions">
        <n-button :disabled="dismissing" @click="dismissTarget = null">Vazgeç</n-button>
        <n-button type="primary" :loading="dismissing" @click="confirmDismiss">İlgisiz olarak işaretle</n-button>
      </div>
    </template>
  </n-modal>
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
.dismiss-title {
  margin: 0 0 16px;
  font-weight: 500;
}
.dialog-actions {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
}
</style>
