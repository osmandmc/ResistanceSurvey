<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { NAlert, NButton, NFormItem, NSelect, NSpin } from 'naive-ui'
import { errorMessage } from '@/api'
import { formatDate, formatDateTime } from '@/format'
import { collectFeeds, getStatus, queueForAi, type CollectedFeed, type QueuedForAi, type StatusPage, type StatusSource } from '@/statusApi'
import { verdictText } from '@/relevance'

const days = ref(14)
const dayOptions = [
  { label: 'Son 7 gün', value: 7 },
  { label: 'Son 14 gün', value: 14 },
  { label: 'Son 30 gün', value: 30 },
]

const status = ref<StatusPage | null>(null)
const loading = ref(false)
const loadError = ref('')

// "Şimdi topla": the pipeline reads the feeds now instead of at its next two-hourly run.
const collecting = ref(false)
const collected = ref<CollectedFeed[] | null>(null)
const collectError = ref('')

async function collectNow() {
  collecting.value = true
  collected.value = null
  collectError.value = ''
  try {
    collected.value = await collectFeeds()
    await load()
  } catch (error) {
    collectError.value = errorMessage(error, 'Kaynaklar okunamadı.')
  } finally {
    collecting.value = false
  }
}

// "AI ile işle": the source's articles the AI has not handled go to it now (verdict, then a draft for relevant ones).
const queuing = ref<string | null>(null)
const queued = ref<QueuedForAi | null>(null)
const queueError = ref('')

async function processWithAi(source: StatusSource) {
  queuing.value = source.sourceId
  queued.value = null
  queueError.value = ''
  try {
    queued.value = await queueForAi(source.sourceId)
  } catch (error) {
    queueError.value = errorMessage(error, 'Haberler AI’a gönderilemedi.')
  } finally {
    queuing.value = null
  }
}

function minutes(count: number, spacingSeconds: number) {
  return Math.max(1, Math.ceil((count * spacingSeconds) / 60))
}

/** Open articles without a draft that the AI did not reject: the İncelenecek tab. */
function toReview(source: StatusSource) {
  return Math.max(0, source.open - source.openWithDraft - source.openNotRelevant)
}

/** The AI's rejections and what researchers did with them: the number to trust it by. */
const rejected = computed(() => status.value?.ai.agreement.find((a) => a.verdict === 'NotRelevant') ?? null)

async function load() {
  loading.value = true
  loadError.value = ''
  try {
    status.value = await getStatus(days.value)
  } catch (error) {
    loadError.value = errorMessage(error, 'Durum bilgisi yüklenemedi.')
  } finally {
    loading.value = false
  }
}
watch(days, load, { immediate: true })

// A source counts as silent when nothing new arrived for a day: Evrensel publishes every day.
const silentAfterHours = 24

function hoursSince(value: string) {
  return (Date.now() - new Date(value).getTime()) / 3_600_000
}
function isSilent(source: StatusSource) {
  return source.lastReceivedAt === null || hoursSince(source.lastReceivedAt) > silentAfterHours
}
function ago(value: string) {
  const hours = hoursSince(value)
  if (hours < 1) return `${Math.max(1, Math.round(hours * 60))} dakika önce`
  if (hours < 48) return `${Math.round(hours)} saat önce`
  return `${Math.round(hours / 24)} gün önce`
}

const originLabel: Record<string, string> = { Collector: 'RSS', Csv: 'Excel', Manual: 'bağlantı listesi' }

const stageLabel: Record<string, string> = {
  Submitted: 'Alma',
  Fetched: 'Sayfa indirme',
  Parsed: 'Metin okuma',
  Relevance: 'İlgi kontrolü',
  Extracted: 'Bilgi çıkarma',
  Validated: 'Doğrulama',
}

// Bar length for "received" relative to the busiest day of that source.
function barWidth(source: StatusSource, received: number) {
  const max = Math.max(1, ...source.days.map((d) => d.received))
  return `${(received / max) * 100}%`
}

const period = computed(() => (status.value ? `${formatDate(status.value.from)} – ${formatDate(status.value.to)}` : ''))
</script>

<template>
  <div class="page-head">
    <div>
      <h1 class="heading">Durum Paneli</h1>
      <p class="description">Haber işleme hattına gelen haberler ve onlara ne olduğu, kaynak kaynak ve gün gün.</p>
    </div>
    <div class="head-actions">
      <n-button :loading="collecting" @click="collectNow">Şimdi topla</n-button>
      <n-form-item label="Dönem" label-placement="top" :show-feedback="false">
        <n-select v-model:value="days" :options="dayOptions" class="period-select" />
      </n-form-item>
    </div>
  </div>

  <n-alert v-if="queueError" type="error" :show-icon="false" class="collect-result" closable @close="queueError = ''">{{ queueError }}</n-alert>
  <n-alert v-else-if="queued" type="info" :show-icon="false" class="collect-result" closable @close="queued = null">
    <template v-if="!queued.enabled">AI kapalı: işleme hattında anahtar tanımlı değil, hiçbir haber gönderilmedi.</template>
    <template v-else-if="queued.toAssess + queued.toDraft === 0">{{ queued.sourceName }}: AI’ın işlemediği haber yok.</template>
    <template v-else>
      {{ queued.sourceName }}: {{ queued.toAssess }} haber ilgi kontrolüne, {{ queued.toDraft }} haber taslak için sıraya alındı;
      yaklaşık {{ minutes(queued.toAssess + queued.toDraft, queued.spacingSeconds) }} dakika sürer. İlgili bulunanlar ayrıca taslağa gider.
      <template v-if="queued.more"> Daha fazlası bekliyor: bunlar bitince tekrar tıklayın.</template>
    </template>
  </n-alert>
  <n-alert v-if="collectError" type="error" :show-icon="false" class="collect-result" closable @close="collectError = ''">{{ collectError }}</n-alert>
  <n-alert v-else-if="collected" type="info" :show-icon="false" class="collect-result" closable @close="collected = null">
    <ul class="collect-list">
      <li v-for="feed in collected" :key="feed.sourceId">
        <strong>{{ feed.sourceName }}:</strong>
        <template v-if="feed.error"> okunamadı ({{ feed.error }})</template>
        <template v-else> beslemede {{ feed.found }} haber, {{ feed.accepted }} yeni, {{ feed.duplicate }} zaten vardı<template v-if="feed.rejected">, {{ feed.rejected }} reddedildi</template>.</template>
      </li>
    </ul>
    <p v-if="collected.some((f) => f.accepted > 0)" class="collect-next">Yeni haberler okunduktan sonra <router-link to="/inbox">Gelen Kutusu’nda</router-link>.</p>
  </n-alert>

  <div v-if="loading && !status" class="state"><n-spin /></div>

  <n-alert v-else-if="loadError" type="error" :show-icon="false">
    {{ loadError }}
    <n-button size="small" class="retry" @click="load()">Tekrar dene</n-button>
  </n-alert>

  <template v-else-if="status">
    <p class="period">{{ period }}</p>

    <n-alert v-if="!status.ai.enabled" type="warning" :show-icon="false" class="collect-result">
      AI kapalı: işleme hattında anahtar tanımlı değil. Haberler değerlendirilmiyor ve taslak hazırlanmıyor.
    </n-alert>

    <section class="cards" aria-label="Kaynaklar">
      <article v-for="source in status.sources" :key="source.sourceId" class="card" :class="{ silent: isSilent(source) }">
        <h2 class="card-title">{{ source.sourceName }}</h2>
        <p v-if="source.lastReceivedAt" class="last">
          Son haber {{ ago(source.lastReceivedAt) }}
          <span v-if="source.lastReceivedOrigin" class="muted">({{ originLabel[source.lastReceivedOrigin] ?? source.lastReceivedOrigin }})</span>
        </p>
        <p v-if="isSilent(source)" class="warning">
          {{ source.lastReceivedAt ? `Son ${silentAfterHours} saatte yeni haber gelmedi.` : 'Bu dönemde hiç haber gelmedi.' }}
        </p>
        <dl class="numbers">
          <div>
            <dt>Alınan</dt>
            <dd>{{ source.received }}</dd>
          </div>
          <div>
            <dt>Hatalı</dt>
            <dd :class="{ bad: source.failed > 0 }">{{ source.failed }}</dd>
          </div>
          <div>
            <dt>Gelen kutusunda</dt>
            <dd>
              <router-link :to="{ path: '/inbox', query: { source: source.sourceId } }">{{ source.open }}</router-link>
            </dd>
          </div>
        </dl>
        <ul v-if="source.open > 0" class="split">
          <li><router-link :to="{ path: '/inbox', query: { source: source.sourceId, part: 'draft' } }">{{ source.openWithDraft }} taslak hazır</router-link></li>
          <li><router-link :to="{ path: '/inbox', query: { source: source.sourceId, part: 'other' } }">{{ toReview(source) }} incelenecek</router-link></li>
          <li><router-link :to="{ path: '/inbox', query: { source: source.sourceId, part: 'notRelevant' } }">{{ source.openNotRelevant }} AI: ilgisiz</router-link></li>
          <li :class="{ pending: source.openNotAssessed > 0 }">{{ source.openNotAssessed }} AI değerlendirmedi</li>
        </ul>
        <div v-if="status.ai.enabled" class="ai-row">
          <n-button size="small" :loading="queuing === source.sourceId" :disabled="queuing !== null" @click="processWithAi(source)">AI ile işle</n-button>
          <span class="muted small">Değerlendirilmeyenleri ve taslağı olmayan ilgili haberleri AI’a gönderir.</span>
        </div>
      </article>
    </section>

    <section v-for="source in status.sources.filter((s) => s.received > 0 || s.days.some((d) => d.parsed || d.failed || d.digested || d.dismissed))" :key="source.sourceId" class="panel">
      <h2 class="panel-title">{{ source.sourceName }} · gün gün</h2>
      <div class="table-wrap">
        <table class="days-table">
          <thead>
            <tr>
              <th>Gün</th>
              <th class="received-col">Alınan</th>
              <th class="num">Okunan</th>
              <th class="num">Hata</th>
              <th class="num">Vakaya bağlanan</th>
              <th class="num">İlgisiz</th>
              <th title="AI'ın o gün verdiği kararlar: ilgili · belirsiz · ilgisiz">AI kararı</th>
              <th class="num">Taslak</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="day in source.days" :key="day.day" :class="{ empty: day.received === 0 }">
              <td class="nowrap">{{ formatDate(day.day) }}</td>
              <td class="received-col">
                <div class="bar-cell">
                  <span class="bar" :style="{ width: barWidth(source, day.received) }" aria-hidden="true" />
                  <span class="bar-value">{{ day.received }}</span>
                </div>
              </td>
              <td class="num">{{ day.parsed }}</td>
              <td class="num" :class="{ bad: day.failed > 0 }">{{ day.failed }}</td>
              <td class="num">{{ day.digested }}</td>
              <td class="num">{{ day.dismissed }}</td>
              <td class="nowrap" :class="{ muted: day.assessedRelevant + day.assessedUncertain + day.assessedNotRelevant === 0 }">
                {{ day.assessedRelevant }} · {{ day.assessedUncertain }} · {{ day.assessedNotRelevant }}
              </td>
              <td class="num">{{ day.drafted }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>

    <section v-if="status.ai.enabled || status.ai.agreement.some((a) => a.digested + a.dismissed > 0)" class="panel">
      <h2 class="panel-title">AI ilgi kontrolü</h2>
      <p class="pad">
        Bu dönemde karar verilen haberler için AI’ın ne dediği ve araştırmacıların ne yaptığı.
        <template v-if="rejected && rejected.digested + rejected.dismissed > 0">
          AI’ın ilgisiz bulduğu haberler: {{ rejected.dismissed }} ilgisiz olarak kapatıldı,
          <strong :class="{ bad: rejected.digested > 0 }">{{ rejected.digested }} vakaya bağlandı</strong>.
          <template v-if="rejected.digested === 0"> AI’ın ilgisiz dedikleri şimdiye kadar güvenilir görünüyor.</template>
          <template v-else> Bu haberler AI’ın kaçırdığı vakalar: ilgi kontrolü istemini gözden geçirmek gerekebilir.</template>
        </template>
      </p>
      <div class="table-wrap">
        <table class="days-table agreement">
          <thead>
            <tr>
              <th>AI dedi ki</th>
              <th class="num">Vakaya bağlanan</th>
              <th class="num">İlgisiz bulunan</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="row in status.ai.agreement" :key="row.verdict">
              <td>{{ verdictText[row.verdict] }}</td>
              <td class="num" :class="{ bad: row.verdict === 'NotRelevant' && row.digested > 0 }">{{ row.digested }}</td>
              <td class="num">{{ row.dismissed }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>

    <section class="panel">
      <div class="panel-head">
        <h2 class="panel-title">Son hatalar</h2>
        <router-link :to="{ path: '/archive', query: { filter: 'failed' } }">Tüm hatalı haberler</router-link>
      </div>
      <p v-if="status.recentFailures.length === 0" class="muted pad">Bu dönemde hata yok.</p>
      <ul v-else class="failures">
        <li v-for="failure in status.recentFailures" :key="`${failure.articleId}-${failure.stage}`">
          <span class="when">{{ formatDateTime(failure.at) }}</span>
          <span class="what">
            <router-link :to="{ path: '/archive', query: { q: failure.url } }">{{ failure.title || failure.url }}</router-link>
            <span class="muted">{{ failure.sourceName }} · {{ stageLabel[failure.stage] ?? failure.stage }}<template v-if="failure.reason">: {{ failure.reason }}</template></span>
          </span>
        </li>
      </ul>
    </section>
  </template>
</template>

<style scoped>
.page-head {
  display: flex;
  flex-wrap: wrap;
  align-items: flex-end;
  justify-content: space-between;
  gap: 16px;
  margin-bottom: 8px;
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
.head-actions {
  display: flex;
  align-items: flex-end;
  gap: 12px;
}
.collect-result {
  margin-bottom: 16px;
}
.collect-list {
  margin: 0;
  padding-left: 18px;
}
.collect-next {
  margin: 6px 0 0;
}
.period-select {
  min-width: 160px;
}
.period {
  margin: 0 0 16px;
  font-size: 14px;
  color: var(--rs-muted);
}
.state {
  padding: 40px 16px;
}
.retry {
  margin-left: 12px;
}
.muted {
  color: var(--rs-muted);
}
.cards {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
  gap: 16px;
  margin-bottom: 20px;
}
.card {
  background: var(--rs-surface);
  border: 1px solid var(--rs-border);
  border-radius: 10px;
  padding: 16px 20px;
}
.card.silent {
  border-color: #f0b429;
}
.card-title {
  margin: 0 0 4px;
  font-size: 18px;
}
.last {
  margin: 0;
  font-size: 14px;
}
.warning {
  margin: 8px 0 0;
  padding: 6px 10px;
  border-radius: 6px;
  background: #fff7e0;
  color: #7a4d00;
  font-size: 14px;
}
.numbers {
  display: flex;
  gap: 24px;
  margin: 14px 0 0;
}
.numbers dt {
  font-size: 12px;
  color: var(--rs-muted);
}
.numbers dd {
  margin: 2px 0 0;
  font-size: 22px;
  font-weight: 600;
  font-variant-numeric: tabular-nums;
}
.numbers a {
  color: var(--rs-accent);
}
.bad {
  color: #b42318;
}
.split {
  list-style: none;
  margin: 10px 0 0;
  padding: 0;
  display: flex;
  flex-wrap: wrap;
  gap: 4px 14px;
  font-size: 13px;
}
.split a {
  color: var(--rs-accent);
}
.split .pending {
  color: #7a4d00;
  font-weight: 600;
}
.ai-row {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 8px;
  margin-top: 12px;
}
.small {
  font-size: 12px;
}
.agreement {
  min-width: 360px;
}
.panel {
  background: var(--rs-surface);
  border: 1px solid var(--rs-border);
  border-radius: 10px;
  margin-bottom: 20px;
}
.panel-head {
  display: flex;
  flex-wrap: wrap;
  justify-content: space-between;
  align-items: baseline;
  gap: 8px;
  padding-right: 16px;
}
.panel-head a {
  font-size: 14px;
  color: var(--rs-accent);
}
.panel-title {
  margin: 0;
  padding: 14px 16px;
  font-size: 16px;
}
.pad {
  margin: 0;
  padding: 0 16px 16px;
}
.table-wrap {
  overflow-x: auto;
}
.days-table {
  width: 100%;
  min-width: 620px;
  border-collapse: collapse;
  font-size: 14px;
}
.days-table th {
  text-align: left;
  font-size: 12px;
  letter-spacing: 0.06em;
  text-transform: uppercase;
  color: var(--rs-muted);
  font-weight: 600;
  padding: 8px 16px;
  border-bottom: 1px solid var(--rs-border);
}
.days-table td {
  padding: 8px 16px;
  border-bottom: 1px solid #eef0f2;
  font-variant-numeric: tabular-nums;
}
.days-table tr.empty td {
  color: var(--rs-muted);
}
.days-table .num {
  text-align: right;
}
.received-col {
  width: 40%;
}
.bar-cell {
  display: flex;
  align-items: center;
  gap: 8px;
}
.bar {
  display: inline-block;
  height: 10px;
  max-width: calc(100% - 40px);
  border-radius: 3px;
  background: var(--rs-accent);
}
.bar-value {
  min-width: 28px;
}
.nowrap {
  white-space: nowrap;
}
.failures {
  list-style: none;
  margin: 0;
  padding: 0 16px 12px;
  display: flex;
  flex-direction: column;
  gap: 10px;
  font-size: 14px;
}
.failures li {
  display: flex;
  flex-wrap: wrap;
  gap: 4px 16px;
}
.failures .when {
  min-width: 150px;
  color: var(--rs-muted);
  font-variant-numeric: tabular-nums;
}
.failures .what {
  display: flex;
  flex-direction: column;
  min-width: 0;
  word-break: break-word;
}
.failures a {
  color: var(--rs-ink);
}
</style>
