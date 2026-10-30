<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { NButton, NFormItem, NInput, NModal, NSelect, useMessage } from 'naive-ui'
import { ApiError, errorMessage } from '@/api'
import { UnionTypes, createCorporation, type CorporationOption, type Option } from '@/reviewApi'

/**
 * "Kurum Ekle" (R34): a new organization with its type and, for a union, its confederation. A similar existing name is
 * returned instead of a duplicate. From a case the organization waits for approval in Tanımlar; from Tanımlar it is approved.
 */
const props = defineProps<{
  corporationTypes: Option[]
  confederations: Option[]
  /** Prefilled name (what the researcher typed). */
  initialName?: string
  fromDefinitions?: boolean
}>()
const show = defineModel<boolean>('show', { required: true })
const emit = defineEmits<{ created: [corporation: CorporationOption, existed: boolean] }>()
const message = useMessage()

const name = ref('')
const typeId = ref<number | null>(null)
const confederationId = ref<number | null>(null)
const saving = ref(false)
const nameError = ref('')

watch(show, (open) => {
  if (!open) return
  name.value = props.initialName ?? ''
  typeId.value = null
  confederationId.value = null
  nameError.value = ''
})

const isUnion = computed(() => typeId.value === UnionTypes.TradeUnion)
watch(isUnion, (yes) => {
  if (!yes) confederationId.value = null
})

const toOptions = (list: Option[]) => list.map((o) => ({ label: o.name, value: o.id }))

async function save() {
  saving.value = true
  nameError.value = ''
  try {
    const result = await createCorporation({ name: name.value, typeId: typeId.value, confederationId: confederationId.value }, props.fromDefinitions)
    emit('created', result.corporation, result.existed)
    if (result.existed) message.info(`“${result.corporation.name}” zaten kayıtlı; o seçildi.`)
    else message.success(props.fromDefinitions ? 'Kurum eklendi.' : 'Kurum eklendi; Tanımlar’da onay bekliyor.')
    show.value = false
  } catch (error) {
    nameError.value = error instanceof ApiError && error.fields.name ? error.fields.name : errorMessage(error, 'Kurum eklenemedi.')
  } finally {
    saving.value = false
  }
}
</script>

<template>
  <n-modal v-model:show="show" preset="card" title="Kurum Ekle" :style="{ width: 'min(520px, calc(100vw - 32px))' }" :mask-closable="!saving">
    <n-form-item label="Kurumun adı" label-placement="top" :validation-status="nameError ? 'error' : undefined" :feedback="nameError || 'Örn. DİSK Genel-İş, Edremit Demokrasi Platformu'">
      <n-input v-model:value="name" />
    </n-form-item>
    <n-form-item label="Türü" label-placement="top" :show-feedback="false">
      <n-select v-model:value="typeId" :options="toOptions(corporationTypes)" clearable placeholder="Seçiniz" />
    </n-form-item>
    <n-form-item v-if="isUnion" label="Konfederasyonu" label-placement="top" :feedback="'Bağımsız sendikalar için boş bırakın.'">
      <n-select v-model:value="confederationId" :options="toOptions(confederations)" clearable placeholder="Bağımsız" />
    </n-form-item>
    <template #footer>
      <div class="actions">
        <n-button :disabled="saving" @click="show = false">Vazgeç</n-button>
        <n-button type="primary" :loading="saving" :disabled="!name.trim()" @click="save">Ekle</n-button>
      </div>
    </template>
  </n-modal>
</template>

<style scoped>
.actions {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
}
</style>
