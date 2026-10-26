<script setup lang="ts">
import { computed, ref } from 'vue'
import { NAlert, NButton, NInput, NPopconfirm, NSpin, NTag, useMessage } from 'naive-ui'
import { errorMessage } from '@/api'
import { isAdmin } from '@/auth'
import { formatDateTime } from '@/format'
import { activatePrompt, createPromptVersion, tryPrompt, type AiPromptInfo, type PromptTry } from '@/statusApi'

// R28: a prompt's versions edited in the app. A yönetici writes a new version, tries it on recent decided articles
// (compared with the running prompt and with what researchers decided), then activates it, or goes back to the
// built-in prompt from the pipeline's repository.
const props = defineProps<{ prompt: AiPromptInfo }>()
const emit = defineEmits<{ changed: [] }>()
const message = useMessage()

const builtin = 'builtin'
const required = computed(() => (props.prompt.name === 'extraction' ? ['{{LISTS}}', '{{TITLE}}', '{{TEXT}}'] : ['{{TITLE}}', '{{TEXT}}']))
const edited = computed(() => props.prompt.version !== props.prompt.builtin)

// Writing a new version, starting from the running text.
const editing = ref(false)
const text = ref('')
const note = ref('')
const saving = ref(false)
const missing = computed(() => required.value.filter((p) => !text.value.includes(p)))

function startEditing() {
  text.value = props.prompt.text
  note.value = ''
  editing.value = true
}

async function save() {
  saving.value = true
  try {
    const saved = await createPromptVersion(props.prompt.name, text.value, note.value)
    message.success(`${saved.version.version} kaydedildi. Denedikten sonra etkinleştirebilirsiniz.`)
    editing.value = false
    emit('changed')
  } catch (error) {
    message.error(errorMessage(error, 'Sürüm kaydedilemedi.'))
  } finally {
    saving.value = false
  }
}

// Trying a version: one AI request per article, up to 10.
const trying = ref<string | null>(null)
const tried = ref<PromptTry | null>(null)

async function runTry(version: string) {
  trying.value = version
  tried.value = null
  try {
    tried.value = await tryPrompt(props.prompt.name, version)
  } catch (error) {
    message.error(errorMessage(error, 'Deneme yapılamadı.'))
  } finally {
    trying.value = null
  }
}

const activating = ref<string | null>(null)

async function activate(version: string) {
  activating.value = version
  try {
    await activatePrompt(props.prompt.name, version)
    message.success(version === builtin ? 'Yerleşik sürüme dönüldü.' : `${version} etkin; işleme hattı bir dakika içinde kullanmaya başlar.`)
    emit('changed')
  } catch (error) {
    message.error(errorMessage(error, 'Sürüm etkinleştirilemedi.'))
  } finally {
    activating.value = null
  }
}

const shownText = ref<string | null>(null)

const verdicts: Record<string, string> = { Relevant: 'ilgili', Uncertain: 'belirsiz', NotRelevant: 'ilgisiz' }
const decisions: Record<string, string> = {
  linked: 'Vakaya bağlandı',
  dismissed: 'İlgisiz işaretlendi',
  'dismissed:NotLabour': 'İlgisiz: emek hareketiyle ilgisiz',
  'dismissed:AlreadyRecorded': 'İlgisiz: zaten kayıtlı',
  'dismissed:OutOfScope': 'İlgisiz: kapsam dışı',
  'dismissed:Other': 'İlgisiz: diğer',
  newCase: 'Yeni vaka',
  addProtest: 'Eylem eklendi',
}

function errorText(error: string) {
  if (error === 'NoText') return 'Haberin metni yok.'
  if (error === 'RateLimited') return 'AI kotası şu an dolu; biraz sonra tekrar deneyin.'
  if (error === 'NotConfigured') return 'İşleme hattında AI anahtarı yok.'
  return 'Yanıt okunamadı.'
}

function verdict(value: string | null) {
  return value ? (verdicts[value] ?? value) : '—'
}

function summary(value: string | null) {
  return value === 'NoEvent' ? 'Eylem bulmadı' : (value ?? '—')
}

function percent(value: number | null) {
  return value === null ? '—' : `%${value}`
}
</script>

<template>
  <div class="versions">
    <div class="head">
      <h3 class="title">Sürümler</h3>
      <n-button v-if="isAdmin && !editing" size="small" @click="startEditing">Yeni sürüm yaz</n-button>
    </div>

    <div v-if="editing" class="editor">
      <n-input v-model:value="text" type="textarea" :autosize="{ minRows: 12, maxRows: 30 }" class="text" aria-label="İstemin metni" />
      <p class="small" :class="{ bad: missing.length }">
        Metinde kalması gereken yer tutucular: <code v-for="p in required" :key="p" class="placeholder">{{ p }}</code>
        <template v-if="missing.length"> · Eksik: {{ missing.join(', ') }}</template>
      </p>
      <n-input v-model:value="note" placeholder="Ne değişti? (ör. “Basın açıklamaları da ilgili”)" maxlength="500" class="note" aria-label="Not" />
      <div class="buttons">
        <n-button type="primary" :loading="saving" :disabled="missing.length > 0 || !text.trim() || text.trim() === prompt.text.trim()" @click="save">
          Sürüm olarak kaydet
        </n-button>
        <n-button :disabled="saving" @click="editing = false">Vazgeç</n-button>
      </div>
      <p class="small muted">Kaydedilen sürüm hemen kullanılmaz: önce deneyin, sonra etkinleştirin.</p>
    </div>

    <table class="list">
      <tbody>
        <tr v-for="v in prompt.versions" :key="v.version">
          <td>
            <strong>{{ v.version }}</strong>
            <n-tag v-if="v.active" type="success" size="small" :bordered="false" class="tag">Etkin</n-tag>
            <div class="small muted">
              {{ v.createdBy ?? '—' }}, {{ formatDateTime(v.createdAt) }}<template v-if="v.note"> · {{ v.note }}</template>
            </div>
          </td>
          <td class="actions">
            <n-button size="tiny" quaternary @click="shownText = shownText === v.version ? null : v.version">Metin</n-button>
            <template v-if="isAdmin">
              <n-button size="tiny" :loading="trying === v.version" :disabled="trying !== null" @click="runTry(v.version)">Dene</n-button>
              <n-popconfirm v-if="!v.active" positive-text="Etkinleştir" negative-text="Vazgeç" @positive-click="activate(v.version)">
                <template #trigger><n-button size="tiny" type="primary" :loading="activating === v.version">Etkinleştir</n-button></template>
                İşleme hattı bundan sonraki haberleri {{ v.version }} ile işler.
              </n-popconfirm>
            </template>
          </td>
        </tr>
        <tr>
          <td>
            <strong>{{ prompt.builtin }}</strong> <span class="muted">(yerleşik, işleme hattının deposundan)</span>
            <n-tag v-if="!edited" type="success" size="small" :bordered="false" class="tag">Etkin</n-tag>
          </td>
          <td class="actions">
            <template v-if="isAdmin">
              <n-button size="tiny" :loading="trying === builtin" :disabled="trying !== null" @click="runTry(builtin)">Dene</n-button>
              <n-popconfirm v-if="edited" positive-text="Yerleşik sürüme dön" negative-text="Vazgeç" @positive-click="activate(builtin)">
                <template #trigger><n-button size="tiny" :loading="activating === builtin">Yerleşik sürüme dön</n-button></template>
                İşleme hattı deposundaki {{ prompt.builtin }} sürümüne döner.
              </n-popconfirm>
            </template>
          </td>
        </tr>
      </tbody>
    </table>
    <pre v-if="shownText" class="prompt">{{ prompt.versions.find((v) => v.version === shownText)?.text }}</pre>

    <div v-if="trying" class="state"><n-spin size="small" /> Haberler AI’a soruluyor (en çok 10 haber, bir dakika kadar sürebilir) …</div>
    <section v-if="tried" class="try">
      <div class="head">
        <h3 class="title">Deneme: {{ tried.version }}<span v-if="tried.model" class="muted"> · {{ tried.model }}</span></h3>
        <n-button size="tiny" quaternary @click="tried = null">Kapat</n-button>
      </div>
      <n-alert v-if="tried.message" type="info" :show-icon="false">{{ tried.message }}</n-alert>
      <template v-else-if="tried.name === 'relevance'">
        <p class="small">
          Karara göre değerlendirilebilen {{ tried.compared }} haberden şu anki istem <strong>{{ tried.currentAgrees }}</strong>,
          denenen sürüm <strong>{{ tried.triedAgrees }}</strong> haberde araştırmacılarla aynı yönde karar verdi.
        </p>
        <div class="table-wrap">
          <table class="results">
            <thead>
              <tr><th>Haber</th><th>Araştırmacılar</th><th>Şu anki istem</th><th>Denenen sürüm</th></tr>
            </thead>
            <tbody>
              <tr v-for="r in tried.rows" :key="r.articleId">
                <td>{{ r.title || r.articleId }}</td>
                <td>{{ decisions[r.decision] ?? r.decision }}</td>
                <td :class="{ good: r.currentAgrees === true, bad: r.currentAgrees === false }">{{ verdict(r.current) }}</td>
                <td :class="{ good: r.triedAgrees === true, bad: r.triedAgrees === false }">
                  <template v-if="r.error">{{ errorText(r.error) }}</template>
                  <template v-else>{{ verdict(r.tried) }}<span v-if="r.triedReason" class="small muted"> · {{ r.triedReason }}</span></template>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </template>
      <template v-else>
        <p class="small">
          AI yanıtıyla kaydedilmiş son vakalarda: AI’ın verdiği değerlerden araştırmacıların koruduğu pay, şu anki istemin
          taslağıyla ve denenen sürümünkiyle.
        </p>
        <div class="table-wrap">
          <table class="results">
            <thead>
              <tr><th>Haber</th><th>Kayıt</th><th>Şu anki taslak</th><th>Denenen taslak</th><th class="num">Korunan</th></tr>
            </thead>
            <tbody>
              <tr v-for="r in tried.rows" :key="r.articleId">
                <td>{{ r.title || r.articleId }}</td>
                <td>{{ decisions[r.decision] ?? r.decision }}</td>
                <td>{{ summary(r.current) }}</td>
                <td>{{ r.error ? errorText(r.error) : summary(r.tried) }}</td>
                <td class="num">{{ percent(r.keptCurrent) }} → {{ percent(r.keptTried) }}</td>
              </tr>
            </tbody>
          </table>
        </div>
        <div v-if="tried.fields.length" class="table-wrap">
          <table class="results">
            <thead>
              <tr><th>Alan</th><th class="num">Şu anki</th><th class="num">Denenen</th></tr>
            </thead>
            <tbody>
              <tr v-for="f in tried.fields" :key="f.field">
                <td>{{ f.label }}</td>
                <td class="num">{{ percent(f.keptCurrent) }} <span class="muted">({{ f.givenCurrent }})</span></td>
                <td class="num" :class="{ good: (f.keptTried ?? 0) > (f.keptCurrent ?? 0), bad: (f.keptTried ?? 0) < (f.keptCurrent ?? 0) }">
                  {{ percent(f.keptTried) }} <span class="muted">({{ f.givenTried }})</span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </template>
      <p class="small muted">Deneme hiçbir şeyi kaydetmez; her haber için bir AI isteği harcar.</p>
    </section>
  </div>
</template>

<style scoped>
.versions {
  margin-top: 12px;
  border-top: 1px solid var(--rs-border);
  padding-top: 12px;
}
.head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
}
.title {
  margin: 0;
  font-size: 15px;
}
.editor {
  margin: 10px 0 16px;
}
.text :deep(textarea) {
  font-family: ui-monospace, SFMono-Regular, Menlo, monospace;
  font-size: 12.5px;
}
.note {
  margin: 8px 0;
}
.buttons > * + * {
  margin-left: 8px;
}
.placeholder {
  margin-left: 6px;
}
.small {
  margin: 6px 0;
  font-size: 13px;
}
.muted {
  color: var(--rs-muted);
}
.bad {
  color: #b42318;
}
.good {
  color: #1f7a3f;
}
.list,
.results {
  width: 100%;
  border-collapse: collapse;
  font-size: 14px;
  margin-top: 8px;
}
.list td,
.results td,
.results th {
  padding: 6px 8px;
  border-bottom: 1px solid #eef0f2;
  vertical-align: top;
  text-align: left;
}
.results th {
  font-size: 12px;
  color: var(--rs-muted);
  font-weight: 600;
}
.num {
  text-align: right;
  white-space: nowrap;
}
.results th.num {
  text-align: right;
}
.actions {
  text-align: right;
  white-space: nowrap;
}
.actions > * + * {
  margin-left: 6px;
}
.tag {
  margin-left: 6px;
}
.table-wrap {
  overflow-x: auto;
}
.state {
  margin: 12px 0;
  font-size: 13px;
  display: flex;
  gap: 8px;
  align-items: center;
}
.try {
  margin-top: 16px;
  padding: 12px;
  border: 1px solid var(--rs-border);
  border-radius: 8px;
  background: var(--rs-ground);
}
.prompt {
  margin: 10px 0 0;
  max-height: 360px;
  overflow: auto;
  white-space: pre-wrap;
  font-size: 12.5px;
  padding: 12px 14px;
  border: 1px solid var(--rs-border);
  border-radius: 8px;
  background: var(--rs-ground);
}
</style>
