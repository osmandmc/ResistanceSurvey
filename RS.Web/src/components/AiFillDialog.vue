<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { NAlert, NButton, NInput, NModal, NSpin, NStep, NSteps, NTag } from 'naive-ui'
import { errorMessage } from '@/api'
import { checkAiAnswer, getAiPrompt, type AiCheckResult, type AiItemStatus, type AiPrompt, type AiUse } from '@/aiFillApi'

/**
 * "AI ile doldur" (R19): copy the prompt into your own AI chat, paste its answer, see what the app makes of it, then
 * fill the form. Nothing is saved here.
 */
const props = defineProps<{
  /** The article to fill the form for; null closes the dialog. */
  articleId: string | null
}>()

const emit = defineEmits<{
  /** An answer was read: kept by the page and sent with the decision (R19c). */
  checked: [use: AiUse]
  apply: [result: AiCheckResult]
  /** The AI found the article not relevant. */
  notRelevant: [reason: string | null]
  close: []
}>()

const step = ref(1)
const prompt = ref<AiPrompt | null>(null)
const promptError = ref('')
const loadingPrompt = ref(false)
const copied = ref(false)
const answer = ref('')
const checking = ref(false)
const checkError = ref('')
const result = ref<AiCheckResult | null>(null)
const promptBox = ref<HTMLElement | null>(null)

async function loadPrompt(articleId: string) {
  loadingPrompt.value = true
  promptError.value = ''
  try {
    prompt.value = await getAiPrompt(articleId)
  } catch (error) {
    promptError.value = errorMessage(error, 'Komut hazırlanamadı.')
  } finally {
    loadingPrompt.value = false
  }
}

// Each article starts over: a new prompt, an empty answer.
watch(
  () => props.articleId,
  (articleId, previous) => {
    if (!articleId) return
    if (articleId !== previous || !prompt.value) {
      prompt.value = null
      answer.value = ''
      result.value = null
      void loadPrompt(articleId)
    }
    step.value = result.value ? 3 : answer.value ? 2 : 1
    copied.value = false
    checkError.value = ''
  },
  { immediate: true },
)

async function copy() {
  if (!prompt.value) return
  try {
    await navigator.clipboard.writeText(prompt.value.text)
    copied.value = true
  } catch {
    // Clipboard refused: select the text so Ctrl+C works.
    const selection = window.getSelection()
    if (promptBox.value && selection) {
      selection.selectAllChildren(promptBox.value)
    }
    promptError.value = 'Otomatik kopyalanamadı; metin seçildi, Ctrl+C (Mac: ⌘+C) ile kopyalayın.'
  }
}

async function check() {
  checking.value = true
  checkError.value = ''
  try {
    const checked = await checkAiAnswer(answer.value)
    emit('checked', { answer: answer.value, promptVersion: prompt.value?.version ?? null, applied: false })
    if (!checked.relevant) {
      emit('notRelevant', checked.relevanceReason)
      return
    }
    result.value = checked
    step.value = 3
  } catch (error) {
    checkError.value = errorMessage(error, 'Yanıt kontrol edilemedi.')
  } finally {
    checking.value = false
  }
}

function apply() {
  if (result.value) emit('apply', result.value)
}

const statusText: Record<AiItemStatus, string> = {
  Matched: 'eşleşti',
  Corrected: 'yazımı düzeltildi',
  New: 'yeni kayıt',
  NotInList: 'alınmadı',
  Skipped: 'siz seçin',
}
const statusType: Record<AiItemStatus, 'success' | 'warning' | 'info' | 'error' | 'default'> = {
  Matched: 'success',
  Corrected: 'warning',
  New: 'info',
  NotInList: 'error',
  Skipped: 'default',
}

const summary = computed(() => {
  const counts = new Map<AiItemStatus, number>()
  for (const item of result.value?.fields.flatMap((f) => f.items) ?? []) counts.set(item.status, (counts.get(item.status) ?? 0) + 1)
  return [...counts.entries()]
})
</script>

<template>
  <n-modal
    :show="props.articleId !== null"
    preset="card"
    title="AI ile doldur"
    :style="{ width: 'min(860px, calc(100vw - 32px))' }"
    :mask-closable="!checking"
    @update:show="(show: boolean) => !show && emit('close')"
  >
    <n-steps :current="step" size="small" class="steps">
      <n-step title="Komutu kopyala" />
      <n-step title="Yanıtı yapıştır" />
      <n-step title="Kontrol et" />
    </n-steps>

    <div v-if="step === 1" class="body">
      <p class="lead">Bu komutu kendi AI sohbetinize (ChatGPT, Claude, Gemini …) yapıştırın. Haberin metni ve güncel listeler içinde.</p>
      <div v-if="loadingPrompt" class="state"><n-spin /></div>
      <n-alert v-else-if="promptError && !prompt" type="error" :show-icon="false">{{ promptError }}</n-alert>
      <template v-else-if="prompt">
        <pre ref="promptBox" class="prompt" tabindex="0" aria-label="AI komutu">{{ prompt.text }}</pre>
        <p v-if="promptError" class="error">{{ promptError }}</p>
        <div class="row">
          <span class="note">{{ prompt.text.length.toLocaleString('tr-TR') }} karakter · istem {{ prompt.version }}</span>
          <div class="actions">
            <n-button :type="copied ? 'default' : 'primary'" @click="copy">{{ copied ? 'Kopyalandı' : 'Komutu kopyala' }}</n-button>
            <n-button :type="copied ? 'primary' : 'default'" @click="step = 2">Yanıtı yapıştırmaya geç</n-button>
          </div>
        </div>
      </template>
    </div>

    <div v-else-if="step === 2" class="body">
      <p class="lead">AI'ın yanıtını olduğu gibi yapıştırın. Başında ya da sonunda açıklama ya da <code>```json</code> olması sorun değil.</p>
      <n-input
        v-model:value="answer"
        type="textarea"
        :autosize="{ minRows: 10, maxRows: 20 }"
        placeholder='{ "relevant": true, … }'
        :input-props="{ id: 'ai-answer', spellcheck: false }"
        class="answer"
      />
      <p v-if="checkError" class="error">{{ checkError }}</p>
      <div class="row">
        <n-button quaternary @click="step = 1">Geri</n-button>
        <n-button type="primary" :disabled="!answer.trim()" :loading="checking" @click="check">Kontrol et</n-button>
      </div>
    </div>

    <div v-else-if="result" class="body">
      <div class="summary">
        <n-tag v-for="[status, count] in summary" :key="status" :type="statusType[status]" size="small" :bordered="false">
          {{ count }} {{ statusText[status] }}
        </n-tag>
      </div>
      <div class="table-wrap">
        <table>
          <thead>
            <tr><th>Alan</th><th>AI yanıtı</th><th>Forma gelecek</th></tr>
          </thead>
          <tbody>
            <tr v-for="field in result.fields" :key="field.field">
              <td class="label">{{ field.label }}</td>
              <td class="value">{{ field.items.map((i) => i.answer).join(', ') }}</td>
              <td class="value">
                <div v-for="(item, index) in field.items" :key="index" class="item">
                  <span v-if="item.value && item.status !== 'Skipped'">{{ item.value }}</span>
                  <span v-else class="muted">{{ item.answer }}</span>
                  <n-tag :type="statusType[item.status]" size="tiny" :bordered="false">{{ statusText[item.status] }}</n-tag>
                  <span v-if="item.note" class="muted">{{ item.note }}</span>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <p class="note">
        “Alınmadı” değerler forma gelmez. Yeni örgütler kayıtta eklenir; yeni bir şirketi formdaki düğmeyle siz eklersiniz.
        Sonuç, işyeri büyüklüğü, cinsiyet, hak geliştirme ve sendika yetkisini siz seçersiniz.
      </p>
      <div class="row">
        <n-button quaternary @click="step = 2">Geri</n-button>
        <n-button type="primary" @click="apply">Forma uygula</n-button>
      </div>
    </div>
  </n-modal>
</template>

<style scoped>
.steps {
  margin-bottom: 16px;
}
.body {
  display: flex;
  flex-direction: column;
  gap: 12px;
}
.lead {
  margin: 0;
}
.state {
  padding: 32px;
  display: flex;
  justify-content: center;
}
.prompt {
  margin: 0;
  max-height: 300px;
  overflow: auto;
  white-space: pre-wrap;
  font-size: 12.5px;
  line-height: 1.45;
  padding: 10px 12px;
  border: 1px solid var(--rs-border);
  border-radius: 8px;
  background: var(--rs-ground);
}
.answer :deep(textarea) {
  font-family: ui-monospace, monospace;
  font-size: 12.5px;
}
.row {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
}
.actions {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}
.note,
.muted {
  font-size: 13px;
  color: var(--rs-muted);
}
.note {
  margin: 0;
}
.error {
  margin: 0;
  font-size: 14px;
  color: #b42318;
}
.summary {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}
.table-wrap {
  overflow-x: auto;
  border: 1px solid var(--rs-border);
  border-radius: 8px;
  max-height: 50vh;
  overflow-y: auto;
}
table {
  width: 100%;
  border-collapse: collapse;
  font-size: 14px;
}
th,
td {
  text-align: left;
  vertical-align: top;
  padding: 7px 10px;
  border-bottom: 1px solid var(--rs-border);
}
th {
  font-size: 12px;
  font-weight: 500;
  color: var(--rs-muted);
  text-transform: uppercase;
  letter-spacing: 0.04em;
  position: sticky;
  top: 0;
  background: var(--rs-surface);
}
tr:last-child td {
  border-bottom: 0;
}
.label {
  white-space: nowrap;
}
.value {
  overflow-wrap: anywhere;
  min-width: 140px;
}
.item {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 6px;
}
.item + .item {
  margin-top: 4px;
}
</style>
