<script setup lang="ts">
import { reactive, ref } from 'vue'
import { NAlert, NButton, NEmpty, NPopconfirm, NSpin, NTag, useMessage } from 'naive-ui'
import { errorMessage } from '@/api'
import {
  getSolidarityLinks,
  markTargetChecked,
  resolveSolidarity,
  type SolidarityAction,
  type SolidarityGroup,
  type SolidarityLinksPage,
} from '@/casesApi'
import CaseSearchSelect from '@/components/CaseSearchSelect.vue'
import { formatDate } from '@/format'

// "Dayanışma bağlantıları" (R31c): before R31 the supported struggle of a Dayanışma case was written into its company
// field. The migration named those cases after it; here a researcher links each group to the supported case, or moves
// the name to the organizers, or keeps it as the workplace whose workers acted, or leaves it.
const message = useMessage()
const page = ref<SolidarityLinksPage | null>(null)
const loadError = ref('')
const busy = ref<string | null>(null)
const expanded = ref<string | null>(null)
// Per group: a case chosen by search instead of the suggestion.
const chosen = reactive<Record<string, number | null>>({})

async function load() {
  loadError.value = ''
  try {
    page.value = await getSolidarityLinks()
  } catch (error) {
    loadError.value = errorMessage(error, 'Liste yüklenemedi.')
  }
}
void load()

async function resolve(group: SolidarityGroup, action: SolidarityAction, supportedCaseId: number | null = null) {
  busy.value = group.name
  try {
    await resolveSolidarity(group.caseIds, action, supportedCaseId)
    message.success(
      action === 'Link' ? `${group.caseIds.length} vaka bağlandı.`
        : action === 'Organizer' ? 'Örgütleyen kurum olarak eklendi.'
        : action === 'Workplace' ? 'Eylemi yapan işyeri olarak kaydedildi.'
        : 'Boş bırakıldı.',
    )
    await load()
  } catch (error) {
    message.error(errorMessage(error, 'Kaydedilemedi.'))
  } finally {
    busy.value = null
  }
}

async function checked(caseId: number) {
  try {
    await markTargetChecked(caseId)
    await load()
  } catch (error) {
    message.error(errorMessage(error, 'Kaydedilemedi.'))
  }
}

function period(g: SolidarityGroup) {
  if (!g.firstDate) return null
  return g.firstDate === g.lastDate ? formatDate(g.firstDate) : `${formatDate(g.firstDate)} – ${formatDate(g.lastDate)}`
}
</script>

<template>
  <p class="back"><router-link to="/cases">← Vakalar</router-link></p>
  <h1 class="heading">Dayanışma bağlantıları</h1>
  <p class="description">
    Eskiden dayanışma vakalarında desteklenen mücadele “şirket” alanına yazılıyordu; bu vakalar o adla adlandırıldı. Aynı
    adı taşıyanlar bir grup: grubu bir kez bağlamak hepsini bağlar. Ad aslında eylemi örgütleyen kurumsa “Örgütleyen kurum”,
    eylemi yapan işçilerin işyeriyse “Eylemi yapan işyeri”.
  </p>

  <n-alert v-if="loadError" type="error" :show-icon="false">
    {{ loadError }} <n-button size="small" @click="load">Tekrar dene</n-button>
  </n-alert>
  <div v-else-if="!page" class="state"><n-spin /></div>

  <template v-else>
    <p class="summary">
      {{ page.groups.length }} grup, {{ page.groups.reduce((n, g) => n + g.caseIds.length, 0) }} vaka bekliyor.
      <template v-if="page.unlinkedNewCases"> Sonradan girilmiş {{ page.unlinkedNewCases }} dayanışma vakası da henüz bağlanmamış (vaka sayfalarından bağlanır).</template>
    </p>
    <n-empty v-if="page.groups.length === 0" description="Bağlanacak eski dayanışma vakası kalmadı." class="state" />

    <article v-for="g in page.groups" :key="g.name" class="group">
      <header class="group-head">
        <h2 class="group-name">{{ g.name ? `“${g.name}”` : 'Adı olmayan' }}</h2>
        <span class="muted">
          {{ g.caseIds.length }} vaka<template v-if="g.cities.length"> · {{ g.cities.join(', ') }}</template><template v-if="period(g)"> · {{ period(g) }}</template>
        </span>
        <n-tag v-if="g.oldCompanyUsedByIsletme" size="small" :bordered="false">gerçek bir işyeri</n-tag>
        <n-tag v-if="g.looksLikeOrganization" size="small" type="info" :bordered="false">bir kurumun adı</n-tag>
      </header>

      <div class="row">
        <template v-if="g.suggestion">
          <span class="muted">Önerilen vaka:</span>
          <router-link :to="`/cases/${g.suggestion.caseId}`" class="suggestion" target="_blank">
            <strong>{{ g.suggestion.name || `Vaka #${g.suggestion.caseId}` }}</strong>
            <span class="muted"> · {{ g.suggestion.reason }} · {{ g.suggestion.ongoing ? 'devam ediyor' : `son eylem ${formatDate(g.suggestion.lastProtestDate)}` }}</span>
          </router-link>
          <n-button size="small" type="primary" :loading="busy === g.name" :disabled="busy !== null" @click="resolve(g, 'Link', g.suggestion.caseId)">Grubu bağla</n-button>
        </template>
        <span v-else class="muted">Önerilecek vaka bulunamadı.</span>
      </div>

      <div class="row">
        <case-search-select v-model="chosen[g.name]" :exclude="g.caseIds" class="search" placeholder="Başka vaka ara ve bağla" />
        <n-button size="small" :disabled="!chosen[g.name] || busy !== null" @click="resolve(g, 'Link', chosen[g.name] ?? null)">Bu vakaya bağla</n-button>
      </div>

      <div class="row">
        <n-button size="small" :disabled="busy !== null || !g.name" @click="resolve(g, 'Organizer')">Örgütleyen kurum</n-button>
        <n-button v-if="g.oldCompanyId" size="small" :disabled="busy !== null" @click="resolve(g, 'Workplace')">Eylemi yapan işyeri</n-button>
        <n-popconfirm positive-text="Boş bırak" negative-text="Vazgeç" @positive-click="resolve(g, 'None')">
          <template #trigger><n-button size="small" quaternary :disabled="busy !== null">Boş bırak</n-button></template>
          Desteklenen vaka boş kalır; vakalar sonra tek tek vaka sayfasından bağlanabilir.
        </n-popconfirm>
        <n-button size="small" quaternary @click="expanded = expanded === g.name ? null : g.name">{{ expanded === g.name ? 'Vakaları gizle' : 'Vakaları gör' }}</n-button>
      </div>
      <ul v-if="expanded === g.name" class="cases">
        <li v-for="id in g.caseIds" :key="id"><router-link :to="`/cases/${id}`" target="_blank">Vaka #{{ id }}</router-link></li>
      </ul>
    </article>

    <section v-if="page.targetChecks.length" class="group">
      <h2 class="group-name">İşletme vakaları: eski “hedef kurum”</h2>
      <p class="muted small">Bu İşletme vakalarında “Kime karşı: Kurum” seçilmişti. Vakayı açıp kurumun yerini (kurumsallık, işyeri …) kontrol edin.</p>
      <ul class="checks">
        <li v-for="c in page.targetChecks" :key="c.caseId">
          <router-link :to="`/cases/${c.caseId}`" target="_blank">{{ c.name || c.companyName || `Vaka #${c.caseId}` }}</router-link>
          <span class="muted"> · eski hedef kurum: {{ c.oldTargetName ?? '—' }}</span>
          <n-button size="tiny" @click="checked(c.caseId)">Kontrol edildi</n-button>
        </li>
      </ul>
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
  margin: 6px 0 16px;
  color: var(--rs-muted);
  font-size: 15px;
  max-width: 820px;
}
.summary {
  margin: 0 0 12px;
  font-size: 14px;
}
.state {
  padding: 32px 16px;
}
.group {
  background: var(--rs-surface);
  border: 1px solid var(--rs-border);
  border-radius: 10px;
  padding: 14px 18px;
  margin-bottom: 12px;
  display: flex;
  flex-direction: column;
  gap: 10px;
}
.group-head {
  display: flex;
  flex-wrap: wrap;
  align-items: baseline;
  gap: 6px 12px;
}
.group-name {
  margin: 0;
  font-size: 17px;
}
.row {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 8px;
  font-size: 14px;
}
.suggestion {
  color: var(--rs-ink);
  text-decoration: none;
  border: 1px solid var(--rs-accent);
  background: #f0f7f7;
  border-radius: 6px;
  padding: 4px 10px;
}
.search {
  flex: 1 1 320px;
  max-width: 520px;
}
.muted {
  color: var(--rs-muted);
}
.small {
  font-size: 13px;
  margin: 0;
}
.cases,
.checks {
  margin: 0;
  padding-left: 18px;
  font-size: 14px;
  display: flex;
  flex-direction: column;
  gap: 6px;
}
.checks li > * + * {
  margin-left: 8px;
}
</style>
