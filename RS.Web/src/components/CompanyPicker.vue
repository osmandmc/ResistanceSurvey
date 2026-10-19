<script setup lang="ts">
import { ref, watch } from 'vue'
import { NButton, NFormItem, NInput, NModal, NSelect, useMessage } from 'naive-ui'
import { ApiError } from '@/api'
import { createCompany, searchCompanies, type Option } from '@/reviewApi'

/** Picks a company by searching its name, or adds a new one ("Şirket Ekle"). */
const props = defineProps<{
  inputId: string
  /** The "add a company" button's label; leave out to hide the button (e.g. in a search filter). */
  addLabel?: string
  companyTypes?: Option[]
  companyScales?: Option[]
  companyWorklines?: Option[]
  status?: 'error'
  placeholder?: string
  /** Name of a company chosen elsewhere, so it can be shown before any search found it. */
  selectedName?: string | null
}>()

const companyId = defineModel<number | null>({ required: true })

const message = useMessage()
const options = ref<{ label: string; value: number }[]>([])
const searching = ref(false)
let timer: ReturnType<typeof setTimeout> | undefined

function search(text: string) {
  clearTimeout(timer)
  timer = setTimeout(async () => {
    searching.value = true
    try {
      options.value = (await searchCompanies(text)).map((c) => ({ label: c.name, value: c.id }))
    } catch {
      options.value = []
    } finally {
      searching.value = false
    }
  }, 300)
}

search('')

watch(
  () => [companyId.value, props.selectedName] as const,
  ([id, name]) => {
    if (id !== null && name && !options.value.some((o) => o.value === id)) {
      options.value = [{ label: name, value: id }, ...options.value]
    }
  },
  { immediate: true },
)

// "Şirket Ekle"
const adding = ref(false)
const saving = ref(false)
const newCompany = ref({ name: '', typeId: null as number | null, scaleId: null as number | null, worklineId: null as number | null })
const nameError = ref('')

function openAdd() {
  newCompany.value = { name: '', typeId: null, scaleId: null, worklineId: null }
  nameError.value = ''
  adding.value = true
}

async function saveCompany() {
  saving.value = true
  nameError.value = ''
  try {
    const result = await createCompany(newCompany.value)
    if (!options.value.some((o) => o.value === result.company.id)) {
      options.value = [{ label: result.company.name, value: result.company.id }, ...options.value]
    }
    companyId.value = result.company.id
    if (result.existed) message.info(`“${result.company.name}” zaten kayıtlı; o seçildi.`)
    else message.success('Şirket eklendi.')
    adding.value = false
  } catch (error) {
    nameError.value = error instanceof ApiError && error.fields.name ? error.fields.name : 'Şirket eklenemedi.'
  } finally {
    saving.value = false
  }
}

const toOptions = (list: Option[] | undefined) => (list ?? []).map((o) => ({ label: o.name, value: o.id }))
</script>

<template>
  <div class="picker">
    <n-select
      v-model:value="companyId"
      :options="options"
      :loading="searching"
      :status="props.status"
      filterable
      remote
      clearable
      :placeholder="props.placeholder ?? 'Şirket adıyla arayın'"
      :input-props="{ id: props.inputId }"
      class="select"
      @search="search"
    />
    <n-button v-if="props.addLabel" @click="openAdd">{{ props.addLabel }}</n-button>
  </div>

  <n-modal
    v-model:show="adding"
    preset="card"
    :title="props.addLabel"
    :style="{ width: 'min(520px, calc(100vw - 32px))' }"
    :mask-closable="!saving"
  >
    <n-form-item label="Şirket adı" label-placement="top" :validation-status="nameError ? 'error' : undefined" :feedback="nameError">
      <n-input v-model:value="newCompany.name" placeholder="Örn. Örnek Tekstil A.Ş." />
    </n-form-item>
    <n-form-item label="Şirket türü" label-placement="top">
      <n-select v-model:value="newCompany.typeId" :options="toOptions(props.companyTypes)" clearable />
    </n-form-item>
    <n-form-item label="Ölçek" label-placement="top">
      <n-select v-model:value="newCompany.scaleId" :options="toOptions(props.companyScales)" clearable />
    </n-form-item>
    <n-form-item label="İş kolu" label-placement="top">
      <n-select v-model:value="newCompany.worklineId" :options="toOptions(props.companyWorklines)" filterable clearable />
    </n-form-item>
    <template #footer>
      <div class="actions">
        <n-button :disabled="saving" @click="adding = false">Vazgeç</n-button>
        <n-button type="primary" :loading="saving" :disabled="!newCompany.name.trim()" @click="saveCompany">Ekle</n-button>
      </div>
    </template>
  </n-modal>
</template>

<style scoped>
.picker {
  display: flex;
  gap: 8px;
  width: 100%;
}
.select {
  flex: 1 1 auto;
  min-width: 0;
}
.actions {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
}
</style>
