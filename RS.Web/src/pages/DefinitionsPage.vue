<script setup lang="ts">
import { turkishFilter } from '@/search'
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { NAlert, NButton, NCheckbox, NEmpty, NInput, NPopconfirm, NSelect, NSpin, NSwitch, NTag, useMessage } from 'naive-ui'
import { ApiError, errorMessage } from '@/api'
import { isAdmin } from '@/auth'
import CorporationAddDialog from '@/components/CorporationAddDialog.vue'
import { UnionTypes } from '@/reviewApi'
import DuplicateGroups from '@/components/DuplicateGroups.vue'
import MergeDialog from '@/components/MergeDialog.vue'
import { institutionKindOptions,
  approveDefinition,
  definitionKinds,
  deleteDefinition,
  getDefinitionOptions,
  listDefinitions,
  updateDefinition,
  type DefinitionItem,
  type DefinitionKind,
  type DefinitionOptions,
  type Option,
} from '@/definitionsApi'

const route = useRoute()
const router = useRouter()
const message = useMessage()

// The list is in the address (/definitions/corporations); the filters are not, each list starts fresh.
const kind = computed<DefinitionKind>(() => {
  const param = route.params.kind
  return definitionKinds.find((k) => k.kind === param)?.kind ?? 'corporations'
})
const meta = computed(() => definitionKinds.find((k) => k.kind === kind.value)!)

// The list itself, or its likely duplicates (R12a-2).
const view = ref<'list' | 'duplicates'>('list')
const text = ref('')
const pendingOnly = ref(false)
/** Şirketler: only companies no live case uses (as workplace or main company). */
const unusedOnly = ref(false)
const typeId = ref<number | null>(null)
const page = ref(1)

const options = ref<DefinitionOptions>({ corporationTypes: [], companyTypes: [], companyScales: [], companyWorklines: [], confederations: [], serviceBranches: [] })
// "Yeni kurum" (R34): added approved.
const addingCorporation = ref(false)
function reloadOptions() {
  void getDefinitionOptions().then((o) => (options.value = o))
}
reloadOptions()
const toSelect = (list: Option[]) => list.map((o) => ({ label: o.name, value: o.id }))
const nameOf = (list: Option[], id: number | null) => (id === null ? '—' : (list.find((o) => o.id === id)?.name ?? '—'))

const items = ref<DefinitionItem[]>([])
const total = ref(0)
const pageSize = ref(50)
const pendingCount = ref(0)
const loading = ref(false)
const loadError = ref('')

async function load() {
  loading.value = true
  loadError.value = ''
  try {
    const result = await listDefinitions(kind.value, { text: text.value.trim(), pending: pendingOnly.value, typeId: typeId.value, unused: unusedOnly.value, page: page.value })
    items.value = result.items
    total.value = result.total
    pageSize.value = result.pageSize
    pendingCount.value = result.pendingCount
  } catch (error) {
    loadError.value = errorMessage(error, 'Liste yüklenemedi.')
  } finally {
    loading.value = false
  }
}

watch(kind, () => {
  view.value = 'list'
  text.value = ''
  pendingOnly.value = false
  unusedOnly.value = false
  typeId.value = null
  page.value = 1
  editing.value = null
  void load()
}, { immediate: true })

// Searching while typing, a moment after the last key.
let searchTimer: ReturnType<typeof setTimeout> | undefined
watch(text, () => {
  clearTimeout(searchTimer)
  searchTimer = setTimeout(() => {
    page.value = 1
    void load()
  }, 300)
})
watch([pendingOnly, typeId, unusedOnly], () => {
  page.value = 1
  void load()
})
watch(page, () => void load())

const from = computed(() => (total.value === 0 ? 0 : (page.value - 1) * pageSize.value + 1))
const to = computed(() => Math.min(total.value, page.value * pageSize.value))

// Editing one row at a time: a copy, saved as a whole.
const editing = ref<DefinitionItem | null>(null)
const saving = ref(false)

function edit(item: DefinitionItem) {
  editing.value = { ...item }
}

async function save() {
  if (!editing.value) return
  saving.value = true
  try {
    await updateDefinition(kind.value, editing.value)
    if (kind.value === 'corporations') reloadOptions()
    const index = items.value.findIndex((i) => i.id === editing.value!.id)
    if (index >= 0) items.value[index] = { ...editing.value, name: editing.value.name.trim() }
    editing.value = null
  } catch (error) {
    // Another value already has this name: offer to merge into it.
    if (error instanceof ApiError && error.status === 409 && typeof error.details?.existingId === 'number') {
      const item = items.value.find((i) => i.id === editing.value!.id)!
      mergeSuggestion.value = { id: error.details.existingId, name: String(error.details.existingName) }
      mergeFrom.value = item
      message.warning(errorMessage(error, 'Bu ad zaten var.'))
    } else {
      message.error(errorMessage(error, 'Kaydedilemedi.'))
    }
  } finally {
    saving.value = false
  }
}

async function approve(item: DefinitionItem) {
  try {
    await approveDefinition(kind.value, item.id)
    item.approved = true
    pendingCount.value = Math.max(0, pendingCount.value - 1)
    if (pendingOnly.value) items.value = items.value.filter((i) => i.id !== item.id)
  } catch (error) {
    message.error(errorMessage(error, 'Onaylanamadı.'))
  }
}

async function remove(item: DefinitionItem) {
  try {
    await deleteDefinition(kind.value, item.id)
    items.value = items.value.filter((i) => i.id !== item.id)
    total.value -= 1
    message.success(`“${item.name}” silindi.`)
  } catch (error) {
    message.error(errorMessage(error, 'Silinemedi.'))
  }
}

const mergeFrom = ref<DefinitionItem | null>(null)
const mergeSuggestion = ref<{ id: number; name: string } | null>(null)

function startMerge(item: DefinitionItem) {
  mergeSuggestion.value = null
  mergeFrom.value = item
}

function onMerged(affected: number) {
  message.success(`Birleştirildi: ${affected} ${meta.value.usageUnit} güncellendi.`)
  mergeFrom.value = null
  editing.value = null
  void load()
}
</script>

<template>
  <h1 class="heading">Tanımlar</h1>
  <p class="description">
    Vaka formundaki listeler. Araştırmacıların yazdığı yeni değerler onay bekler; aynı şeyin iki adı varsa birleştirin.
  </p>

  <nav class="kinds" aria-label="Listeler">
    <router-link v-for="k in definitionKinds" :key="k.kind" :to="`/definitions/${k.kind}`" class="kind-link" :class="{ active: k.kind === kind }">
      {{ k.label }}
    </router-link>
    <router-link to="/definitions/prompts" class="kind-link">AI istemleri</router-link>
    <router-link v-if="isAdmin" to="/definitions/users" class="kind-link">Kullanıcılar</router-link>
    <router-link v-if="isAdmin" to="/definitions/activity" class="kind-link">Kayıtlar</router-link>
  </nav>

  <section class="panel">
    <div class="views" role="group" aria-label="Görünüm">
      <n-button size="small" :type="view === 'list' ? 'primary' : 'default'" :secondary="view !== 'list'" @click="view = 'list'">Liste</n-button>
      <n-button size="small" :type="view === 'duplicates' ? 'primary' : 'default'" :secondary="view !== 'duplicates'" @click="view = 'duplicates'">
        Olası tekrarlar
      </n-button>
    </div>

    <duplicate-groups v-if="view === 'duplicates'" :kind="kind" :unit="meta.usageUnit" @changed="load()" />

    <template v-else>
    <div class="filters">
      <n-input v-model:value="text" clearable placeholder="Ada göre ara" class="search" :input-props="{ id: 'definition-search', 'aria-label': 'Ada göre ara' }" />
      <n-select
        v-if="kind === 'corporations'"
        v-model:value="typeId"
        :options="toSelect(options.corporationTypes)"
        clearable
        placeholder="Tüm türler"
        class="type-filter"
      />
      <n-checkbox v-if="meta.hasApproval" v-model:checked="pendingOnly">Yalnız onay bekleyenler ({{ pendingCount }})</n-checkbox>
      <n-checkbox v-if="kind === 'companies'" v-model:checked="unusedOnly">Yalnız vakası olmayanlar</n-checkbox>
      <n-button v-if="kind === 'corporations'" @click="addingCorporation = true">Yeni kurum</n-button>
    </div>
    <corporation-add-dialog
      v-if="kind === 'corporations'"
      v-model:show="addingCorporation"
      :corporation-types="options.corporationTypes"
      :confederations="options.confederations"
      from-definitions
      @created="() => { load(); reloadOptions() }"
    />

    <div v-if="loading && items.length === 0" class="state"><n-spin /></div>
    <n-alert v-else-if="loadError" type="error" :show-icon="false" class="state-alert">
      {{ loadError }} <n-button size="small" @click="load()">Tekrar dene</n-button>
    </n-alert>
    <div v-else-if="items.length === 0" class="state"><n-empty :description="pendingOnly ? 'Onay bekleyen değer yok.' : unusedOnly ? 'Vakası olmayan şirket yok.' : 'Değer bulunamadı.'" /></div>

    <div v-else class="table-wrap">
      <table class="definitions-table">
        <thead>
          <tr>
            <th>Ad</th>
            <template v-if="kind === 'corporations'">
              <th>Tür</th>
              <th>Konfederasyon</th>
            </template>
            <template v-if="kind === 'companies'">
              <th>Kurum türü</th>
              <th>Sahiplik</th>
              <th>Büyüklük</th>
              <th>İşkolu / hizmet kolu</th>
            </template>
            <template v-if="kind === 'protest-types'">
              <th>Üretime karşı</th>
              <th>Basit</th>
            </template>
            <th class="num">Kullanım</th>
            <th><span class="visually-hidden">İşlemler</span></th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in items" :key="item.id" :class="{ pending: item.approved === false }">
            <template v-if="editing && editing.id === item.id">
              <td><n-input v-model:value="editing.name" size="small" :input-props="{ 'aria-label': 'Ad' }" @keyup.enter="save" /></td>
              <td v-if="kind === 'corporations'">
                <n-select v-model:value="editing.typeId" :options="toSelect(options.corporationTypes)" clearable size="small" placeholder="Türsüz" />
              </td>
              <td v-if="kind === 'corporations'">
                <n-select
                  v-if="editing.typeId === UnionTypes.TradeUnion"
                  v-model:value="editing.confederationId"
                  :options="toSelect(options.confederations.filter((c) => c.id !== editing!.id))"
                  clearable
                  size="small"
                  placeholder="Bağımsız"
                />
              </td>
              <template v-if="kind === 'companies'">
                <td><n-select v-model:value="editing.institutionKind" :options="institutionKindOptions" clearable size="small" /></td>
                <td><n-select v-model:value="editing.typeId" :options="toSelect(options.companyTypes)" clearable size="small" /></td>
                <td><n-select v-model:value="editing.scaleId" :options="toSelect(options.companyScales)" clearable size="small" /></td>
                <td><n-select v-model:value="editing.worklineId" :options="toSelect(options.companyWorklines)" clearable filterable :filter="turkishFilter" size="small" /></td>
              </template>
              <template v-if="kind === 'protest-types'">
                <td><n-switch :value="editing.againstProduction ?? false" size="small" @update:value="(v: boolean) => (editing!.againstProduction = v)" /></td>
                <td><n-switch :value="editing.simple ?? false" size="small" @update:value="(v: boolean) => (editing!.simple = v)" /></td>
              </template>
              <td class="num">{{ item.usage }}</td>
              <td class="actions">
                <n-button size="small" type="primary" :loading="saving" @click="save">Kaydet</n-button>
                <n-button size="small" :disabled="saving" @click="editing = null">Vazgeç</n-button>
              </td>
            </template>

            <template v-else>
              <td class="name">
                <router-link v-if="kind === 'companies'" :to="`/definitions/companies/${item.id}`" class="name-link">{{ item.name }}</router-link>
                <template v-else>{{ item.name }}</template>
                <n-tag v-if="item.approved === false" size="small" type="warning" :bordered="false" class="tag">onay bekliyor</n-tag>
              </td>
              <template v-if="kind === 'corporations'">
                <td>{{ nameOf(options.corporationTypes, item.typeId) }}</td>
                <td>{{ item.typeId === UnionTypes.TradeUnion ? nameOf(options.confederations, item.confederationId) : '' }}</td>
              </template>
              <template v-if="kind === 'companies'">
                <td>{{ institutionKindOptions.find((o) => o.value === item.institutionKind)?.label ?? '' }}</td>
                <td>{{ nameOf(options.companyTypes, item.typeId) }}</td>
                <td>{{ nameOf(options.companyScales, item.scaleId) }}</td>
                <td>{{ nameOf(options.companyWorklines, item.worklineId) || nameOf(options.serviceBranches ?? [], item.serviceBranchId ?? null) }}</td>
              </template>
              <template v-if="kind === 'protest-types'">
                <td>{{ item.againstProduction ? 'Evet' : 'Hayır' }}</td>
                <td>{{ item.simple ? 'Evet' : 'Hayır' }}</td>
              </template>
              <td class="num">
                <router-link v-if="kind === 'companies' && item.usage > 0" :to="{ path: '/cases', query: { companyId: item.id } }">{{ item.usage }}</router-link>
                <template v-else>{{ item.usage }}</template>
              </td>
              <td class="actions">
                <n-button v-if="item.approved === false" size="small" type="primary" @click="approve(item)">Onayla</n-button>
                <n-button size="small" @click="edit(item)">Düzenle</n-button>
                <n-button size="small" @click="startMerge(item)">Birleştir</n-button>
                <n-popconfirm v-if="item.usage === 0" positive-text="Sil" negative-text="Vazgeç" @positive-click="remove(item)">
                  <template #trigger>
                    <n-button size="small" type="error" ghost>Sil</n-button>
                  </template>
                  “{{ item.name }}” silinsin mi?
                </n-popconfirm>
              </td>
            </template>
          </tr>
        </tbody>
      </table>
    </div>

    <div v-if="total > pageSize" class="pager">
      <n-button size="small" :disabled="page === 1 || loading" @click="page -= 1">Önceki</n-button>
      <span>{{ from }}–{{ to }} / {{ total }}</span>
      <n-button size="small" :disabled="to >= total || loading" @click="page += 1">Sonraki</n-button>
    </div>
    <p v-else-if="items.length" class="count">{{ total }} değer · kullanım: {{ meta.usageUnit }} sayısı</p>
    </template>
  </section>

  <merge-dialog :kind="kind" :from="mergeFrom" :suggested-into="mergeSuggestion" :unit="meta.usageUnit" @merged="onMerged" @close="mergeFrom = null" />
</template>

<style scoped>
.heading {
  margin: 0;
  font-size: 30px;
}
.description {
  margin: 6px 0 16px;
  color: var(--rs-muted);
  font-size: 16px;
  max-width: 760px;
}
.kinds {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-bottom: 16px;
}
.kind-link {
  padding: 6px 14px;
  border-radius: 999px;
  border: 1px solid var(--rs-border);
  background: var(--rs-surface);
  color: var(--rs-ink);
  text-decoration: none;
  font-size: 14px;
}
.kind-link.active {
  background: var(--rs-accent);
  border-color: var(--rs-accent);
  color: #fff;
}
.panel {
  background: var(--rs-surface);
  border: 1px solid var(--rs-border);
  border-radius: 10px;
}
.views {
  display: flex;
  gap: 8px;
  padding: 12px 16px 0;
}
.filters {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 12px;
  padding: 16px;
  border-bottom: 1px solid var(--rs-border);
}
.search {
  flex: 1 1 260px;
  max-width: 420px;
}
.type-filter {
  width: 200px;
}
.state {
  padding: 40px 16px;
}
.state-alert {
  margin: 16px;
}
.table-wrap {
  overflow-x: auto;
}
.definitions-table {
  width: 100%;
  min-width: 720px;
  border-collapse: collapse;
  font-size: 15px;
}
.definitions-table th {
  text-align: left;
  font-size: 12px;
  letter-spacing: 0.06em;
  text-transform: uppercase;
  color: var(--rs-muted);
  font-weight: 600;
  padding: 10px 16px;
  border-bottom: 1px solid var(--rs-border);
}
.definitions-table td {
  padding: 10px 16px;
  border-bottom: 1px solid #eef0f2;
  vertical-align: middle;
}
.definitions-table tr.pending {
  background: #fffaf0;
}
.name {
  font-weight: 500;
}
.tag {
  margin-left: 8px;
}
.name-link {
  color: var(--rs-ink);
}
.num {
  text-align: right;
  font-variant-numeric: tabular-nums;
}
.num a {
  color: var(--rs-accent);
}
.actions {
  white-space: nowrap;
  text-align: right;
}
.actions > * + * {
  margin-left: 6px;
}
.pager {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 12px;
  padding: 12px;
  font-size: 14px;
}
.count {
  margin: 0;
  padding: 12px 16px;
  font-size: 13px;
  color: var(--rs-muted);
}
.visually-hidden {
  position: absolute;
  width: 1px;
  height: 1px;
  overflow: hidden;
  clip: rect(0 0 0 0);
}
</style>
