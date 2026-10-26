<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { NAlert, NButton, NEmpty, NSelect, NSpin } from 'naive-ui'
import { errorMessage } from '@/api'
import { formatDate } from '@/format'
import { getAiReport, type AiReport, type AiReportField } from '@/statusApi'

// "AI raporu" (R19d): per field, how often researchers keep the AI's values; the relevance verdicts against what
// researchers decided. For deciding which fields the drafts should fill and whether a model or prompt change helped.
const days = ref(90)
const group = ref<string | null>(null)
const relevanceVersion = ref<string | null>(null)
const report = ref<AiReport | null>(null)
const loading = ref(false)
const loadError = ref('')

const dayOptions = [30, 90, 180, 365].map((d) => ({ label: `Son ${d} gün`, value: d }))

// Origin and prompt version travel as one choice: "origin|version".
const groupOptions = computed(() =>
  (report.value?.groups ?? []).map((g) => ({
    label: `${originLabel(g.origin)}${g.promptVersion ? ` · ${g.promptVersion}` : ''} (${g.count})`,
    value: `${g.origin}|${g.promptVersion ?? ''}`,
  })),
)
const versionOptions = computed(() => (report.value?.relevance.versions ?? []).map((v) => ({ label: v, value: v })))

function originLabel(origin: string) {
  return origin === 'pasted' ? 'Yapıştırılan yanıt' : origin.replace(/^pipeline/, 'İşleme hattı taslağı')
}

async function load() {
  loading.value = true
  loadError.value = ''
  const [origin, promptVersion] = group.value ? group.value.split('|') : [null, null]
  try {
    report.value = await getAiReport({ days: days.value, origin, promptVersion: promptVersion || null, relevanceVersion: relevanceVersion.value })
  } catch (error) {
    loadError.value = errorMessage(error, 'Rapor yüklenemedi.')
  } finally {
    loading.value = false
  }
}
watch([days, group, relevanceVersion], load, { immediate: true })

/** Of the values the AI gave (scalars), or the items it suggested (lists): the share researchers kept. */
function keptShare(f: AiReportField) {
  const given = f.kept + f.changed + f.removed
  return given === 0 ? null : Math.round((100 * f.kept) / given)
}

function shareClass(share: number | null) {
  return share === null ? '' : share >= 80 ? 'good' : share >= 50 ? 'fair' : 'poor'
}

const expanded = ref<string | null>(null)

const verdictLabels: Record<string, string> = {
  Relevant: 'İlgili',
  Uncertain: 'Belirsiz',
  NotRelevant: 'İlgisiz',
  NotAssessed: 'Değerlendirilmedi',
}
const dismissColumns = [
  { key: 'NotLabour', label: 'Emek hareketiyle ilgisiz' },
  { key: 'AlreadyRecorded', label: 'Zaten kayıtlı' },
  { key: 'OutOfScope', label: 'Kapsam dışı' },
  { key: 'Other', label: 'Diğer' },
  { key: 'Unknown', label: 'Nedeni yok (eski)' },
]
</script>

<template>
  <p class="back"><router-link to="/status">← Durum Paneli</router-link></p>
  <h1 class="heading">AI raporu</h1>
  <p class="description">
    Araştırmacıların AI’ın önerdiği değerleri ne sıklıkla olduğu gibi kaydettiği, alan alan. Taslakların hangi alanları
    doldurması gerektiğine ve bir model ya da istem değişikliğinin işe yarayıp yaramadığına karar vermek için.
  </p>

  <div class="filters">
    <n-select v-model:value="days" :options="dayOptions" class="filter" aria-label="Dönem" />
    <n-select v-model:value="group" :options="groupOptions" clearable placeholder="Tüm yanıtlar" class="filter wide" aria-label="Yanıtın kaynağı" />
  </div>

  <n-alert v-if="loadError" type="error" :show-icon="false">
    {{ loadError }} <n-button size="small" @click="load">Tekrar dene</n-button>
  </n-alert>
  <div v-else-if="!report" class="state"><n-spin /></div>

  <template v-else>
    <section class="panel" :class="{ dim: loading }">
      <h2 class="panel-title">Alanlar</h2>
      <p class="small">
        {{ formatDate(report.from) }} – {{ formatDate(report.to) }}: AI yanıtıyla verilen {{ report.decisions }} karar (yeni vaka ya da
        eylem ekleme), {{ report.applied }} tanesinde “Forma uygula” kullanıldı.
        <template v-if="report.others"> {{ report.others }} yanıt “Sadece bağla” ya da ilgisiz kararıyla verildi; karşılaştırılacak alan yok.</template>
      </p>
      <n-empty v-if="report.fields.length === 0" description="Bu dönemde AI yanıtıyla kaydedilmiş vaka ya da eylem yok." class="state" />
      <div v-else class="table-wrap">
        <table class="report-table">
          <thead>
            <tr>
              <th>Alan</th>
              <th class="num" title="AI'ın değeri olduğu gibi kaydedildi">Korundu</th>
              <th class="num" title="Başka bir değer kaydedildi">Değişti</th>
              <th class="num" title="AI doldurdu, boş kaydedildi">Silindi</th>
              <th class="num" title="AI boş bıraktı, araştırmacı doldurdu">Eklendi</th>
              <th class="num" title="AI'ın verdiği değerlerden korunanların payı">Korunma</th>
            </tr>
          </thead>
          <tbody>
            <template v-for="f in report.fields" :key="f.field">
              <tr>
                <td>
                  <button v-if="f.isList && f.items.length" type="button" class="expand" :aria-expanded="expanded === f.field" @click="expanded = expanded === f.field ? null : f.field">
                    {{ expanded === f.field ? '▾' : '▸' }} {{ f.label }}
                  </button>
                  <template v-else>{{ f.label }}</template>
                  <span v-if="f.isList" class="muted"> (öğe)</span>
                </td>
                <td class="num">{{ f.kept }}</td>
                <td class="num">{{ f.isList ? '—' : f.changed }}</td>
                <td class="num">{{ f.removed }}</td>
                <td class="num">{{ f.added }}</td>
                <td class="num share" :class="shareClass(keptShare(f))">{{ keptShare(f) === null ? '—' : `%${keptShare(f)}` }}</td>
              </tr>
              <template v-if="expanded === f.field">
                <tr v-for="item in f.items" :key="item.name" class="item">
                  <td>{{ item.name }}</td>
                  <td class="num">{{ item.kept }}</td>
                  <td class="num">—</td>
                  <td class="num">{{ item.removed }}</td>
                  <td class="num">{{ item.added }}</td>
                  <td></td>
                </tr>
              </template>
            </template>
          </tbody>
        </table>
      </div>
      <p class="small muted">
        Korunma: AI’ın verdiği değerlerden (ya da listelerde önerdiği öğelerden) olduğu gibi kaydedilenlerin payı. Eylem
        eklerken yalnızca eylem alanları karşılaştırılır. Kaydedildiği andaki değerler sayılır; sonradan yapılan düzeltmeler değil.
      </p>
    </section>

    <section class="panel" :class="{ dim: loading }">
      <div class="panel-head">
        <h2 class="panel-title">İlgi kontrolü ve kararlar</h2>
        <n-select v-if="versionOptions.length" v-model:value="relevanceVersion" :options="versionOptions" clearable placeholder="Tüm sürümler" size="small" class="filter" aria-label="İstem sürümü" />
      </div>
      <n-alert v-if="report.relevance.error" type="warning" :show-icon="false">{{ report.relevance.error }}</n-alert>
      <template v-else>
        <p class="small">Bu dönemde gelen ve karar verilen haberler: işleme hattındaki AI’ın kararı (satırlar) ve araştırmacıların kararı (sütunlar).</p>
        <div class="table-wrap">
          <table class="report-table">
            <thead>
              <tr>
                <th>AI</th>
                <th class="num">Vakaya bağlandı</th>
                <th v-for="c in dismissColumns" :key="c.key" class="num">{{ c.label }}</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="r in report.relevance.rows" :key="r.verdict">
                <td>{{ verdictLabels[r.verdict] }}</td>
                <td class="num">{{ r.linked }}</td>
                <td v-for="c in dismissColumns" :key="c.key" class="num">{{ r.dismissed[c.key] ?? 0 }}</td>
              </tr>
            </tbody>
          </table>
        </div>
        <p v-if="report.relevance.truncated" class="small muted">Çok sayıda haber var; yalnızca ilk 3.000 tanesi sayıldı. Daha kısa bir dönem seçin.</p>
        <p class="small muted">“İlgisiz” satırında vakaya bağlanan haberler, AI’ın kaçırdıklarıdır; “İlgili” satırında “Emek hareketiyle ilgisiz” sayılanlar, boşuna incelenenler.</p>
      </template>
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
.filters {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-bottom: 16px;
}
.filter {
  width: 180px;
}
.filter.wide {
  width: 340px;
  max-width: 100%;
}
.state {
  padding: 32px 16px;
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
  align-items: center;
  justify-content: space-between;
  gap: 8px;
}
.panel-title {
  margin: 0;
  font-size: 18px;
}
.dim {
  opacity: 0.6;
}
.small {
  margin: 8px 0;
  font-size: 13px;
}
.muted {
  color: var(--rs-muted);
}
.table-wrap {
  overflow-x: auto;
}
.report-table {
  width: 100%;
  min-width: 640px;
  border-collapse: collapse;
  font-size: 14px;
}
.report-table th {
  text-align: left;
  font-size: 12px;
  color: var(--rs-muted);
  font-weight: 600;
  padding: 8px 10px;
  border-bottom: 1px solid var(--rs-border);
}
.report-table td {
  padding: 7px 10px;
  border-bottom: 1px solid #eef0f2;
}
.num {
  text-align: right;
  font-variant-numeric: tabular-nums;
}
.item td:first-child {
  padding-left: 32px;
  color: var(--rs-muted);
}
.expand {
  border: 0;
  background: none;
  padding: 0;
  font: inherit;
  color: var(--rs-accent);
  cursor: pointer;
}
.share.good {
  color: #1f7a3f;
  font-weight: 600;
}
.share.fair {
  color: #9a6700;
  font-weight: 600;
}
.share.poor {
  color: #b42318;
  font-weight: 600;
}
</style>
