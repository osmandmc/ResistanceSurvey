<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { NAlert, NButton, NFormItem, NSelect, NSpin } from 'naive-ui'
import { errorMessage } from '@/api'
import { formatDate, formatDateTime } from '@/format'
import { getStatus, type StatusPage, type StatusSource } from '@/statusApi'

const days = ref(14)
const dayOptions = [
  { label: 'Son 7 gün', value: 7 },
  { label: 'Son 14 gün', value: 14 },
  { label: 'Son 30 gün', value: 30 },
]

const status = ref<StatusPage | null>(null)
const loading = ref(false)
const loadError = ref('')

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
    <n-form-item label="Dönem" label-placement="top" :show-feedback="false">
      <n-select v-model:value="days" :options="dayOptions" class="period-select" />
    </n-form-item>
  </div>

  <div v-if="loading && !status" class="state"><n-spin /></div>

  <n-alert v-else-if="loadError" type="error" :show-icon="false">
    {{ loadError }}
    <n-button size="small" class="retry" @click="load()">Tekrar dene</n-button>
  </n-alert>

  <template v-else-if="status">
    <p class="period">{{ period }}</p>

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
