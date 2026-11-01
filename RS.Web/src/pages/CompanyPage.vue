<script setup lang="ts">
import { turkishFilter } from '@/search'
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { NAlert, NButton, NDatePicker, NFormItem, NInput, NInputNumber, NModal, NPopconfirm, NSelect, NSpin, useMessage } from 'naive-ui'
import { ApiError, errorMessage } from '@/api'
import CompanyPicker from '@/components/CompanyPicker.vue'
import MergeDialog from '@/components/MergeDialog.vue'
import {
  addCompanyRelation,
  addWorkplace,
  changeCompanyRelation,
  deleteDefinition,
  getCompany,
  getDefinitionOptions,
  institutionKindOptions,
  InstitutionKind,
  relationChoices,
  removeCompanyRelation,
  updateDefinition,
  type CompanyDetail,
  type CompanyRelationItem,
  type CompanyWorkplace,
  type DefinitionItem,
  type DefinitionOptions,
  type Option,
} from '@/definitionsApi'
import { formatDate } from '@/format'
import { EmployerRole, getFormOptions, type DistrictOption } from '@/reviewApi'

// One company (R12a-3, R37): its fields, its facts with other companies, its links from cases, and its cases.
const route = useRoute()
const router = useRouter()
const message = useMessage()
const id = computed(() => Number(route.params.id))

const company = ref<CompanyDetail | null>(null)
const loadError = ref('')
const options = ref<DefinitionOptions>({ corporationTypes: [], companyTypes: [], companyScales: [], companyWorklines: [], confederations: [], serviceBranches: [] })
void getDefinitionOptions().then((o) => (options.value = o))
const toSelect = (list: Option[] | null) => (list ?? []).map((o) => ({ label: o.name, value: o.id }))

// The editable fields, saved together.
const emptyForm = () => ({
  name: '',
  typeId: null as number | null,
  institutionKind: null as InstitutionKind | null,
  scaleId: null as number | null,
  worklineId: null as number | null,
  serviceBranchId: null as number | null,
})
const form = ref(emptyForm())
const saving = ref(false)

async function load() {
  loadError.value = ''
  try {
    const c = await getCompany(id.value)
    company.value = c
    form.value = { name: c.name, typeId: c.typeId, institutionKind: c.institutionKind, scaleId: c.scaleId, worklineId: c.worklineId, serviceBranchId: c.serviceBranchId }
  } catch (error) {
    loadError.value = error instanceof ApiError && error.status === 404 ? 'Şirket bulunamadı (silinmiş ya da başka bir şirketle birleştirilmiş olabilir).' : errorMessage(error, 'Şirket yüklenemedi.')
  }
}
watch(id, load, { immediate: true })

const asItem = computed<DefinitionItem | null>(() =>
  company.value
    ? {
        id: company.value.id, name: company.value.name, approved: null, usage: company.value.cases.length, typeId: form.value.typeId,
        againstProduction: null, simple: null, scaleId: form.value.scaleId, worklineId: form.value.worklineId, confederationId: null,
        institutionKind: form.value.institutionKind, serviceBranchId: form.value.serviceBranchId,
      }
    : null,
)
const changed = computed(() => {
  const c = company.value
  if (!c) return false
  const f = form.value
  return f.name.trim() !== c.name || f.typeId !== c.typeId || f.institutionKind !== c.institutionKind || f.scaleId !== c.scaleId
    || f.worklineId !== c.worklineId || f.serviceBranchId !== c.serviceBranchId
})

async function save() {
  if (!asItem.value) return
  saving.value = true
  try {
    await updateDefinition('companies', { ...asItem.value, name: form.value.name })
    message.success('Kaydedildi.')
    await load()
  } catch (error) {
    if (error instanceof ApiError && error.status === 409 && typeof error.details?.existingId === 'number') {
      mergeSuggestion.value = { id: error.details.existingId, name: String(error.details.existingName) }
      mergeFrom.value = asItem.value
    }
    message.error(errorMessage(error, 'Kaydedilemedi.'))
  } finally {
    saving.value = false
  }
}

// ---- Facts with other companies (R37, part B) ----

// The page's company type puts the likely relation first: a holding has group companies, a public body dependent ones.
const orderedChoices = computed(() => {
  const k = company.value?.institutionKind
  const first =
    k === InstitutionKind.Holding ? [1] :
    k === InstitutionKind.PublicBody || k === InstitutionKind.Municipality || k === InstitutionKind.StateEnterprise ? [3] :
    [0, 2]
  return [...first, ...relationChoices.map((_, i) => i).filter((i) => !first.includes(i))].map((i) => relationChoices[i])
})
const choiceOptions = computed(() => orderedChoices.value.map((c) => ({ label: c.label, value: relationChoices.indexOf(c) })))
const relationGroups = computed(() =>
  relationChoices
    .map((choice) => ({ choice, items: (company.value?.relations ?? []).filter((r) => r.kind === choice.kind && r.outgoing === choice.outgoing) }))
    .filter((g) => g.items.length > 0),
)

function period(from: string | null, to: string | null) {
  if (!from && !to) return null
  return `${from ? formatDate(from) : '?'} – ${to ? formatDate(to) : 'bugün'}`
}

// The add / change dialog.
const editing = ref(false)
const editingId = ref<number | null>(null)
const draft = ref({
  choice: 0,
  otherId: null as number | null,
  otherName: null as string | null,
  share: null as number | null,
  start: null as number | null,
  end: null as number | null,
  source: '',
  note: '',
})
const draftError = ref('')
const savingRelation = ref(false)
const draftChoice = computed(() => relationChoices[draft.value.choice])

const toDay = (t: number | null) => {
  if (t === null) return null
  const d = new Date(t)
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
}
const fromDay = (s: string | null) => {
  const m = s ? /^(\d{4})-(\d{2})-(\d{2})/.exec(s) : null
  return m ? new Date(Number(m[1]), Number(m[2]) - 1, Number(m[3])).getTime() : null
}

function openAdd() {
  editingId.value = null
  draft.value = { choice: relationChoices.indexOf(orderedChoices.value[0]), otherId: null, otherName: null, share: null, start: null, end: null, source: '', note: '' }
  draftError.value = ''
  editing.value = true
}

function openChange(r: CompanyRelationItem) {
  editingId.value = r.id
  draft.value = {
    choice: relationChoices.findIndex((c) => c.kind === r.kind && c.outgoing === r.outgoing),
    otherId: r.otherId, otherName: r.otherName, share: r.sharePercent, start: fromDay(r.startDate), end: fromDay(r.endDate),
    source: r.sourceUrl ?? '', note: r.note ?? '',
  }
  draftError.value = ''
  editing.value = true
}

async function saveRelation() {
  const d = draft.value
  if (d.otherId === null) {
    draftError.value = 'Diğer şirketi seçin.'
    return
  }
  const input = {
    kind: draftChoice.value.kind, otherCompanyId: d.otherId, thisIsFirst: draftChoice.value.outgoing,
    sharePercent: draftChoice.value.kind === 1 ? d.share : null, startDate: toDay(d.start), endDate: toDay(d.end),
    sourceUrl: d.source.trim() || null, note: d.note.trim() || null,
  }
  savingRelation.value = true
  draftError.value = ''
  try {
    if (editingId.value === null) await addCompanyRelation(id.value, input)
    else await changeCompanyRelation(id.value, editingId.value, input)
    editing.value = false
    message.success('İlişki kaydedildi.')
    await load()
  } catch (error) {
    draftError.value = errorMessage(error, 'İlişki kaydedilemedi.')
  } finally {
    savingRelation.value = false
  }
}

async function removeRelation(r: CompanyRelationItem) {
  try {
    await removeCompanyRelation(id.value, r.id)
    await load()
  } catch (error) {
    message.error(errorMessage(error, 'İlişki kaldırılamadı.'))
  }
}

// ---- Workplaces (R37): its own, its group companies' (a holding), and those its cases name ----
const cities = ref<Option[]>([])
const districts = ref<DistrictOption[]>([])
void getFormOptions().then((o) => {
  cities.value = o.cities
  districts.value = o.districts
})
const workplaceGroups = computed(() =>
  (
    [
      ['own', 'Bu şirkete ait'],
      ['group', 'Grup şirketlerine ait'],
      ['cases', 'Vakalarında geçen (başkasına ait ya da sahibi girilmemiş)'],
    ] as const
  )
    .map(([source, title]) => ({ title, items: (company.value?.workplaces ?? []).filter((w) => w.source === source) }))
    .filter((g) => g.items.length > 0),
)

const placeEditing = ref(false)
const placeId = ref<number | null>(null)
const placeDraft = ref({ name: '', cityId: null as number | null, districtId: null as number | null, ownerId: null as number | null, ownerName: null as string | null })
const placeError = ref('')
const savingPlace = ref(false)
const placeDistricts = computed(() => districts.value.filter((d) => d.cityId === placeDraft.value.cityId))

function openPlace(w: CompanyWorkplace | null) {
  placeId.value = w?.id ?? null
  placeDraft.value = w
    ? { name: w.name, cityId: w.cityId, districtId: w.districtId, ownerId: w.ownerCompanyId, ownerName: w.ownerName }
    : { name: '', cityId: null, districtId: null, ownerId: company.value?.id ?? null, ownerName: company.value?.name ?? null }
  placeError.value = ''
  placeEditing.value = true
}
function onPlaceCity(cityId: number | null) {
  placeDraft.value.cityId = cityId
  if (!placeDistricts.value.some((d) => d.id === placeDraft.value.districtId)) placeDraft.value.districtId = null
}

async function savePlace() {
  const d = placeDraft.value
  if (!d.name.trim()) {
    placeError.value = 'İşyerinin adını girin.'
    return
  }
  savingPlace.value = true
  placeError.value = ''
  try {
    if (placeId.value === null) {
      const result = await addWorkplace({ name: d.name, cityId: d.cityId, districtId: d.districtId, ownerCompanyId: d.ownerId })
      if (result.existed) message.info(`“${result.workplace.name}” zaten kayıtlıydı.`)
    } else {
      await updateDefinition('workplaces', {
        id: placeId.value, name: d.name, approved: null, usage: 0, typeId: null, againstProduction: null, simple: null, scaleId: null,
        worklineId: null, confederationId: null, cityId: d.cityId, districtId: d.districtId, ownerCompanyId: d.ownerId,
      })
    }
    placeEditing.value = false
    message.success('İşyeri kaydedildi.')
    await load()
  } catch (error) {
    placeError.value = errorMessage(error, 'İşyeri kaydedilemedi.')
  } finally {
    savingPlace.value = false
  }
}

// ---- Links read from the cases (R37, decision b) ----
const roleTitles: Record<number, [string, string]> = {
  [EmployerRole.Subcontractor]: ['Taşeron olarak çalıştığı', 'Taşeronları'],
  [EmployerRole.Contractor]: ['Yüklenicisi olduğu işlerin sahibi', 'Yüklenicileri'],
  [EmployerRole.Operator]: ['İşlettiği yerlerin sahibi', 'İşletmecileri'],
}
const jobGroups = computed(() => {
  const groups = new Map<string, NonNullable<typeof company.value>['jobLinks']>()
  for (const link of company.value?.jobLinks ?? []) {
    const title = roleTitles[link.role]?.[link.asEmployer ? 0 : 1] ?? 'Diğer'
    groups.set(title, [...(groups.get(title) ?? []), link])
  }
  return [...groups.entries()].map(([title, links]) => ({ title, links }))
})

async function remove() {
  try {
    await deleteDefinition('companies', id.value)
    message.success('Şirket silindi.')
    void router.push('/definitions/companies')
  } catch (error) {
    message.error(errorMessage(error, 'Silinemedi.'))
  }
}

const mergeFrom = ref<DefinitionItem | null>(null)
const mergeSuggestion = ref<{ id: number; name: string } | null>(null)

function startMerge() {
  mergeSuggestion.value = null
  mergeFrom.value = asItem.value
}

// This company is gone now; continue on the one it was merged into.
function onMerged(affected: number, intoId: number) {
  message.success(`Birleştirildi: ${affected} vaka güncellendi.`)
  mergeFrom.value = null
  void router.push(`/definitions/companies/${intoId}`)
}
</script>

<template>
  <p class="back"><router-link to="/definitions/companies">← Şirketler</router-link></p>

  <n-alert v-if="loadError" type="error" :show-icon="false">{{ loadError }}</n-alert>
  <div v-else-if="!company" class="state"><n-spin /></div>

  <template v-else>
    <div class="head">
      <h1 class="heading">{{ company.name }}</h1>
      <div class="head-actions">
        <n-button @click="startMerge">Birleştir</n-button>
        <n-popconfirm v-if="company.cases.length === 0" positive-text="Sil" negative-text="Vazgeç" @positive-click="remove">
          <template #trigger><n-button type="error" ghost>Sil</n-button></template>
          “{{ company.name }}” silinsin mi?
        </n-popconfirm>
      </div>
    </div>

    <section class="panel">
      <h2 class="panel-title">Bilgiler</h2>
      <div class="fields">
        <n-form-item label="Ad" label-placement="top" :show-feedback="false" class="wide">
          <n-input v-model:value="form.name" :input-props="{ id: 'company-name' }" />
        </n-form-item>
        <n-form-item label="Kurum türü" label-placement="top" :show-feedback="false">
          <n-select v-model:value="form.institutionKind" :options="institutionKindOptions" clearable placeholder="Seçiniz" />
        </n-form-item>
        <n-form-item label="Sahiplik" label-placement="top" :show-feedback="false">
          <n-select v-model:value="form.typeId" :options="toSelect(options.companyTypes)" clearable placeholder="Seçiniz" />
        </n-form-item>
        <n-form-item label="Büyüklük" label-placement="top" :show-feedback="false">
          <n-select v-model:value="form.scaleId" :options="toSelect(options.companyScales)" clearable placeholder="Seçiniz" />
        </n-form-item>
        <n-form-item label="İşkolu (işçi)" label-placement="top" :show-feedback="false">
          <n-select v-model:value="form.worklineId" :options="toSelect(options.companyWorklines)" clearable filterable :filter="turkishFilter" placeholder="Seçiniz" />
        </n-form-item>
        <n-form-item label="Hizmet kolu (memur)" label-placement="top" :show-feedback="false">
          <n-select v-model:value="form.serviceBranchId" :options="toSelect(options.serviceBranches)" clearable filterable :filter="turkishFilter" placeholder="Kamu kurumları için" />
        </n-form-item>
      </div>
      <div class="save">
        <n-button type="primary" :disabled="!changed || !form.name.trim()" :loading="saving" @click="save">Kaydet</n-button>
      </div>
    </section>

    <section class="panel">
      <div class="panel-head">
        <h2 class="panel-title">Şirkete dair ilişkiler</h2>
        <n-button size="small" @click="openAdd">İlişki ekle</n-button>
      </div>
      <p class="hint">Her vakada geçerli bilgiler: grubu, bağlı olduğu kamu kurumu, ad değişikliği. Tarih ve kaynakla bir kez girilir; vakalarda işverenin altında görünür.</p>
      <p v-if="relationGroups.length === 0" class="empty">Henüz yok.</p>
      <div v-for="g in relationGroups" :key="g.choice.listTitle" class="group">
        <h3 class="group-title">{{ g.choice.listTitle }}</h3>
        <ul class="links">
          <li v-for="r in g.items" :key="r.id">
            <div class="link-text">
              <router-link :to="`/definitions/companies/${r.otherId}`">{{ r.otherName }}</router-link>
              <span v-if="r.sharePercent !== null" class="meta">%{{ r.sharePercent }}</span>
              <span v-if="period(r.startDate, r.endDate)" class="meta">{{ period(r.startDate, r.endDate) }}</span>
              <a v-if="r.sourceUrl" :href="r.sourceUrl" target="_blank" rel="noopener" class="meta">kaynak ↗</a>
              <span v-if="r.note" class="meta note">{{ r.note }}</span>
            </div>
            <div class="link-actions">
              <n-button size="tiny" quaternary @click="openChange(r)">Düzenle</n-button>
              <n-popconfirm positive-text="Kaldır" negative-text="Vazgeç" @positive-click="removeRelation(r)">
                <template #trigger><n-button size="tiny" quaternary>Kaldır</n-button></template>
                Bu ilişki kaldırılsın mı?
              </n-popconfirm>
            </div>
          </li>
        </ul>
      </div>
    </section>

    <section class="panel">
      <div class="panel-head">
        <h2 class="panel-title">İşyerleri</h2>
        <n-button size="small" @click="openPlace(null)">İşyeri ekle</n-button>
      </div>
      <p class="hint">Yerler: fabrika, hastane, şantiye, maden … Bir işyerinin kime ait olduğunu buradan değiştirebilirsiniz.</p>
      <p v-if="workplaceGroups.length === 0" class="empty">Yok.</p>
      <div v-for="g in workplaceGroups" :key="g.title" class="group">
        <h3 class="group-title">{{ g.title }}</h3>
        <ul class="links">
          <li v-for="w in g.items" :key="w.id">
            <div class="link-text">
              <span>{{ w.name }}</span>
              <span v-if="w.placeName" class="meta">{{ w.placeName }}</span>
              <span v-if="w.source !== 'own'" class="meta">
                <template v-if="w.ownerCompanyId">kime ait: <router-link :to="`/definitions/companies/${w.ownerCompanyId}`">{{ w.ownerName }}</router-link></template>
                <template v-else>kime ait: girilmemiş</template>
              </span>
              <span class="meta">{{ w.cases }} vaka</span>
              <span v-if="!w.approved" class="meta pending">onay bekliyor</span>
            </div>
            <div class="link-actions">
              <n-button size="tiny" quaternary @click="openPlace(w)">Düzenle</n-button>
            </div>
          </li>
        </ul>
      </div>
    </section>

    <section class="panel">
      <h2 class="panel-title">Vakalardan</h2>
      <p class="hint">Vakaların “İşveren Durumu”ndan (taşeron, yüklenici, işletmeci); burada girilmez. Düzeltmek için vakayı açın.</p>
      <p v-if="jobGroups.length === 0" class="empty">Yok.</p>
      <div v-for="g in jobGroups" :key="g.title" class="group">
        <h3 class="group-title">{{ g.title }}</h3>
        <ul class="links">
          <li v-for="l in g.links" :key="`${l.role}-${l.otherId}`">
            <div class="link-text">
              <router-link v-if="l.otherId" :to="`/definitions/companies/${l.otherId}`">{{ l.otherName }}</router-link>
              <span v-else>{{ l.otherName }}</span>
              <span class="meta">{{ l.cases }} vaka</span>
              <span v-if="period(l.first, l.last)" class="meta">{{ period(l.first, l.last) }}</span>
            </div>
          </li>
        </ul>
      </div>
    </section>

    <section class="panel">
      <h2 class="panel-title">Vakalar ({{ company.cases.length }})</h2>
      <ul v-if="company.cases.length" class="cases">
        <li v-for="c in company.cases" :key="c.id">
          <span class="date">{{ formatDate(c.startDate) }}</span>
          <router-link :to="`/cases/${c.id}`">{{ c.name || `Vaka ${c.id}` }}</router-link>
          <span v-if="c.asMainCompany" class="role">işverenin üstündeki kurum olarak</span>
        </li>
      </ul>
      <p v-else class="empty">Bu şirketin vakası yok.</p>
    </section>
  </template>

  <n-modal
    v-model:show="editing"
    preset="card"
    :title="editingId === null ? 'İlişki ekle' : 'İlişkiyi düzenle'"
    :style="{ width: 'min(560px, calc(100vw - 32px))' }"
    :mask-closable="!savingRelation"
  >
    <n-form-item label="İlişki" label-placement="top" :show-feedback="false">
      <n-select
        v-model:value="draft.choice"
        :options="choiceOptions"
        :disabled="editingId !== null"
      />
    </n-form-item>
    <n-form-item label="Şirket ya da kurum" label-placement="top" :show-feedback="false">
      <company-picker
        v-if="editingId === null"
        v-model="draft.otherId"
        input-id="relation-company"
        placeholder="Adıyla arayın"
      />
      <span v-else>{{ draft.otherName }}</span>
    </n-form-item>
    <n-form-item v-if="draftChoice.kind === 1" label="Pay (%)" label-placement="top" :show-feedback="false">
      <n-input-number v-model:value="draft.share" :min="0" :max="100" clearable placeholder="Bilinmiyorsa boş" />
    </n-form-item>
    <div class="dates">
      <n-form-item :label="draftChoice.kind === 3 ? 'Ad değişikliği tarihi' : 'Başlangıç'" label-placement="top" :show-feedback="false">
        <n-date-picker v-model:value="draft.start" type="date" format="dd.MM.yyyy" clearable />
      </n-form-item>
      <n-form-item v-if="draftChoice.kind !== 3" label="Bitiş" label-placement="top" :show-feedback="false">
        <n-date-picker v-model:value="draft.end" type="date" format="dd.MM.yyyy" clearable placeholder="Sürüyorsa boş" />
      </n-form-item>
    </div>
    <n-form-item label="Kaynak bağlantısı" label-placement="top" :show-feedback="false">
      <n-input v-model:value="draft.source" placeholder="https://…" />
    </n-form-item>
    <n-form-item label="Not" label-placement="top" :validation-status="draftError ? 'error' : undefined" :feedback="draftError">
      <n-input v-model:value="draft.note" type="textarea" :autosize="{ minRows: 1, maxRows: 4 }" />
    </n-form-item>
    <template #footer>
      <div class="actions">
        <n-button :disabled="savingRelation" @click="editing = false">Vazgeç</n-button>
        <n-button type="primary" :loading="savingRelation" @click="saveRelation">Kaydet</n-button>
      </div>
    </template>
  </n-modal>

  <n-modal
    v-model:show="placeEditing"
    preset="card"
    :title="placeId === null ? 'İşyeri ekle' : 'İşyerini düzenle'"
    :style="{ width: 'min(520px, calc(100vw - 32px))' }"
    :mask-closable="!savingPlace"
  >
    <n-form-item label="İşyerinin adı" label-placement="top" :show-feedback="false">
      <n-input v-model:value="placeDraft.name" placeholder="Örn. Gölcük fabrikası" />
    </n-form-item>
    <div class="dates">
      <n-form-item label="İl" label-placement="top" :show-feedback="false">
        <n-select :value="placeDraft.cityId" :options="toSelect(cities)" filterable :filter="turkishFilter" clearable @update:value="onPlaceCity" />
      </n-form-item>
      <n-form-item label="İlçe" label-placement="top" :show-feedback="false">
        <n-select v-model:value="placeDraft.districtId" :options="toSelect(placeDistricts)" :disabled="!placeDraft.cityId" filterable :filter="turkishFilter" clearable />
      </n-form-item>
    </div>
    <n-form-item label="Kime ait" label-placement="top" :validation-status="placeError ? 'error' : undefined" :feedback="placeError || 'İşyerinin sahibi olan şirket ya da kurum (ör. hastane için üniversite ya da bakanlık).'">
      <company-picker v-model="placeDraft.ownerId" input-id="workplace-owner" :selected-name="placeDraft.ownerName" placeholder="Adıyla arayın" />
    </n-form-item>
    <template #footer>
      <div class="actions">
        <n-button :disabled="savingPlace" @click="placeEditing = false">Vazgeç</n-button>
        <n-button type="primary" :loading="savingPlace" @click="savePlace">Kaydet</n-button>
      </div>
    </template>
  </n-modal>

  <merge-dialog kind="companies" :from="mergeFrom" :suggested-into="mergeSuggestion" unit="vaka" @merged="onMerged" @close="mergeFrom = null" />
</template>

<style scoped>
.back {
  margin: 0 0 8px;
  font-size: 14px;
}
.back a {
  color: var(--rs-accent);
}
.state {
  padding: 40px 16px;
}
.head {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 16px;
}
.heading {
  margin: 0;
  font-size: 28px;
}
.head-actions {
  display: flex;
  gap: 8px;
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
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}
.panel-title {
  margin: 0 0 8px;
  font-size: 17px;
}
.fields {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 12px;
}
.wide {
  grid-column: 1 / -1;
}
.save {
  display: flex;
  justify-content: flex-end;
  margin-top: 12px;
}
.hint {
  margin: 0 0 10px;
  font-size: 13px;
  color: var(--rs-muted);
}
.group {
  margin-bottom: 10px;
}
.group-title {
  margin: 0 0 4px;
  font-size: 13px;
  font-weight: 600;
  color: var(--rs-muted);
  text-transform: uppercase;
  letter-spacing: 0.04em;
}
.links,
.cases {
  list-style: none;
  margin: 0 0 8px;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 6px;
  font-size: 15px;
}
.links li {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 4px 8px;
}
.link-text {
  display: flex;
  flex-wrap: wrap;
  align-items: baseline;
  gap: 4px 10px;
  min-width: 0;
}
.link-actions {
  display: flex;
  gap: 4px;
}
.links a,
.cases a {
  color: var(--rs-ink);
}
.meta {
  font-size: 13px;
  color: var(--rs-muted);
}
a.meta {
  color: var(--rs-accent);
}
.note {
  font-style: italic;
}
.pending {
  color: #92400e;
}
.cases li {
  display: flex;
  flex-wrap: wrap;
  gap: 4px 12px;
  align-items: baseline;
}
.date {
  min-width: 100px;
  color: var(--rs-muted);
  font-variant-numeric: tabular-nums;
}
.role {
  font-size: 13px;
  color: var(--rs-muted);
}
.empty {
  margin: 0 0 8px;
  color: var(--rs-muted);
}
.dates {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
  gap: 12px;
}
.actions {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
}
</style>
