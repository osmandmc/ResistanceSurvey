<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { NAlert, NButton, NFormItem, NInput, NPopconfirm, NSelect, NSpin, useMessage } from 'naive-ui'
import { ApiError, errorMessage } from '@/api'
import CompanyPicker from '@/components/CompanyPicker.vue'
import MergeDialog from '@/components/MergeDialog.vue'
import {
  addCompanyLink,
  deleteDefinition,
  getCompany,
  getDefinitionOptions,
  removeCompanyLink,
  updateDefinition,
  type CompanyDetail,
  type DefinitionItem,
  type DefinitionOptions,
  type Option,
} from '@/definitionsApi'
import { formatDate } from '@/format'

// One company (R12a-3): its fields, its cases and its subcontractor relations.
const route = useRoute()
const router = useRouter()
const message = useMessage()
const id = computed(() => Number(route.params.id))

const company = ref<CompanyDetail | null>(null)
const loadError = ref('')
const options = ref<DefinitionOptions>({ corporationTypes: [], companyTypes: [], companyScales: [], companyWorklines: [], confederations: [] })
void getDefinitionOptions().then((o) => (options.value = o))
const toSelect = (list: Option[]) => list.map((o) => ({ label: o.name, value: o.id }))

// The editable fields, saved together.
const form = ref({ name: '', typeId: null as number | null, scaleId: null as number | null, worklineId: null as number | null })
const saving = ref(false)

async function load() {
  loadError.value = ''
  try {
    company.value = await getCompany(id.value)
    form.value = { name: company.value.name, typeId: company.value.typeId, scaleId: company.value.scaleId, worklineId: company.value.worklineId }
  } catch (error) {
    loadError.value = error instanceof ApiError && error.status === 404 ? 'Şirket bulunamadı (silinmiş ya da başka bir şirketle birleştirilmiş olabilir).' : errorMessage(error, 'Şirket yüklenemedi.')
  }
}
watch(id, load, { immediate: true })

const asItem = computed<DefinitionItem | null>(() =>
  company.value
    ? { id: company.value.id, name: company.value.name, approved: null, usage: company.value.cases.length, typeId: form.value.typeId, againstProduction: null, simple: null, scaleId: form.value.scaleId, worklineId: form.value.worklineId, confederationId: null }
    : null,
)
const changed = computed(
  () =>
    !!company.value &&
    (form.value.name.trim() !== company.value.name ||
      form.value.typeId !== company.value.typeId ||
      form.value.scaleId !== company.value.scaleId ||
      form.value.worklineId !== company.value.worklineId),
)

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

// Subcontractor relations: pick a company to add.
const newMain = ref<number | null>(null)
const newSub = ref<number | null>(null)

async function addLink(relation: 'subcontractors' | 'main-companies', other: number | null) {
  if (other === null) return
  try {
    await addCompanyLink(id.value, relation, other)
    newMain.value = null
    newSub.value = null
    await load()
  } catch (error) {
    message.error(errorMessage(error, 'İlişki eklenemedi.'))
  }
}
watch(newMain, (value) => void addLink('main-companies', value))
watch(newSub, (value) => void addLink('subcontractors', value))

async function removeLink(relation: 'subcontractors' | 'main-companies', other: number) {
  try {
    await removeCompanyLink(id.value, relation, other)
    await load()
  } catch (error) {
    message.error(errorMessage(error, 'İlişki kaldırılamadı.'))
  }
}

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
          “{{ company.name }}” silinsin mi? Taşeron ilişkileri de kalkar.
        </n-popconfirm>
      </div>
    </div>

    <section class="panel">
      <h2 class="panel-title">Bilgiler</h2>
      <div class="fields">
        <n-form-item label="Ad" label-placement="top" :show-feedback="false" class="wide">
          <n-input v-model:value="form.name" :input-props="{ id: 'company-name' }" />
        </n-form-item>
        <n-form-item label="Tür" label-placement="top" :show-feedback="false">
          <n-select v-model:value="form.typeId" :options="toSelect(options.companyTypes)" clearable />
        </n-form-item>
        <n-form-item label="Ölçek" label-placement="top" :show-feedback="false">
          <n-select v-model:value="form.scaleId" :options="toSelect(options.companyScales)" clearable />
        </n-form-item>
        <n-form-item label="İş kolu" label-placement="top" :show-feedback="false">
          <n-select v-model:value="form.worklineId" :options="toSelect(options.companyWorklines)" clearable filterable />
        </n-form-item>
      </div>
      <div class="save">
        <n-button type="primary" :disabled="!changed || !form.name.trim()" :loading="saving" @click="save">Kaydet</n-button>
      </div>
    </section>

    <div class="columns">
      <section class="panel">
        <h2 class="panel-title">Taşeronu olduğu şirketler</h2>
        <p class="hint">Bu şirket, aşağıdaki şirketlerin taşeronu olarak çalışıyor.</p>
        <ul v-if="company.mainCompanies.length" class="links">
          <li v-for="c in company.mainCompanies" :key="c.id">
            <router-link :to="`/definitions/companies/${c.id}`">{{ c.name }}</router-link>
            <n-button size="tiny" quaternary @click="removeLink('main-companies', c.id)">Kaldır</n-button>
          </li>
        </ul>
        <p v-else class="empty">Yok.</p>
        <company-picker v-model="newMain" input-id="add-main-company" placeholder="Ana şirket ekle…" class="picker" />
      </section>

      <section class="panel">
        <h2 class="panel-title">Taşeronları</h2>
        <p class="hint">Aşağıdaki şirketler bu şirketin taşeronu.</p>
        <ul v-if="company.subcontractors.length" class="links">
          <li v-for="c in company.subcontractors" :key="c.id">
            <router-link :to="`/definitions/companies/${c.id}`">{{ c.name }}</router-link>
            <n-button size="tiny" quaternary @click="removeLink('subcontractors', c.id)">Kaldır</n-button>
          </li>
        </ul>
        <p v-else class="empty">Yok.</p>
        <company-picker v-model="newSub" input-id="add-subcontractor" placeholder="Taşeron ekle…" class="picker" />
      </section>
    </div>

    <section class="panel">
      <h2 class="panel-title">Vakalar ({{ company.cases.length }})</h2>
      <p class="hint">Bir ilişkiyi kaldırmak geçmiş vakaları değiştirmez; vakalar ana şirketi kayıt anındaki haliyle tutar.</p>
      <ul v-if="company.cases.length" class="cases">
        <li v-for="c in company.cases" :key="c.id">
          <span class="date">{{ formatDate(c.startDate) }}</span>
          <router-link :to="`/cases/${c.id}`">{{ c.name || `Vaka ${c.id}` }}</router-link>
          <span v-if="c.asMainCompany" class="role">ana şirket olarak</span>
        </li>
      </ul>
      <p v-else class="empty">Bu şirketin vakası yok.</p>
    </section>
  </template>

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
.columns {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 16px;
}
.columns .panel {
  margin-bottom: 16px;
}
.hint {
  margin: 0 0 10px;
  font-size: 13px;
  color: var(--rs-muted);
}
.links,
.cases {
  list-style: none;
  margin: 0 0 12px;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 6px;
  font-size: 15px;
}
.links li {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
}
.links a,
.cases a {
  color: var(--rs-ink);
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
  margin: 0 0 12px;
  color: var(--rs-muted);
}
.picker {
  max-width: 420px;
}
</style>
