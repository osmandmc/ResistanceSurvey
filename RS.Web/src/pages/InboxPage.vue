<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { NAlert, NButton, NDatePicker, NEmpty, NFormItem, NSelect, NSpin, NTabPane, NTabs } from 'naive-ui'
import { errorMessage } from '@/api'
import DismissDialog from '@/components/DismissDialog.vue'
import InboxTable from '@/components/InboxTable.vue'
import { getInbox, type InboxItem, type InboxPart } from '@/inboxApi'

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

// Three parts, by what the pipeline's AI did: articles with a draft to check (R11), the rest to review, and what it
// judged not relevant, in its own tab and never hidden (R21).
const parts: InboxPart[] = ['draft', 'other', 'notRelevant']
const tab = ref<InboxPart>(parts.includes(route.query.part as InboxPart) ? (route.query.part as InboxPart) : 'draft')

/** One part of the inbox, loaded page by page. */
function inboxPart(part: InboxPart) {
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
      const page = await getInbox(monthParam.value, source.value || null, more ? next.value : null, part)
      items.value = more ? [...items.value, ...page.items] : page.items
      next.value = page.next
    } catch (error) {
      loadError.value = errorText(error, 'Haberler yüklenemedi.')
    } finally {
      loading.value = false
      loadingMore.value = false
    }
  }

  const count = computed(() => `${items.value.length}${next.value ? '+' : ''}`)
  return reactive({ items, next, loading, loadingMore, loadError, load, count })
}

const drafts = inboxPart('draft')
const toReview = inboxPart('other')
const notRelevant = inboxPart('notRelevant')

const filterQuery = computed(() => ({ month: monthParam.value, ...(source.value ? { source: source.value } : {}) }))

watch(
  [monthParam, source],
  () => {
    void router.replace({ query: filterQuery.value })
    void drafts.load()
    void toReview.load()
    void notRelevant.load()
  },
  { immediate: true },
)

// Dismissing happens in DismissDialog; a handled article leaves the list. From the AI's list the reason "Emek
// hareketiyle ilgisiz" is chosen in advance, with the AI's reason as the details; the researcher can change both.
const dismissTarget = ref<InboxItem | null>(null)
const dismissFromAiList = ref(false)

function dismiss(item: InboxItem, fromAiList: boolean) {
  dismissFromAiList.value = fromAiList
  dismissTarget.value = item
}

function onHandled(articleId: string) {
  drafts.items = drafts.items.filter((i) => i.articleId !== articleId)
  toReview.items = toReview.items.filter((i) => i.articleId !== articleId)
  notRelevant.items = notRelevant.items.filter((i) => i.articleId !== articleId)
  dismissTarget.value = null
}

function errorText(error: unknown, fallback: string) {
  return errorMessage(error, fallback)
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
      <!-- display-directive="show" keeps each pane mounted, so tab switches never patch one pane's components with the other's static props. -->
      <n-tab-pane
        v-for="pane in [
          { name: 'draft', part: drafts, label: 'Taslak hazır', empty: 'AI taslağı hazır, karar bekleyen haber yok.' },
          { name: 'other', part: toReview, label: 'İncelenecek', empty: 'Bu ay için karar bekleyen haber yok.' },
          { name: 'notRelevant', part: notRelevant, label: 'AI: ilgisiz görünüyor', empty: 'AI’ın ilgisiz bulduğu, karar bekleyen haber yok.' },
        ]"
        :key="pane.name"
        :name="pane.name"
        :tab="`${pane.label} · ${pane.part.count}`"
        display-directive="show"
      >
        <p v-if="pane.name === 'draft'" class="hint">
          Yapay zekânın formu önceden doldurduğu haberler. Açınca taslağı kontrol edip forma uygulayabilirsiniz; hiçbir şey onayınız olmadan kaydedilmez.
        </p>
        <p v-if="pane.name === 'notRelevant'" class="hint">
          Yapay zekânın emek direnişiyle ilgisiz bulduğu haberler. Göz atıp “İlgisiz” ile kapatabilir ya da açıp inceleyebilirsiniz.
        </p>

        <div v-if="pane.part.loading" class="state"><n-spin /></div>

        <div v-else-if="pane.part.loadError" class="state">
          <n-alert type="error" :show-icon="false">
            {{ pane.part.loadError }}
            <n-button size="small" class="retry" @click="pane.part.load()">Tekrar dene</n-button>
          </n-alert>
        </div>

        <div v-else-if="pane.part.items.length === 0" class="state">
          <n-empty :description="pane.empty" />
        </div>

        <template v-else>
          <inbox-table :items="pane.part.items" :query="{ ...filterQuery, part: pane.name }" @dismiss="(item) => dismiss(item, pane.name === 'notRelevant')" />
          <div v-if="pane.part.next" class="more">
            <n-button :loading="pane.part.loadingMore" @click="pane.part.load(true)">Daha fazla yükle</n-button>
          </div>
        </template>
      </n-tab-pane>
    </n-tabs>
  </section>

  <dismiss-dialog
    :article="dismissTarget"
    :preset-kind="dismissFromAiList ? 1 : null"
    :suggested-reason="dismissFromAiList ? dismissTarget?.ai?.reason : null"
    @handled="onHandled"
    @close="dismissTarget = null"
  />
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
.hint {
  margin: 12px 16px 0;
  font-size: 14px;
  color: var(--rs-muted);
}
.retry {
  margin-left: 12px;
}
.more {
  display: flex;
  justify-content: center;
  padding: 16px;
}
</style>
