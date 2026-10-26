<script setup lang="ts">
import { ref } from 'vue'
import { NAlert, NButton, NSpin, useMessage } from 'naive-ui'
import { errorMessage } from '@/api'
import { getAiPrompts, queueForAi, sendLists, type AiPrompts } from '@/statusApi'
import { formatDateTime } from '@/format'

// "AI istemleri" (R25): what the pipeline's AI stages run now. Read only: a prompt changes through the pipeline
// repository (docs/extraction/), measured before it is merged.
const message = useMessage()
const prompts = ref<AiPrompts | null>(null)
const loadError = ref('')
const reassessing = ref(false)

const titles: Record<string, { title: string; what: string }> = {
  relevance: {
    title: 'İlgi kontrolü',
    what: 'Her yeni haberin başını okuyup emek eylemi anlatıp anlatmadığına karar verir: ilgili, belirsiz ya da ilgisiz.',
  },
  extraction: {
    title: 'Taslak (bilgi çıkarma)',
    what: 'İlgili ya da belirsiz bulunan haberlerden vaka ve eylem formunun taslağını çıkarır. Araştırmacılar “AI ile doldur”da aynı istemi kopyalar.',
  },
}

async function load() {
  loadError.value = ''
  try {
    prompts.value = await getAiPrompts()
  } catch (error) {
    loadError.value = errorMessage(error, 'İstemler yüklenemedi.')
  }
}
void load()

// The pipeline's sources with an inbox (as the inbox's source filter).
const sources = ['evrensel', 'kizilbayrak']

// The drafts offer the app's lists; the app sends them when they change (every 15 minutes at most) or here at once.
const sendingLists = ref(false)

async function sendListsNow() {
  sendingLists.value = true
  try {
    const result = await sendLists()
    message.success(result.changed ? 'Listeler gönderildi; yeni taslaklar bunları kullanacak.' : 'İşleme hattındaki listeler zaten güncel.')
    await load()
  } catch (error) {
    message.error(errorMessage(error, 'Listeler gönderilemedi.'))
  } finally {
    sendingLists.value = false
  }
}

async function reassess() {
  reassessing.value = true
  try {
    const results = await Promise.all(sources.map((s) => queueForAi(s, true)))
    const queued = results.reduce((sum, r) => sum + r.toAssess, 0)
    message.success(queued ? `${queued} haber yeni istemle değerlendirilmek üzere sıraya alındı.` : 'Yeniden değerlendirilecek haber yok.')
    await load()
  } catch (error) {
    message.error(errorMessage(error, 'Haberler sıraya alınamadı.'))
  } finally {
    reassessing.value = false
  }
}
</script>

<template>
  <p class="back"><router-link to="/definitions">← Tanımlar</router-link></p>
  <h1 class="heading">AI istemleri</h1>
  <p class="description">
    İşleme hattındaki yapay zekânın şu an kullandığı istemler. Bir istem, ölçülüp denendikten sonra işleme hattının
    deposunda (<code>docs/extraction/</code>) değiştirilir; burada yalnızca görüntülenir.
  </p>

  <n-alert v-if="loadError" type="error" :show-icon="false">
    {{ loadError }}
    <n-button size="small" class="retry" @click="load">Tekrar dene</n-button>
  </n-alert>
  <div v-else-if="!prompts" class="state"><n-spin /></div>

  <template v-else>
    <n-alert v-if="prompts.openWithOlderVerdict > 0" type="info" :show-icon="false" class="older">
      Gelen kutusundaki {{ prompts.openWithOlderVerdict }} haber, ilgi kontrolünün eski bir sürümüyle değerlendirildi.
      <n-button size="small" :loading="reassessing" class="retry" @click="reassess">Yeni istemle yeniden değerlendir</n-button>
      <p class="small">Her haber için bir AI isteği harcanır; yeni karar ilgisizse haber “AI: ilgisiz görünüyor” sekmesine geçer.</p>
    </n-alert>

    <section v-for="p in prompts.prompts" :key="p.name" class="panel">
      <div class="panel-head">
        <h2 class="panel-title">{{ titles[p.name]?.title ?? p.name }}</h2>
        <span class="version">{{ p.version }} · {{ p.model }}</span>
      </div>
      <p class="what">{{ titles[p.name]?.what }}</p>
      <p v-if="p.name === 'extraction'" class="lists">
        Listeler (nedenler, eylem türleri, iller …):
        <strong>{{ prompts.listsUpdatedAt ? formatDateTime(prompts.listsUpdatedAt) : '21.10.2026 tarihli ilk kopya' }}</strong>.
        Tanımlar’daki değişiklikler en geç 15 dakikada iletilir.
        <n-button size="tiny" :loading="sendingLists" class="retry" @click="sendListsNow">Listeleri şimdi gönder</n-button>
      </p>
      <details>
        <summary>İstemin metni</summary>
        <pre class="prompt">{{ p.text }}</pre>
      </details>
    </section>
  </template>
</template>

<style scoped>
.back {
  margin: 0 0 8px;
  font-size: 14px;
}
.back a {
  color: var(--rs-accent);
}
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
.state {
  padding: 40px 16px;
}
.retry {
  margin-left: 12px;
}
.older {
  margin-bottom: 16px;
}
.small {
  margin: 6px 0 0;
  font-size: 13px;
}
.panel {
  background: var(--rs-surface);
  border: 1px solid var(--rs-border);
  border-radius: 10px;
  padding: 16px 20px;
  margin-bottom: 16px;
}
.panel-head {
  display: flex;
  flex-wrap: wrap;
  align-items: baseline;
  justify-content: space-between;
  gap: 8px;
}
.panel-title {
  margin: 0;
  font-size: 18px;
}
.version {
  font-size: 13px;
  color: var(--rs-muted);
  font-variant-numeric: tabular-nums;
}
.what {
  margin: 8px 0 12px;
  font-size: 14px;
}
.lists {
  margin: 0 0 12px;
  font-size: 13px;
  color: var(--rs-muted);
}
summary {
  cursor: pointer;
  color: var(--rs-accent);
  font-size: 14px;
}
.prompt {
  margin: 10px 0 0;
  max-height: 480px;
  overflow: auto;
  white-space: pre-wrap;
  font-size: 12.5px;
  line-height: 1.5;
  padding: 12px 14px;
  border: 1px solid var(--rs-border);
  border-radius: 8px;
  background: var(--rs-ground);
}
</style>
