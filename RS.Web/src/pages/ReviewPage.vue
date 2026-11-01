<script setup lang="ts">
import { computed, nextTick, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { NAlert, NButton, NRadio, NRadioGroup, NResult, NSpin, NTag, useMessage } from 'naive-ui'
import { ApiError, errorMessage } from '@/api'
import { aiCaseValues, aiProtestValues, getAiDraft, matchAiCases, unchangedFields, type AiCaseMatch, type AiCaseMatches, type AiCheckResult, type AiDraft, type AiFlag, type AiUse } from '@/aiFillApi'
import { emptyCase, toCaseInput, type CaseFormValue } from '@/caseForm'
import AiFillDialog from '@/components/AiFillDialog.vue'
import CaseFinder from '@/components/CaseFinder.vue'
import CaseForm from '@/components/CaseForm.vue'
import ProtestForm from '@/components/ProtestForm.vue'
import { emptyProtest, toProtestInput, type ProtestFormValue } from '@/protestForm'
import DismissDialog from '@/components/DismissDialog.vue'
import { formatDate } from '@/format'
import { verdictText } from '@/relevance'
import { evidenceFormField, findQuote, highlight, type AiEvidence } from '@/evidence'
import { getInbox } from '@/inboxApi'
import {
  getFormOptions,
  getReviewArticle,
  getSuggestions,
  saveReview,
  type CaseFormOptions,
  type CaseOption,
  type CaseSuggestion,
  type ReviewAction,
  type ReviewArticle,
} from '@/reviewApi'

const route = useRoute()
const router = useRouter()
const message = useMessage()

const articleId = computed(() => String(route.params.articleId))

// The article shown on the left.
const article = ref<ReviewArticle | null>(null)
const imageFailed = ref(false)
const loading = ref(false)
const loadError = ref<'handled' | 'notFound' | 'unavailable' | ''>('')
const loadErrorText = ref('')

async function loadArticle() {
  loading.value = true
  loadError.value = ''
  article.value = null
  imageFailed.value = false
  try {
    article.value = await getReviewArticle(articleId.value)
  } catch (error) {
    if (error instanceof ApiError && error.status === 409) loadError.value = 'handled'
    else if (error instanceof ApiError && error.status === 404) loadError.value = 'notFound'
    else {
      loadError.value = 'unavailable'
      loadErrorText.value = errorMessage(error, 'Haber yüklenemedi.')
    }
  } finally {
    loading.value = false
  }
}

watch(articleId, () => loadArticle(), { immediate: true })

// The form's lists, loaded once with the page.
const formOptions = ref<CaseFormOptions | null>(null)
const formOptionsError = ref(false)

async function loadFormOptions() {
  formOptionsError.value = false
  try {
    formOptions.value = await getFormOptions()
  } catch {
    formOptionsError.value = true
  }
}

void loadFormOptions()

const caseValue = ref<CaseFormValue>(emptyCase())
const fieldErrors = ref<Record<string, string>>({})
const protestValue = ref<ProtestFormValue>(emptyProtest())

// The decision on the right.
const action = ref<ReviewAction>('newCase')
const actionOptions: { value: ReviewAction; label: string }[] = [
  { value: 'newCase', label: 'Yeni direniş + eylem oluştur' },
  { value: 'addProtest', label: 'Mevcut direnişe eylem ekle' },
  { value: 'linkOnly', label: 'Sadece haberi bağla' },
]
const needsCase = computed(() => action.value !== 'newCase')
const needsProtest = computed(() => action.value !== 'linkOnly')

// The existing case, for "add protest" and "link only" (CaseFinder). Its company filter can be set from the
// case form's duplicate warning.
const resistanceId = ref<number | null>(null)
const finderCompanyId = ref<number | null>(null)
const finderCompanyName = ref<string | null>(null)

function onUseExisting(existing: CaseOption) {
  action.value = 'addProtest'
  finderCompanyId.value = caseValue.value.companyId
  finderCompanyName.value = existing.companyName
  resistanceId.value = existing.id
  void nextTick(() => document.getElementById('decision-heading')?.scrollIntoView({ behavior: 'smooth', block: 'start' }))
}

// "Olası vakalar": cases of companies named in the article. Only a hint: failures show nothing.
const suggestions = ref<CaseSuggestion[]>([])

async function loadSuggestions() {
  suggestions.value = []
  try {
    suggestions.value = await getSuggestions(articleId.value)
  } catch {
    // the researcher can still search
  }
}

watch(articleId, () => loadSuggestions(), { immediate: true })

function useSuggestion(suggestion: CaseSuggestion, next: 'addProtest' | 'linkOnly') {
  action.value = next
  finderCompanyId.value = suggestion.matchedCompanyId
  finderCompanyName.value = suggestion.matchedCompany
  resistanceId.value = suggestion.case.id
}

// "AI ile doldur" (R19): what the answer put into the form, so those fields stay marked until changed.
const aiDialogFor = ref<string | null>(null)
/** The pipeline's draft for this article (R11); null when it has none. Applied to the form as soon as it loads. */
const aiDraft = ref<AiDraft | null>(null)
/** The applied answer's flags (R11, D-008): values to check, each with its reason. */
const aiFlags = ref<AiFlag[]>([])
const aiApplied = ref<{ case: Partial<CaseFormValue>; protest: Partial<ProtestFormValue> } | null>(null)
const aiCompanyName = ref<string | null>(null)
/** Names of the registered companies the answer chose: the company pickers show them instead of a bare id. */
const aiChosenCompanyName = ref<string | null>(null)
const aiChosenMainCompanyName = ref<string | null>(null)
const aiMainCompanyName = ref<string | null>(null)
/** R37: the registered workplace the AI chose (shown by name) and a workplace it named that is not registered. */
const aiChosenWorkplaceName = ref<string | null>(null)
const aiWorkplaceName = ref<string | null>(null)
/** The AI's reason when it found the article not relevant; the banner offers dismissing it. */
const aiNotRelevant = ref<string | null>(null)
/** The last answer checked for this article; sent with the save or the dismissal (R19c). */
const aiUse = ref<AiUse | null>(null)
const aiCaseFields = computed(() => unchangedFields(caseValue.value, aiApplied.value?.case ?? null))
const aiProtestFields = computed(() => unchangedFields(protestValue.value, aiApplied.value?.protest ?? null))

// R20: with an AI answer (a draft or a pasted one), the existing cases it points to replace the name-based suggestions.
const aiMatches = ref<AiCaseMatches | null>(null)
const aiMatchesShown = computed(() => !!aiMatches.value && aiMatches.value.likely.length + aiMatches.value.older.length > 0)
watch(
  () => aiUse.value?.answer,
  async (answer) => {
    aiMatches.value = null
    if (!answer) return
    const id = articleId.value
    try {
      const matches = await matchAiCases(id, answer)
      if (id === articleId.value && aiUse.value?.answer === answer) aiMatches.value = matches
    } catch {
      // only a hint: the name-based suggestions and the case search remain
    }
  },
)

function useAiMatch(match: AiCaseMatch, next: 'addProtest' | 'linkOnly') {
  action.value = next
  finderCompanyId.value = match.companyId
  finderCompanyName.value = match.case.companyName
  resistanceId.value = match.case.id
}

// Evidence (R27): the AI's quotes for the fields it filled, highlighted in the article while those fields still hold
// its values; the "Kanıtlar" row jumps to each one.
const aiEvidence = ref<AiEvidence[]>([])
const activeEvidence = ref<string | null>(null)
const evidenceShown = computed(() =>
  aiEvidence.value.filter((e) => {
    const f = evidenceFormField[e.field]
    return f && (f.part === 'case' ? aiCaseFields.value : aiProtestFields.value).includes(f.key)
  }),
)
const titleSegments = computed(() => highlight(article.value?.title ?? '', evidenceShown.value))
const bodySegments = computed(() => highlight(article.value?.body ?? '', evidenceShown.value))
const evidenceList = computed(() =>
  evidenceShown.value.map((e) => ({
    ...e,
    found: !!findQuote(article.value?.title ?? '', e.quote) || !!findQuote(article.value?.body ?? '', e.quote),
  })),
)

function showEvidence(field: string) {
  activeEvidence.value = field
  document.querySelector(`[data-evidence~="${field}"]`)?.scrollIntoView({ behavior: 'smooth', block: 'center' })
}

/** auto: the pipeline's draft, applied when the article opens (no message). */
function onAiApply(result: AiCheckResult, auto = false) {
  aiEvidence.value = result.evidence ?? []
  aiFlags.value = result.flags ?? []
  activeEvidence.value = null
  const caseValues = result.case ? aiCaseValues(result.case) : {}
  const protestValues = result.protest ? aiProtestValues(result.protest) : {}
  caseValue.value = { ...caseValue.value, ...structuredClone(caseValues) }
  protestValue.value = { ...protestValue.value, ...structuredClone(protestValues) }
  aiApplied.value = { case: caseValues, protest: protestValues }
  if (aiUse.value) aiUse.value = { ...aiUse.value, applied: true }
  aiCompanyName.value = result.case?.newCompanyName ?? null
  aiChosenCompanyName.value = result.case?.companyName ?? null
  aiChosenMainCompanyName.value = result.case?.mainCompanyName ?? null
  aiMainCompanyName.value = result.case?.newMainCompanyName ?? null
  aiChosenWorkplaceName.value = result.case?.workplaceName ?? null
  aiWorkplaceName.value = result.case?.newWorkplaceName ?? null
  aiNotRelevant.value = null
  aiDialogFor.value = null
  if (action.value === 'linkOnly') action.value = 'newCase'
  if (!auto) message.success('Form dolduruldu. “AI” işaretli alanları kontrol edin; kaydetmeden önce her şeyi değiştirebilirsiniz.')
}

function onAiNotRelevant(reason: string | null) {
  aiDialogFor.value = null
  aiNotRelevant.value = reason ?? 'Gerekçe yazılmadı.'
}

function openAiDialog() {
  aiDialogFor.value = articleId.value
}

/**
 * What to check before saving (R11): the flags of the applied answer for fields that still hold its value, and the
 * quotes not found in the article. The check table's other values (matched, corrected) need no attention.
 */
const toCheck = computed(() => {
  const formField = (field: string) => evidenceFormField[field]
  const stillAi = (field: string) => {
    const f = formField(field)
    return !f || (f.part === 'case' ? aiCaseFields.value : aiProtestFields.value).includes(f.key)
  }
  return [
    ...aiFlags.value.filter((f) => stillAi(f.field)).map((f) => ({ key: `${f.field}:${f.message}`, label: f.label, text: f.message })),
    ...evidenceList.value
      .filter((e) => !e.found)
      .map((e) => ({ key: `quote:${e.field}`, label: e.label, text: 'AI’ın gösterdiği cümle haberde bulunamadı.' })),
  ]
})

// The pipeline's draft is kept with the decision even when not applied (R19c): that it was ignored says something too.
async function loadDraft() {
  const id = articleId.value
  const draft = await getAiDraft(id)
  if (id !== articleId.value) return
  aiDraft.value = draft
  if (!draft) return
  aiUse.value = { answer: draft.answer, promptVersion: 'v2', applied: false, origin: draft.origin }
  // A relevant draft fills the form at once (R11): the researcher reviews the marked fields and the evidence.
  if (draft.check.relevant && needsProtest.value) onAiApply(draft.check, true)
}

function clearAi() {
  aiEvidence.value = []
  aiFlags.value = []
  activeEvidence.value = null
  aiDraft.value = null
  aiUse.value = null
  aiApplied.value = null
  aiCompanyName.value = null
  aiChosenCompanyName.value = null
  aiChosenMainCompanyName.value = null
  aiMainCompanyName.value = null
  aiChosenWorkplaceName.value = null
  aiWorkplaceName.value = null
  aiNotRelevant.value = null
}

// A new article starts with a clean form.
watch(articleId, () => {
  clearAi()
  action.value = 'newCase'
  caseValue.value = emptyCase()
  protestValue.value = emptyProtest()
  resistanceId.value = null
  finderCompanyId.value = null
  finderCompanyName.value = null
  fieldErrors.value = {}
})

// After the reset above, also for the first article opened.
watch(articleId, () => void loadDraft(), { immediate: true })

// The inbox's filters travel along, so "next" and "back" stay in the same month and source.
const inboxQuery = computed(() => ({
  ...(typeof route.query.month === 'string' ? { month: route.query.month } : {}),
  ...(typeof route.query.source === 'string' ? { source: route.query.source } : {}),
  ...(typeof route.query.part === 'string' ? { part: route.query.part } : {}),
}))

function currentMonth() {
  const d = new Date()
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`
}

/** The next open article in the inbox (after this one when it is still open), or back to the inbox. */
async function goNext() {
  try {
    // The next article of the tab the researcher came from (drafts, or the rest to review); never the AI's rejections.
    const part = inboxQuery.value.part === 'draft' ? 'draft' : 'other'
    const page = await getInbox(inboxQuery.value.month ?? currentMonth(), inboxQuery.value.source ?? null, null, part)
    const index = page.items.findIndex((i) => i.articleId === articleId.value)
    const next = index >= 0 ? page.items[index + 1] : page.items[0]
    if (next) {
      await router.push({ path: `/review/${encodeURIComponent(next.articleId)}`, query: inboxQuery.value })
      return
    }
    message.info('Bu ayda karar bekleyen başka haber yok.')
  } catch {
    // fall back to the inbox
  }
  await router.push({ path: '/inbox', query: inboxQuery.value })
}

// Saving: the server checks the required fields and answers with a message per field.
const saving = ref(false)
const canSave = computed(() => !saving.value && (!needsProtest.value || formOptions.value !== null))

const successText: Record<ReviewAction, string> = {
  newCase: 'Vaka kaydedildi; haber vakaya bağlandı.',
  addProtest: 'Eylem eklendi; haber vakaya bağlandı.',
  linkOnly: 'Haber vakaya bağlandı.',
}

function request() {
  switch (action.value) {
    case 'newCase':
      return { action: action.value, case: toCaseInput(caseValue.value), protest: toProtestInput(protestValue.value), ai: aiUse.value }
    case 'addProtest':
      return { action: action.value, resistanceId: resistanceId.value, protest: toProtestInput(protestValue.value), ai: aiUse.value }
    default:
      return { action: action.value, resistanceId: resistanceId.value, ai: aiUse.value }
  }
}

// Bring the first field with a message into view.
async function showFirstError() {
  await nextTick()
  document.querySelector('.n-form-item-feedback--error, .field-error')?.scrollIntoView({ behavior: 'smooth', block: 'center' })
}

async function save() {
  if (!canSave.value) return
  saving.value = true
  fieldErrors.value = {}
  try {
    const result = await saveReview(articleId.value, request())
    if (result.outcomeSent) message.success(successText[action.value])
    else message.warning(`${successText[action.value]} İşleme hattına daha sonra bildirilecek.`)
    await goNext()
  } catch (error) {
    if (error instanceof ApiError && error.status === 400) {
      fieldErrors.value = error.fields
      message.error(error.message)
      await showFirstError()
    } else if (error instanceof ApiError && error.status === 409) {
      message.warning(error.message)
      await goNext()
    } else {
      message.error(errorMessage(error, 'Kaydedilemedi.'))
    }
  } finally {
    saving.value = false
  }
}

const dismissTarget = ref<ReviewArticle | null>(null)

async function onDismissed() {
  dismissTarget.value = null
  await goNext()
}
</script>

<template>
  <router-link :to="{ path: '/inbox', query: inboxQuery }" class="back">← Gelen Kutusu</router-link>

  <div v-if="loading" class="state"><n-spin /></div>

  <n-result v-else-if="loadError === 'handled'" status="info" title="Bu haber zaten işlendi" description="Bir başkası bu haberi bir vakaya bağladı ya da ilgisiz olarak işaretledi.">
    <template #footer><router-link to="/inbox">Gelen Kutusu’na dön</router-link></template>
  </n-result>

  <n-result v-else-if="loadError === 'notFound'" status="404" title="Haber bulunamadı">
    <template #footer><router-link to="/inbox">Gelen Kutusu’na dön</router-link></template>
  </n-result>

  <n-alert v-else-if="loadError === 'unavailable'" type="error" :show-icon="false">
    {{ loadErrorText }}
    <n-button size="small" class="retry" @click="loadArticle">Tekrar dene</n-button>
  </n-alert>

  <div v-else-if="article" class="columns">
    <article class="article panel">
      <div class="meta">
        <span>{{ article.sourceName }}</span>
        <span>{{ formatDate(article.publishedAt) }}</span>
        <a :href="article.url" target="_blank" rel="noopener">Kaynağa git ↗</a>
      </div>
      <h1 class="article-title">
        <template v-if="article.title">
          <template v-for="(s, i) in titleSegments" :key="i">
            <mark v-if="s.fields.length" class="evidence" :class="{ active: s.fields.includes(activeEvidence ?? '') }" :data-evidence="s.fields.join(' ')" :title="s.labels.join(', ')">{{ s.text }}</mark>
            <template v-else>{{ s.text }}</template>
          </template>
        </template>
        <template v-else>{{ article.url }}</template>
      </h1>
      <p v-if="article.ai" class="ai-verdict" :class="article.ai.verdict">
        <strong>AI: {{ verdictText[article.ai.verdict] }}.</strong>
        <span v-if="article.ai.reason"> {{ article.ai.reason }}</span>
        <span v-if="article.ai.version" class="ai-origin">{{ article.ai.version }}</span>
      </p>
      <div v-if="evidenceList.length" class="evidence-list" aria-label="AI kanıtları">
        <span class="evidence-head">AI kanıtları:</span>
        <button
          v-for="e in evidenceList"
          :key="e.field"
          type="button"
          class="evidence-chip"
          :class="{ missing: !e.found, active: activeEvidence === e.field }"
          :title="e.found ? e.quote : `Metinde bulunamadı: “${e.quote}”`"
          @click="e.found && showEvidence(e.field)"
        >
          {{ e.label }}<span v-if="!e.found"> · bulunamadı</span>
        </button>
      </div>
      <!-- From the news site itself (not copied); hidden if the site no longer serves it. -->
      <img
        v-if="article.imageUrl && !imageFailed"
        :src="article.imageUrl"
        alt=""
        class="article-image"
        loading="lazy"
        referrerpolicy="no-referrer"
        @error="imageFailed = true"
      />
      <p v-if="article.body" class="body">
        <template v-for="(s, i) in bodySegments" :key="i">
          <mark v-if="s.fields.length" class="evidence" :class="{ active: s.fields.includes(activeEvidence ?? '') }" :data-evidence="s.fields.join(' ')" :title="s.labels.join(', ')">{{ s.text }}</mark>
          <template v-else>{{ s.text }}</template>
        </template>
      </p>
      <p v-else class="no-body">Haberin metni henüz okunmadı; kaynağa giderek okuyabilirsiniz.</p>
    </article>

    <section class="decision panel" aria-labelledby="decision-heading">
      <div v-if="aiMatchesShown" class="suggestions" aria-labelledby="ai-suggestions-heading">
        <h2 id="ai-suggestions-heading" class="decision-heading">Olası vakalar</h2>
        <p class="hint">AI’ın bulduğu şirkete, örgüte ve ile göre; neden önerildiği her vakanın altında.</p>
        <ul class="suggestion-list">
          <li v-for="m in aiMatches!.likely" :key="m.case.id" class="suggestion" :class="{ selected: resistanceId === m.case.id && action !== 'newCase', recorded: !!m.recorded }">
            <div class="suggestion-text">
              <span class="suggestion-name">
                {{ m.case.name || `Vaka #${m.case.id}` }}
                <n-tag v-if="m.case.ongoing" size="small" type="success" :bordered="false">devam ediyor</n-tag>
              </span>
              <span class="suggestion-meta">{{ m.reasons.join(' · ') }} · {{ m.case.protestCount }} eylem</span>
              <span v-if="m.recorded" class="recorded">
                Bu eylem kayıtlı görünüyor: {{ formatDate(m.recorded.startDate) }} · {{ m.recorded.cities.join(', ') }} · {{ m.recorded.types.join(', ') }}
              </span>
            </div>
            <div class="suggestion-actions">
              <template v-if="m.recorded">
                <n-button size="small" type="primary" @click="useAiMatch(m, 'linkOnly')">Sadece bağla</n-button>
                <n-button size="small" quaternary @click="useAiMatch(m, 'addProtest')">Eylem ekle</n-button>
              </template>
              <template v-else>
                <n-button size="small" @click="useAiMatch(m, 'addProtest')">Eylem ekle</n-button>
                <n-button size="small" quaternary @click="useAiMatch(m, 'linkOnly')">Sadece bağla</n-button>
              </template>
            </div>
          </li>
        </ul>
        <p v-if="aiMatches!.likely.length === 0" class="hint">Bu şirketin yakın zamanda süren bir vakası yok; yeni vaka olabilir.</p>
        <details v-if="aiMatches!.older.length" class="older-cases">
          <summary>Şirketin önceki vakaları ({{ aiMatches!.older.length }})</summary>
          <ul class="suggestion-list">
            <li v-for="m in aiMatches!.older" :key="m.case.id" class="suggestion" :class="{ selected: resistanceId === m.case.id && action !== 'newCase' }">
              <div class="suggestion-text">
                <span class="suggestion-name">{{ m.case.name || `Vaka #${m.case.id}` }}</span>
                <span class="suggestion-meta">{{ m.reasons.join(' · ') }}<template v-if="m.case.lastProtestDate"> · son eylem {{ formatDate(m.case.lastProtestDate) }}</template></span>
              </div>
              <div class="suggestion-actions">
                <n-button size="small" @click="useAiMatch(m, 'addProtest')">Eylem ekle</n-button>
                <n-button size="small" quaternary @click="useAiMatch(m, 'linkOnly')">Sadece bağla</n-button>
              </div>
            </li>
          </ul>
        </details>
      </div>
      <div v-else-if="suggestions.length" class="suggestions" aria-labelledby="suggestions-heading">
        <h2 id="suggestions-heading" class="decision-heading">Olası vakalar</h2>
        <p class="hint">Haberde geçen şirketlerin vakaları.</p>
        <ul class="suggestion-list">
          <li v-for="s in suggestions" :key="s.case.id" class="suggestion" :class="{ selected: resistanceId === s.case.id && action !== 'newCase' }">
            <div class="suggestion-text">
              <span class="suggestion-name">
                {{ s.case.name || `Vaka #${s.case.id}` }}
                <n-tag v-if="s.case.ongoing" size="small" type="success" :bordered="false">devam ediyor</n-tag>
              </span>
              <span class="suggestion-meta">
                “{{ s.matchedCompany }}” {{ s.inTitle ? 'başlıkta' : 'metinde' }} geçiyor
                · {{ s.case.protestCount }} eylem
                <template v-if="s.case.lastProtestDate"> · son eylem {{ formatDate(s.case.lastProtestDate) }}</template>
              </span>
            </div>
            <div class="suggestion-actions">
              <n-button size="small" @click="useSuggestion(s, 'addProtest')">Eylem ekle</n-button>
              <n-button size="small" quaternary @click="useSuggestion(s, 'linkOnly')">Sadece bağla</n-button>
            </div>
          </li>
        </ul>
      </div>

      <n-alert v-if="aiNotRelevant" type="warning" :show-icon="false" class="ai-irrelevant">
        <p class="ai-text"><strong>AI: ilgisiz görünüyor.</strong> {{ aiNotRelevant }}</p>
        <div class="ai-actions">
          <n-button size="small" @click="aiNotRelevant = null">Yine de incele</n-button>
          <n-button size="small" type="primary" @click="dismissTarget = article">İlgisiz olarak işaretle</n-button>
        </div>
      </n-alert>
      <div v-else-if="aiDraft?.check.relevant && needsProtest" class="ai-bar">
        <p class="ai-text">
          <strong>Form AI taslağıyla dolduruldu.</strong> “AI” işaretli alanları ve haberdeki kanıtları kontrol edin.
          <span class="ai-origin">{{ aiDraft.origin }}</span>
        </p>
        <n-button size="small" quaternary @click="openAiDialog()">Kendi AI sohbetimle doldur</n-button>
      </div>
      <div v-else-if="article.body && needsProtest" class="ai-bar">
        <p class="ai-text">Formu bir AI sohbetinin yanıtıyla önceden doldurun.</p>
        <n-button size="small" class="ai-button" @click="openAiDialog()">AI ile doldur</n-button>
      </div>
      <div v-if="toCheck.length && needsProtest" class="to-check" role="note" aria-labelledby="to-check-heading">
        <p id="to-check-heading" class="to-check-head">{{ toCheck.length }} değer kontrol edilmeli</p>
        <ul>
          <li v-for="c in toCheck" :key="c.key"><strong>{{ c.label }}:</strong> {{ c.text }}</li>
        </ul>
      </div>

      <h2 id="decision-heading" class="decision-heading">Önerilen işlem</h2>

      <n-radio-group v-model:value="action" name="action" class="choices">
        <label v-for="option in actionOptions" :key="option.value" class="choice" :class="{ selected: action === option.value }">
          <n-radio :value="option.value" />
          <span>{{ option.label }}</span>
        </label>
      </n-radio-group>

      <div v-if="needsCase" class="field">
        <span class="field-label">Vaka</span>
        <case-finder
          v-model="resistanceId"
          v-model:company-id="finderCompanyId"
          v-model:company-name="finderCompanyName"
          :cities="formOptions?.cities ?? []"
          :error="fieldErrors.resistanceId"
        />
      </div>

      <template v-if="action === 'newCase'">
        <n-alert v-if="formOptionsError" type="error" :show-icon="false">
          Form listeleri yüklenemedi.
          <n-button size="small" class="retry" @click="loadFormOptions">Tekrar dene</n-button>
        </n-alert>
        <div v-else-if="!formOptions" class="state"><n-spin /></div>
        <case-form
          v-else
          v-model="caseValue"
          :options="formOptions"
          :errors="fieldErrors"
          :ai-fields="aiCaseFields"
          :company-name="aiChosenCompanyName"
          :main-company-name="aiChosenMainCompanyName"
          :suggested-company-name="aiCompanyName"
          :suggested-main-company-name="aiMainCompanyName"
          :workplace-name="aiChosenWorkplaceName"
          :suggested-workplace-name="aiWorkplaceName"
          @use-existing="onUseExisting"
        />
      </template>

      <template v-if="needsProtest && formOptions">
        <protest-form v-model="protestValue" :options="formOptions" :errors="fieldErrors" :ai-fields="aiProtestFields" />
      </template>
      <n-alert v-else-if="needsProtest && formOptionsError && action === 'addProtest'" type="error" :show-icon="false">
        Form listeleri yüklenemedi.
        <n-button size="small" class="retry" @click="loadFormOptions">Tekrar dene</n-button>
      </n-alert>

      <div class="buttons">
        <n-button type="primary" :disabled="!canSave" :loading="saving" @click="save">Kaydet</n-button>
        <n-button @click="dismissTarget = article">İlgisiz olarak işaretle</n-button>
        <n-button quaternary @click="router.push({ path: '/inbox', query: inboxQuery })">Vazgeç</n-button>
        <n-button quaternary class="next" @click="goNext">Sonraki haber →</n-button>
      </div>
      <p class="hint">Hiçbir şey sizin onayınız olmadan kaydedilmez. Kaydedince haber Gelen Kutusu’ndan kalkar.</p>
    </section>
  </div>

  <dismiss-dialog :article="dismissTarget" :suggested-reason="aiNotRelevant" :ai="aiUse" @handled="onDismissed" @close="dismissTarget = null" />
  <ai-fill-dialog :article-id="aiDialogFor" @checked="aiUse = $event" @apply="onAiApply" @not-relevant="onAiNotRelevant" @close="aiDialogFor = null" />
</template>

<style scoped>
.back {
  display: inline-block;
  margin-bottom: 16px;
  font-weight: 500;
}
.state {
  padding: 48px 16px;
  display: flex;
  justify-content: center;
}
.retry {
  margin-left: 12px;
}
.columns {
  display: flex;
  flex-wrap: wrap;
  gap: 20px;
  align-items: flex-start;
}
.panel {
  background: var(--rs-surface);
  border: 1px solid var(--rs-border);
  border-radius: 10px;
  box-sizing: border-box;
}
.article {
  flex: 1 1 420px;
  min-width: 0;
  padding: 24px 28px;
}
/* On wide screens the article stays in view while the form on the right scrolls. */
@media (min-width: 1000px) {
  .article {
    position: sticky;
    top: 16px;
    max-height: calc(100vh - 32px);
    overflow: auto;
  }
}
.meta {
  display: flex;
  flex-wrap: wrap;
  gap: 8px 16px;
  font-size: 13px;
  color: var(--rs-muted);
}
.article-title {
  margin: 12px 0 16px;
  font-size: 24px;
  line-height: 1.3;
}
.article-image {
  display: block;
  width: 100%;
  max-height: 360px;
  object-fit: cover;
  border-radius: 8px;
  margin: 0 0 16px;
}
.body {
  margin: 0;
  font-size: 16px;
  line-height: 1.7;
  white-space: pre-line;
}
.no-body {
  color: var(--rs-muted);
}
.decision {
  flex: 1.2 1 480px;
  min-width: 0;
  padding: 24px;
  display: flex;
  flex-direction: column;
  gap: 16px;
}
.decision-heading {
  margin: 0;
  font-size: 18px;
}
.choices {
  display: flex;
  flex-direction: column;
  gap: 8px;
}
.choice {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 12px 14px;
  border: 1px solid #d1d5db;
  border-radius: 8px;
  cursor: pointer;
  font-size: 15px;
}
.choice.selected {
  border-color: var(--rs-accent);
  background: #f0f8f7;
}
.field {
  display: flex;
  flex-direction: column;
  gap: 6px;
}
.field-label {
  font-size: 14px;
  font-weight: 600;
}
.field-error {
  font-size: 13px;
  color: #b42318;
}
.buttons {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
  padding-top: 8px;
  border-top: 1px solid var(--rs-border);
}
.next {
  margin-left: auto;
}
.suggestions {
  display: flex;
  flex-direction: column;
  gap: 8px;
  padding-bottom: 16px;
  border-bottom: 1px solid var(--rs-border);
}
.suggestion-list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 6px;
}
.suggestion {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
  padding: 10px 12px;
  border: 1px solid #d1d5db;
  border-radius: 8px;
}
.suggestion.recorded {
  border-color: #e0b84c;
}
.recorded {
  font-size: 13px;
  font-weight: 500;
  color: #7a4a00;
}
.older-cases summary {
  cursor: pointer;
  font-size: 14px;
  color: var(--rs-muted);
  margin: 4px 0 6px;
}
.suggestion.selected {
  border-color: var(--rs-accent);
  background: #f0f8f7;
}
.suggestion-text {
  display: flex;
  flex-direction: column;
  gap: 2px;
  min-width: 0;
}
.suggestion-name {
  font-weight: 600;
  display: flex;
  align-items: center;
  gap: 8px;
}
.suggestion-meta {
  font-size: 13px;
  color: var(--rs-muted);
}
.suggestion-actions {
  display: flex;
  gap: 6px;
}
.evidence {
  background: #efeafb;
  color: inherit;
  border-bottom: 2px solid #b8a6ec;
  border-radius: 2px;
  padding: 0 1px;
  transition: background 0.3s;
}
.evidence.active {
  background: #d9cdf7;
  border-bottom-color: #6d4bc4;
}
.evidence-list {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 6px;
  margin: 0 0 14px;
  font-size: 13px;
}
.evidence-head {
  color: var(--rs-muted);
}
.evidence-chip {
  border: 1px solid #b8a6ec;
  background: #f5f2fc;
  color: #5c3db0;
  border-radius: 999px;
  padding: 1px 10px;
  font: inherit;
  cursor: pointer;
}
.evidence-chip.active {
  background: #6d4bc4;
  color: #fff;
}
.evidence-chip.missing {
  border-style: dashed;
  border-color: #c9ccd1;
  background: transparent;
  color: var(--rs-muted);
  cursor: help;
}
.ai-verdict {
  margin: 0 0 16px;
  padding: 8px 12px;
  border-radius: 8px;
  font-size: 14px;
  background: #f3f4f6;
  color: var(--rs-muted);
}
.ai-verdict.Uncertain {
  background: #fdf3e1;
  color: #7a4a00;
}
.ai-verdict.Relevant {
  background: #e8f4ec;
  color: #1f5b33;
}
.ai-bar,
.ai-irrelevant :deep(.n-alert-body__content) {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
}
.ai-bar {
  padding: 10px 14px;
  border-radius: 8px;
  background: #f5f2fc;
}
.to-check {
  margin-top: 10px;
  padding: 10px 14px;
  border-left: 3px solid #d4a72c;
  background: #fff8e5;
  border-radius: 0 8px 8px 0;
  font-size: 14px;
}
.to-check-head {
  margin: 0 0 4px;
  font-weight: 600;
}
.to-check ul {
  margin: 0;
  padding-left: 18px;
  display: flex;
  flex-direction: column;
  gap: 2px;
}
.ai-origin {
  margin-left: 6px;
  font-size: 12px;
  color: var(--rs-muted);
}
.ai-text {
  margin: 0;
  font-size: 14px;
}
.ai-actions {
  display: flex;
  gap: 8px;
}
.ai-button {
  --n-color: #6d4bc4;
  --n-color-hover: #7d5dd0;
  --n-color-pressed: #5c3db0;
  --n-color-focus: #7d5dd0;
  --n-text-color: #fff;
  --n-text-color-hover: #fff;
  --n-text-color-pressed: #fff;
  --n-text-color-focus: #fff;
  --n-border: 1px solid #6d4bc4;
  --n-border-hover: 1px solid #7d5dd0;
  --n-border-pressed: 1px solid #5c3db0;
  --n-border-focus: 1px solid #7d5dd0;
}
.hint {
  margin: 0;
  font-size: 13px;
  color: var(--rs-muted);
}
</style>
