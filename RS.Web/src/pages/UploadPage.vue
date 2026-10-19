<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { NAlert, NButton, NFormItem, NSelect } from 'naive-ui'
import { ApiError } from '@/api'
import { uploadNews, uploadSources, uploadTarget, type UploadResult } from '@/uploadApi'

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
</script>

<template>
  <h1 class="heading">Yükleme</h1>
  <p class="description">
    Bir kaynağın bir aylık haberlerini Excel ile ekleyin. Sütunlar: Link, Tarih, Başlık, Metin; ilk satır başlıktır.
  </p>

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
</style>
