<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { NAlert, NButton, NFormItem, NInput, NSelect, NTabPane, NTabs, NTag } from 'naive-ui'
import { ApiError } from '@/api'
import {
  isLink,
  maxLinks,
  splitLinks,
  submitLinks,
  uploadNews,
  uploadSources,
  uploadTarget,
  type LinkResult,
  type UploadResult,
} from '@/uploadApi'

const tab = ref<'excel' | 'links'>('excel')
const now = new Date()
const source = ref(1)
const year = ref(now.getFullYear())
const month = ref(now.getMonth() + 1)
const file = ref<File | null>(null)
const fileInput = ref<HTMLInputElement | null>(null)

const yearOptions = Array.from({ length: now.getFullYear() - 2018 }, (_, i) => ({ label: String(now.getFullYear() - i), value: now.getFullYear() - i }))
const monthOptions = ['Ocak', 'Şubat', 'Mart', 'Nisan', 'Mayıs', 'Haziran', 'Temmuz', 'Ağustos', 'Eylül', 'Ekim', 'Kasım', 'Aralık'].map(
  (label, i) => ({ label, value: i + 1 }),
)

// Where this source's month will go: the pipeline (from the cut-over) or the app (as before).
const toPipeline = ref<boolean | null>(null)
watch(
  [source, year, month],
  async () => {
    toPipeline.value = null
    try {
      toPipeline.value = (await uploadTarget(source.value, year.value, month.value)).toPipeline
    } catch {
      // only a hint
    }
  },
  { immediate: true },
)

const uploading = ref(false)
const result = ref<UploadResult | null>(null)
const errors = ref<string[]>([])

function onFile(event: Event) {
  const input = event.target as HTMLInputElement
  file.value = input.files?.[0] ?? null
  result.value = null
  errors.value = []
}

const canUpload = computed(() => file.value !== null && !uploading.value)

async function upload() {
  if (!file.value) return
  uploading.value = true
  result.value = null
  errors.value = []
  try {
    result.value = await uploadNews(file.value, source.value, year.value, month.value)
    file.value = null
    if (fileInput.value) fileInput.value.value = ''
  } catch (error) {
    const listed = error instanceof ApiError ? (error.details?.errors as string[] | undefined) : undefined
    errors.value = listed ?? [error instanceof ApiError ? error.message : 'Dosya yüklenemedi.']
  } finally {
    uploading.value = false
  }
}

// "Bağlantılar": a pasted list of links, sent to the pipeline.
const linksText = ref('')
const pieces = computed(() => splitLinks(linksText.value))
const linkCount = computed(() => pieces.value.filter(isLink).length)
const invalidCount = computed(() => pieces.value.length - linkCount.value)
const sendingLinks = ref(false)
const linkResults = ref<LinkResult[] | null>(null)
const linksError = ref('')

const statusLabel: Record<LinkResult['status'], { text: string; type: 'success' | 'info' | 'error' | 'warning' }> = {
  Accepted: { text: 'yeni', type: 'success' },
  Duplicate: { text: 'zaten var', type: 'info' },
  Rejected: { text: 'reddedildi', type: 'error' },
  Invalid: { text: 'geçersiz', type: 'warning' },
}
const accepted = computed(() => linkResults.value?.filter((r) => r.status === 'Accepted').length ?? 0)

async function sendLinks() {
  sendingLinks.value = true
  linksError.value = ''
  linkResults.value = null
  try {
    linkResults.value = (await submitLinks(linksText.value)).links
    if (linkResults.value.every((r) => r.status !== 'Invalid' && r.status !== 'Rejected')) linksText.value = ''
  } catch (error) {
    linksError.value = error instanceof ApiError ? error.message : 'Bağlantılar gönderilemedi.'
  } finally {
    sendingLinks.value = false
  }
}
</script>

<template>
  <h1 class="heading">Yükleme</h1>
  <p class="description">Haberleri Excel dosyasıyla ya da bağlantı listesiyle ekleyin.</p>

  <!-- display-directive="show": both panes stay mounted. Otherwise switching tabs patches one pane's button into the other's and keeps the old click handler (Gönder ran the Excel upload). -->
  <n-tabs v-model:value="tab" type="line" class="tabs">
  <n-tab-pane name="excel" tab="Excel dosyası" display-directive="show">
  <p class="description">Bir kaynağın bir aylık haberleri. Sütunlar: Link, Tarih, Başlık, Metin; ilk satır başlıktır.</p>
  <section class="panel">
    <div class="fields">
      <n-form-item label="Kaynak" label-placement="top" :show-feedback="false">
        <n-select v-model:value="source" :options="uploadSources" />
      </n-form-item>
      <n-form-item label="Yıl" label-placement="top" :show-feedback="false">
        <n-select v-model:value="year" :options="yearOptions" />
      </n-form-item>
      <n-form-item label="Ay" label-placement="top" :show-feedback="false">
        <n-select v-model:value="month" :options="monthOptions" />
      </n-form-item>
    </div>

    <p v-if="toPipeline !== null" class="target">
      <template v-if="toPipeline">Bu ayın haberleri <strong>işleme hattına</strong> gönderilecek; hazır olunca Gelen Kutusu’nda görünür.</template>
      <template v-else>Bu ayın haberleri <strong>uygulamaya</strong> yüklenecek (eski yöntem). Daha önce işlenmiş haberler korunur.</template>
    </p>

    <div class="file">
      <label for="upload-file" class="file-label">Dosya (.xlsx, en çok 4 MB)</label>
      <input id="upload-file" ref="fileInput" type="file" accept=".xlsx" @change="onFile" />
    </div>

    <div class="actions">
      <n-button type="primary" :disabled="!canUpload" :loading="uploading" @click="upload">Yükle</n-button>
      <span class="hint">Aynı dosyayı tekrar yüklemek güvenlidir.</span>
    </div>
  </section>

  <n-alert v-if="errors.length" type="error" title="Dosya yüklenmedi" class="result">
    <ul class="list">
      <li v-for="e in errors" :key="e">{{ e }}</li>
    </ul>
  </n-alert>

  <n-alert v-if="result && result.toPipeline" type="success" title="İşleme hattına gönderildi" class="result">
    {{ result.rows }} satır: {{ result.accepted }} yeni, {{ result.duplicate }} zaten vardı, {{ result.rejected?.length ?? 0 }} reddedildi.
    <ul v-if="result.rejected?.length" class="list">
      <li v-for="r in result.rejected.slice(0, 10)" :key="r">{{ r }}</li>
    </ul>
    <p class="next"><router-link to="/inbox">Gelen Kutusu’na git</router-link></p>
  </n-alert>

  <n-alert v-else-if="result" type="success" title="Haberler yüklendi" class="result">
    {{ result.rows }} satır: {{ result.imported }} yeni haber eklendi, {{ result.kept }} haber zaten işlenmişti ve korundu.
  </n-alert>
  </n-tab-pane>

  <n-tab-pane name="links" tab="Bağlantılar" display-directive="show">
  <p class="description">
    Haber bağlantılarını yapıştırın: her satıra bir bağlantı (boşluk, virgül ya da noktalı virgülle de ayırabilirsiniz).
    İşleme hattı sayfaları indirip okur; haberler birkaç dakika içinde Gelen Kutusu’nda görünür.
  </p>
  <section class="panel">
    <n-form-item :label="`Bağlantılar (en çok ${maxLinks})`" label-placement="top" :show-feedback="false">
      <n-input
        v-model:value="linksText"
        type="textarea"
        :autosize="{ minRows: 6, maxRows: 16 }"
        placeholder="https://www.evrensel.net/haber/…&#10;https://www.evrensel.net/haber/…"
        :input-props="{ id: 'links' }"
      />
    </n-form-item>
    <p class="hint">
      {{ linkCount }} bağlantı
      <template v-if="invalidCount"> · {{ invalidCount }} bağlantı olmayan parça (gönderilmez)</template>
      <template v-if="pieces.length > maxLinks"> · en çok {{ maxLinks }} gönderilebilir</template>
    </p>
    <div class="actions">
      <n-button type="primary" :disabled="linkCount === 0 || pieces.length > maxLinks" :loading="sendingLinks" @click="sendLinks">Gönder</n-button>
      <span class="hint">Şimdilik yalnızca Evrensel bağlantıları işlenir. Aynı bağlantıyı tekrar göndermek güvenlidir.</span>
    </div>
  </section>

  <n-alert v-if="linksError" type="error" :show-icon="false" class="result">{{ linksError }}</n-alert>

  <section v-if="linkResults" class="panel result" aria-label="Sonuç">
    <p class="summary">{{ linkResults.length }} parça: {{ accepted }} yeni.</p>
    <ul class="link-results">
      <li v-for="r in linkResults" :key="r.url">
        <n-tag size="small" :type="statusLabel[r.status].type" :bordered="false">{{ statusLabel[r.status].text }}</n-tag>
        <span class="link-url">{{ r.url }}</span>
        <span v-if="r.reason" class="hint">{{ r.reason }}</span>
      </li>
    </ul>
    <p v-if="accepted" class="next"><router-link to="/inbox">Gelen Kutusu’na git</router-link></p>
  </section>
  </n-tab-pane>
  </n-tabs>
</template>

<style scoped>
.heading {
  margin: 0;
  font-size: 30px;
}
.description {
  margin: 6px 0 20px;
  color: var(--rs-muted);
  font-size: 16px;
  max-width: 760px;
}
.panel {
  background: var(--rs-surface);
  border: 1px solid var(--rs-border);
  border-radius: 10px;
  padding: 20px 24px;
  max-width: 760px;
  display: flex;
  flex-direction: column;
  gap: 16px;
}
.fields {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
  gap: 12px;
}
.target {
  margin: 0;
  padding: 10px 12px;
  border-radius: 8px;
  background: #f0f8f7;
  font-size: 14px;
}
.file {
  display: flex;
  flex-direction: column;
  gap: 6px;
}
.file-label {
  font-size: 14px;
  font-weight: 600;
}
.actions {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 12px;
}
.hint {
  font-size: 13px;
  color: var(--rs-muted);
}
.result {
  margin-top: 20px;
  max-width: 760px;
}
.list {
  margin: 8px 0 0;
  padding-left: 18px;
}
.next {
  margin: 8px 0 0;
}
.tabs {
  max-width: 760px;
}
.summary {
  margin: 0;
  font-weight: 600;
}
.link-results {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 6px;
  max-height: 420px;
  overflow: auto;
}
.link-results li {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 4px 10px;
}
.link-url {
  word-break: break-all;
  font-size: 14px;
}
</style>
