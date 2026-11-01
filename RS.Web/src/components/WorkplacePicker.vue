<script setup lang="ts">
import { turkishFilter } from '@/search'
import { computed, ref, watch } from 'vue'
import { NButton, NFormItem, NInput, NModal, NSelect, useMessage } from 'naive-ui'
import { ApiError } from '@/api'
import { createWorkplace, searchWorkplaces, type DistrictOption, type Option, type WorkplaceOption } from '@/reviewApi'

/**
 * İşyeri (R37): a place, picked by name or from the places known for a company, or added (“Yeni İşyeri”). Empty is
 * allowed: for a direct employer it means the employer itself.
 */
const props = defineProps<{
  inputId: string
  /** Name of the chosen place, so it shows before any search found it (editing a case). */
  selectedName?: string | null
  /** A name suggested elsewhere (the AI answer) that is not registered: offered as "add", never added on its own. */
  suggestedName?: string | null
  /** Whose places to suggest before anything is typed: the organization above the employer, else the employer. */
  suggestFor?: number | null
  /** Shown while empty, e.g. “İşverenle aynı” for a direct employer. */
  placeholder?: string
  cities: Option[]
  districts: DistrictOption[]
  /** A new place belongs to this organization (and its name is shown in the dialog). */
  ownerCompanyId?: number | null
  ownerName?: string | null
  status?: 'error'
}>()

const workplaceId = defineModel<number | null>({ required: true })
const emit = defineEmits<{
  /** The place chosen (null when cleared): the form suggests the organization above from its owner. */
  picked: [workplace: WorkplaceOption | null]
}>()

const message = useMessage()
const found = ref<WorkplaceOption[]>([])
const searching = ref(false)
let timer: ReturnType<typeof setTimeout> | undefined
let lastText = ''

const label = (w: WorkplaceOption) => [w.name, w.districtName ? `${w.cityName} / ${w.districtName}` : w.cityName].filter(Boolean).join(' · ')
const options = computed(() => found.value.map((w) => ({ label: label(w), value: w.id })))

function load(text: string) {
  lastText = text
  clearTimeout(timer)
  timer = setTimeout(async () => {
    searching.value = true
    try {
      const results = await searchWorkplaces(text, text ? null : props.suggestFor ?? null)
      const chosen = found.value.find((w) => w.id === workplaceId.value)
      found.value = chosen && !results.some((w) => w.id === chosen.id) ? [chosen, ...results] : results
    } catch {
      // the field still works with what it has
    } finally {
      searching.value = false
    }
  }, text ? 300 : 0)
}

// Opening the empty field shows the suggestions again.
function onFocus() {
  if (!lastText) load('')
}

// The suggestions follow the company they are for.
watch(() => props.suggestFor, () => load(''), { immediate: true })

// A place chosen elsewhere (editing) shows by its name.
watch(
  () => [workplaceId.value, props.selectedName] as const,
  ([id, name]) => {
    if (id !== null && name && !found.value.some((w) => w.id === id))
      found.value = [{ id, name, cityName: null, districtName: null, ownerCompanyId: null, ownerName: null, approved: true }, ...found.value]
  },
  { immediate: true },
)

function onPick(id: number | null) {
  workplaceId.value = id
  emit('picked', found.value.find((w) => w.id === id) ?? null)
}

// “Yeni İşyeri”: name, province and district; it belongs to the organization the form says.
const adding = ref(false)
const saving = ref(false)
const draft = ref({ name: '', cityId: null as number | null, districtId: null as number | null })
const nameError = ref('')
const cityDistricts = computed(() => props.districts.filter((d) => d.cityId === draft.value.cityId))
watch(() => draft.value.cityId, () => (draft.value.districtId = null))

function openAdd(name = lastText) {
  draft.value = { name, cityId: null, districtId: null }
  nameError.value = ''
  adding.value = true
}

async function save() {
  saving.value = true
  nameError.value = ''
  try {
    const result = await createWorkplace({ ...draft.value, ownerCompanyId: props.ownerCompanyId ?? null })
    if (!found.value.some((w) => w.id === result.workplace.id)) found.value = [result.workplace, ...found.value]
    onPick(result.workplace.id)
    if (result.existed) message.info(`“${result.workplace.name}” zaten kayıtlı; o seçildi.`)
    else message.success('İşyeri eklendi.')
    adding.value = false
  } catch (error) {
    nameError.value = error instanceof ApiError ? error.message : 'İşyeri eklenemedi.'
  } finally {
    saving.value = false
  }
}

const toOptions = (list: Option[]) => list.map((o) => ({ label: o.name, value: o.id }))
</script>

<template>
  <div class="column">
    <div class="picker">
      <n-select
        :value="workplaceId"
        :options="options"
        :loading="searching"
        :status="props.status"
        filterable
        :filter="turkishFilter"
        remote
        clearable
        :placeholder="props.placeholder ?? 'Fabrika, hastane, şantiye, maden …'"
        :input-props="{ id: props.inputId }"
        class="select"
        @search="load"
        @update:value="onPick"
        @focus="onFocus"
      />
      <n-button @click="openAdd()">Yeni İşyeri</n-button>
    </div>
    <div v-if="props.suggestedName && workplaceId === null" class="suggested">
      <span>AI önerisi kayıtlı değil:</span>
      <n-button size="small" secondary type="primary" @click="openAdd(props.suggestedName)">“{{ props.suggestedName }}” işyerini ekle</n-button>
    </div>
  </div>

  <n-modal
    v-model:show="adding"
    preset="card"
    title="Yeni İşyeri"
    :style="{ width: 'min(520px, calc(100vw - 32px))' }"
    :mask-closable="!saving"
  >
    <n-form-item label="İşyerinin adı" label-placement="top" :validation-status="nameError ? 'error' : undefined" :feedback="nameError || 'Bir yer: fabrika, hastane, şantiye, maden … Şirket adı değil.'">
      <n-input v-model:value="draft.name" placeholder="Örn. Gölcük fabrikası" />
    </n-form-item>
    <n-form-item label="İl" label-placement="top">
      <n-select v-model:value="draft.cityId" :options="toOptions(props.cities)" filterable :filter="turkishFilter" clearable />
    </n-form-item>
    <n-form-item label="İlçe" label-placement="top">
      <n-select v-model:value="draft.districtId" :options="toOptions(cityDistricts)" :disabled="!draft.cityId" filterable :filter="turkishFilter" clearable />
    </n-form-item>
    <p class="owner">Kime ait: <strong>{{ props.ownerName || 'belirtilmedi' }}</strong> <span>(formdaki işverenden ya da üstündeki kurumdan)</span></p>
    <template #footer>
      <div class="actions">
        <n-button :disabled="saving" @click="adding = false">Vazgeç</n-button>
        <n-button type="primary" :loading="saving" :disabled="!draft.name.trim()" @click="save">Ekle</n-button>
      </div>
    </template>
  </n-modal>
</template>

<style scoped>
.column {
  width: 100%;
}
.picker {
  display: flex;
  gap: 8px;
  width: 100%;
}
.select {
  flex: 1 1 auto;
  min-width: 0;
}
.suggested {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 8px;
  margin-top: 6px;
  font-size: 13px;
  color: var(--rs-muted);
}
.owner {
  margin: 0;
  font-size: 14px;
}
.owner span {
  color: var(--rs-muted);
  font-size: 13px;
}
.actions {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
}
</style>
