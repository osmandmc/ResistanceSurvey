<script setup lang="ts">
import { computed, nextTick, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { NAlert, NButton, NEmpty, NFormItem, NInput, NModal, NResult, NSelect, NSpin, useDialog, useMessage } from 'naive-ui'
import { ApiError, errorMessage } from '@/api'
import { fromCaseInput, toCaseInput, type CaseFormValue } from '@/caseForm'
import {
  attachArticle,
  deleteCase,
  deleteProtest,
  detachArticle,
  getCase,
  saveProtest,
  searchNews,
  updateCase,
  type CaseDetail,
  type NewsOption,
  type ProtestDetail,
} from '@/casesApi'
import CaseForm from '@/components/CaseForm.vue'
import CaseHistory from '@/components/CaseHistory.vue'
import CaseSolidarityPanel from '@/components/CaseSolidarityPanel.vue'
import ProtestForm from '@/components/ProtestForm.vue'
import { formatDate } from '@/format'
import { emptyProtest, fromProtestInput, toProtestInput, type ProtestFormValue } from '@/protestForm'
import { getFormOptions, type CaseFormOptions } from '@/reviewApi'

const route = useRoute()
const router = useRouter()
const message = useMessage()
const dialog = useDialog()

const caseId = computed(() => Number(route.params.id))

const detail = ref<CaseDetail | null>(null)
const options = ref<CaseFormOptions | null>(null)
const loading = ref(false)
const loadError = ref<'notFound' | 'failed' | ''>('')

const caseValue = ref<CaseFormValue | null>(null)
const caseErrors = ref<Record<string, string>>({})
const savingCase = ref(false)

async function load() {
  loading.value = true
  loadError.value = ''
  try {
    const [d, o] = await Promise.all([getCase(caseId.value), options.value ? Promise.resolve(options.value) : getFormOptions()])
    detail.value = d
    options.value = o
    caseValue.value = fromCaseInput(d.case)
  } catch (error) {
    loadError.value = error instanceof ApiError && error.status === 404 ? 'notFound' : 'failed'
  } finally {
    loading.value = false
  }
  // From the case list's protest line (#protest-12): show that protest.
  if (route.hash) {
    await nextTick()
    document.getElementById(route.hash.slice(1))?.scrollIntoView({ block: 'center' })
  }
}

/** Refresh the case after a change, keeping the case form's unsaved edits. */
async function reload() {
  try {
    detail.value = await getCase(caseId.value)
  } catch {
    // the page keeps what it shows
  }
}

watch(caseId, () => load(), { immediate: true })

async function showFirstError() {
  await nextTick()
  document.querySelector('.n-form-item-feedback--error')?.scrollIntoView({ behavior: 'smooth', block: 'center' })
}

function errorText(error: unknown, fallback: string) {
  return errorMessage(error, fallback)
}

// The case part.
async function saveCase() {
  if (!caseValue.value) return
  savingCase.value = true
  caseErrors.value = {}
  try {
    await updateCase(caseId.value, toCaseInput(caseValue.value))
    message.success('Vaka kaydedildi.')
    await reload()
  } catch (error) {
    if (error instanceof ApiError && error.status === 400) {
      caseErrors.value = error.fields
      await showFirstError()
    }
    message.error(errorText(error, 'Vaka kaydedilemedi.'))
  } finally {
    savingCase.value = false
  }
}

function confirmDeleteCase() {
  dialog.warning({
    title: 'Vakayı sil',
    content: 'Vaka, eylemleriyle birlikte silinecek. Emin misiniz?',
    positiveText: 'Sil',
    negativeText: 'Vazgeç',
    onPositiveClick: async () => {
      try {
        await deleteCase(caseId.value)
        message.success('Vaka silindi.')
        await router.push('/cases')
      } catch (error) {
        message.error(errorText(error, 'Vaka silinemedi.'))
      }
    },
  })
}

// Protests: one is edited at a time ("new" for a protest being added).
const editingProtest = ref<number | 'new' | null>(null)
const protestValue = ref<ProtestFormValue>(emptyProtest())
const protestErrors = ref<Record<string, string>>({})
const savingProtest = ref(false)

function editProtest(protest: ProtestDetail) {
  protestValue.value = fromProtestInput(protest.protest)
  protestErrors.value = {}
  editingProtest.value = protest.id
}

function addProtest() {
  protestValue.value = emptyProtest()
  protestErrors.value = {}
  editingProtest.value = 'new'
}

async function submitProtest() {
  if (editingProtest.value === null) return
  savingProtest.value = true
  protestErrors.value = {}
  try {
    await saveProtest(caseId.value, editingProtest.value === 'new' ? null : editingProtest.value, toProtestInput(protestValue.value))
    message.success(editingProtest.value === 'new' ? 'Eylem eklendi.' : 'Eylem kaydedildi.')
    editingProtest.value = null
    await reload()
  } catch (error) {
    if (error instanceof ApiError && error.status === 400) {
      protestErrors.value = error.fields
      await showFirstError()
    }
    message.error(errorText(error, 'Eylem kaydedilemedi.'))
  } finally {
    savingProtest.value = false
  }
}

function confirmDeleteProtest(protest: ProtestDetail) {
  dialog.warning({
    title: 'Eylemi sil',
    content: `${formatDate(protest.protest.startDate)} tarihli eylem silinecek. Emin misiniz?`,
    positiveText: 'Sil',
    negativeText: 'Vazgeç',
    onPositiveClick: async () => {
      try {
        await deleteProtest(caseId.value, protest.id)
        message.success('Eylem silindi.')
        await reload()
      } catch (error) {
        message.error(errorText(error, 'Eylem silinemedi.'))
      }
    },
  })
}

function protestDates(p: ProtestDetail) {
  const start = formatDate(p.protest.startDate)
  return p.protest.endDate ? `${start} – ${formatDate(p.protest.endDate)}` : `${start} (bitiş yok)`
}

// Articles.
const addingArticle = ref(false)
const newsQuery = ref('')
const newsYear = ref<number | null>(null)
const newsMonth = ref<number | null>(null)
const newsResults = ref<NewsOption[]>([])
const searchingNews = ref(false)
let newsTimer: ReturnType<typeof setTimeout> | undefined
const yearOptions = Array.from({ length: new Date().getFullYear() - 2018 }, (_, i) => {
  const y = new Date().getFullYear() - i
  return { label: String(y), value: y }
})
const monthOptions = ['Ocak', 'Şubat', 'Mart', 'Nisan', 'Mayıs', 'Haziran', 'Temmuz', 'Ağustos', 'Eylül', 'Ekim', 'Kasım', 'Aralık'].map(
  (label, i) => ({ label, value: i + 1 }),
)
const linkedIds = computed(() => new Set(detail.value?.articles.map((a) => a.newsId) ?? []))

watch([addingArticle, newsQuery, newsYear, newsMonth], ([open]) => {
  if (!open) return
  clearTimeout(newsTimer)
  newsTimer = setTimeout(async () => {
    searchingNews.value = true
    try {
      newsResults.value = await searchNews(newsQuery.value.trim(), newsYear.value, newsMonth.value)
    } catch {
      newsResults.value = []
    } finally {
      searchingNews.value = false
    }
  }, 300)
})

async function attach(news: NewsOption) {
  try {
    await attachArticle(caseId.value, news.id)
    message.success('Haber vakaya eklendi.')
    await reload()
  } catch (error) {
    message.error(errorText(error, 'Haber eklenemedi.'))
  }
}

function confirmDetach(newsId: number, title: string | null) {
  dialog.warning({
    title: 'Haberi vakadan çıkar',
    content: `“${title ?? 'Haber'}” bu vakadan çıkarılacak. Haberin kendisi silinmez.`,
    positiveText: 'Çıkar',
    negativeText: 'Vazgeç',
    onPositiveClick: async () => {
      try {
        await detachArticle(caseId.value, newsId)
        message.success('Haber vakadan çıkarıldı.')
        await reload()
      } catch (error) {
        message.error(errorText(error, 'Haber çıkarılamadı.'))
      }
    },
  })
}
</script>

<template>
  <router-link to="/cases" class="back">← Vakalar</router-link>

  <div v-if="loading" class="state"><n-spin /></div>

  <n-result v-else-if="loadError === 'notFound'" status="404" title="Vaka bulunamadı" description="Silinmiş olabilir.">
    <template #footer><router-link to="/cases">Vakalar’a dön</router-link></template>
  </n-result>

  <n-alert v-else-if="loadError === 'failed'" type="error" :show-icon="false">
    Vaka yüklenemedi.
    <n-button size="small" class="retry" @click="load">Tekrar dene</n-button>
  </n-alert>

  <template v-else-if="detail && options && caseValue">
    <div class="page-head">
      <div>
        <h1 class="heading">{{ detail.case.name || `Vaka #${detail.id}` }}</h1>
        <p class="meta">
          Vaka #{{ detail.id }}
          <template v-if="detail.creator"> · {{ detail.creator }} oluşturdu {{ formatDate(detail.createDate) }}</template>
          <template v-if="detail.updater"> · son değişiklik {{ detail.updater }}, {{ formatDate(detail.updateDate) }}</template>
        </p>
      </div>
      <n-button type="error" ghost @click="confirmDeleteCase">Vakayı sil</n-button>
    </div>

    <section class="panel">
      <case-form
        v-model="caseValue"
        :options="options"
        :errors="caseErrors"
        :company-name="detail.companyName"
        :main-company-name="detail.mainCompanyName"
        :supported-case-name="detail.supportedCase?.name"
        :workplace-name="detail.workplaceName"
        :warn-about-existing-cases="false"
      />
      <div class="actions">
        <n-button type="primary" :loading="savingCase" @click="saveCase">Vakayı kaydet</n-button>
      </div>
    </section>

    <section class="panel" aria-labelledby="protests-heading">
      <div class="section-head">
        <h2 id="protests-heading" class="section-title">Eylemler · {{ detail.protests.length }}</h2>
        <n-button :disabled="editingProtest !== null" @click="addProtest">Eylem Ekle</n-button>
      </div>

      <div v-if="editingProtest === 'new'" class="protest-edit">
        <protest-form v-model="protestValue" :options="options" :errors="protestErrors" />
        <div class="actions">
          <n-button type="primary" :loading="savingProtest" @click="submitProtest">Eylemi ekle</n-button>
          <n-button quaternary :disabled="savingProtest" @click="editingProtest = null">Vazgeç</n-button>
        </div>
      </div>

      <n-empty v-if="detail.protests.length === 0 && editingProtest !== 'new'" description="Bu vakanın eylemi yok." class="state" />

      <ul class="protests">
        <li v-for="p in detail.protests" :id="`protest-${p.id}`" :key="p.id" class="protest">
          <template v-if="editingProtest === p.id">
            <protest-form v-model="protestValue" :options="options" :errors="protestErrors" />
            <div class="actions">
              <n-button type="primary" :loading="savingProtest" @click="submitProtest">Eylemi kaydet</n-button>
              <n-button quaternary :disabled="savingProtest" @click="editingProtest = null">Vazgeç</n-button>
            </div>
          </template>
          <template v-else>
            <div class="protest-summary">
              <strong>{{ protestDates(p) }}</strong>
              <span class="sub">{{ p.typeNames.join(', ') || 'Eylem türü yok' }}<template v-if="p.cityNames.length"> · {{ p.cityNames.join(', ') }}</template></span>
            </div>
            <div class="row-actions">
              <n-button size="small" :disabled="editingProtest !== null" @click="editProtest(p)">Düzenle</n-button>
              <n-button size="small" quaternary :disabled="editingProtest !== null" @click="confirmDeleteProtest(p)">Sil</n-button>
            </div>
          </template>
        </li>
      </ul>
    </section>

    <section class="panel" aria-labelledby="articles-heading">
      <div class="section-head">
        <h2 id="articles-heading" class="section-title">Haberler · {{ detail.articles.length }}</h2>
        <n-button @click="addingArticle = true">Haber ekle</n-button>
      </div>
      <n-empty v-if="detail.articles.length === 0" description="Bu vakaya bağlı haber yok." class="state" />
      <ul class="articles">
        <li v-for="a in detail.articles" :key="a.newsId" class="article">
          <div>
            <a :href="a.link" target="_blank" rel="noopener">{{ a.title || a.link }}</a>
            <span class="sub">{{ formatDate(a.date) }}</span>
          </div>
          <n-button size="small" quaternary @click="confirmDetach(a.newsId, a.title)">Çıkar</n-button>
        </li>
      </ul>
    </section>

    <case-solidarity-panel
      :case-id="caseId"
      :category-id="detail.case.categoryId"
      :supported-case="detail.supportedCase"
      :version="detail"
      @changed="reload"
    />

    <case-history :case-id="caseId" :version="detail" />

    <n-modal v-model:show="addingArticle" preset="card" title="Haber ekle" :style="{ width: 'min(680px, calc(100vw - 32px))' }">
      <p class="sub">Uygulamadaki haberler. İşleme hattından gelen ve henüz işlenmemiş haberler Gelen Kutusu’ndan bağlanır.</p>
      <div class="news-filters">
        <n-form-item label="Ara" label-placement="top" :show-feedback="false">
          <n-input v-model:value="newsQuery" clearable placeholder="Başlık veya bağlantı" />
        </n-form-item>
        <n-form-item label="Yıl" label-placement="top" :show-feedback="false">
          <n-select v-model:value="newsYear" :options="yearOptions" clearable placeholder="Tümü" />
        </n-form-item>
        <n-form-item label="Ay" label-placement="top" :show-feedback="false">
          <n-select v-model:value="newsMonth" :options="monthOptions" clearable placeholder="Tümü" :disabled="!newsYear" />
        </n-form-item>
      </div>
      <div v-if="searchingNews" class="state"><n-spin size="small" /></div>
      <n-empty v-else-if="newsResults.length === 0" description="Haber bulunamadı." class="state" />
      <ul v-else class="articles">
        <li v-for="n in newsResults" :key="n.id" class="article">
          <div>
            <a :href="n.link" target="_blank" rel="noopener">{{ n.title || n.link }}</a>
            <span class="sub">{{ formatDate(n.date) }}<template v-if="n.linked"> · başka bir vakaya bağlı</template></span>
          </div>
          <n-button size="small" :disabled="linkedIds.has(n.id)" @click="attach(n)">{{ linkedIds.has(n.id) ? 'Ekli' : 'Ekle' }}</n-button>
        </li>
      </ul>
    </n-modal>
  </template>
</template>

<style scoped>
.back {
  display: inline-block;
  margin-bottom: 16px;
  font-weight: 500;
}
.state {
  padding: 32px 16px;
}
.retry {
  margin-left: 12px;
}
.page-head {
  display: flex;
  flex-wrap: wrap;
  align-items: flex-start;
  justify-content: space-between;
  gap: 16px;
  margin-bottom: 20px;
}
.heading {
  margin: 0;
  font-size: 28px;
}
.meta {
  margin: 6px 0 0;
  color: var(--rs-muted);
  font-size: 14px;
}
.panel {
  background: var(--rs-surface);
  border: 1px solid var(--rs-border);
  border-radius: 10px;
  padding: 20px 24px;
  margin-bottom: 20px;
}
.actions {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
  padding-top: 12px;
}
.section-head {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 12px;
}
.section-title {
  margin: 0;
  font-size: 18px;
}
.protest-edit {
  border: 1px solid var(--rs-accent);
  border-radius: 8px;
  padding: 12px 16px;
  margin-bottom: 12px;
}
.protests,
.articles {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 8px;
}
.protest,
.article {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
  padding: 10px 12px;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
}
.protest > :deep(.section) {
  width: 100%;
}
.protest-summary {
  display: flex;
  flex-direction: column;
  gap: 2px;
}
.sub {
  display: block;
  font-size: 13px;
  color: var(--rs-muted);
}
.row-actions {
  display: flex;
  gap: 6px;
}
.news-filters {
  display: grid;
  grid-template-columns: 2fr 1fr 1fr;
  gap: 8px;
  margin-bottom: 8px;
}
@media (max-width: 600px) {
  .news-filters {
    grid-template-columns: 1fr;
  }
  .panel {
    padding: 16px;
  }
}
</style>
