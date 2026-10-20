<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { NAlert, NButton, NFormItem, NModal, NSelect } from 'naive-ui'
import { errorMessage } from '@/api'
import { listDefinitions, mergeDefinitions, mergePreview, type DefinitionItem, type DefinitionKind } from '@/definitionsApi'

// "Birleştir": everything that used `from` will use the chosen value, and `from` goes.
const props = defineProps<{
  kind: DefinitionKind
  /** The value to merge away; null closes the dialog. */
  from: DefinitionItem | null
  /** A value to preselect, e.g. the one that already has the new name. */
  suggestedInto?: { id: number; name: string } | null
  unit: 'vaka' | 'eylem'
}>()
const emit = defineEmits<{ merged: [affected: number]; close: [] }>()

const intoId = ref<number | null>(null)
const options = ref<{ label: string; value: number }[]>([])
const searching = ref(false)
const affected = ref<number | null>(null)
const saving = ref(false)
const error = ref('')

watch(
  () => props.from,
  async (from) => {
    intoId.value = props.suggestedInto?.id ?? null
    options.value = props.suggestedInto ? [{ label: props.suggestedInto.name, value: props.suggestedInto.id }] : []
    affected.value = null
    error.value = ''
    if (from) affected.value = (await mergePreview(props.kind, from.id)).affected
  },
)

async function search(text: string) {
  if (!props.from || text.trim().length < 2) return
  searching.value = true
  try {
    const page = await listDefinitions(props.kind, { text: text.trim() })
    options.value = page.items.filter((i) => i.id !== props.from!.id).map((i) => ({ label: `${i.name} (${i.usage} ${props.unit})`, value: i.id }))
  } finally {
    searching.value = false
  }
}

const intoName = computed(() => options.value.find((o) => o.value === intoId.value)?.label ?? '')

async function merge() {
  if (!props.from || intoId.value === null) return
  saving.value = true
  error.value = ''
  try {
    const result = await mergeDefinitions(props.kind, props.from.id, intoId.value)
    emit('merged', result.affected)
  } catch (e) {
    error.value = errorMessage(e, 'Birleştirilemedi.')
  } finally {
    saving.value = false
  }
}
</script>

<template>
  <n-modal :show="from !== null" preset="card" title="Birleştir" class="merge-dialog" :mask-closable="!saving" @update:show="(show) => !show && emit('close')">
    <template v-if="from">
      <p class="lead">
        <strong>“{{ from.name }}”</strong> kaldırılacak; onu kullanan kayıtlar seçtiğiniz değeri kullanacak.
      </p>
      <n-form-item label="Birleştirilecek değer" label-placement="top" :show-feedback="false">
        <n-select
          v-model:value="intoId"
          :options="options"
          filterable
          remote
          :loading="searching"
          placeholder="En az 2 harf yazın"
          :input-props="{ id: 'merge-into' }"
          @search="search"
        />
      </n-form-item>
      <p v-if="affected !== null" class="hint">
        {{ affected }} {{ unit }} etkilenecek.<template v-if="intoId !== null"> Hepsi “{{ intoName.replace(/ \(\d+ (vaka|eylem)\)$/, '') }}” kullanacak.</template>
        Aynı kayıtta ikisi birden varsa tek kalır.
      </p>
      <n-alert v-if="error" type="error" :show-icon="false" class="error">{{ error }}</n-alert>
    </template>
    <template #footer>
      <div class="actions">
        <n-button :disabled="saving" @click="emit('close')">Vazgeç</n-button>
        <n-button type="primary" :disabled="intoId === null" :loading="saving" @click="merge">Birleştir</n-button>
      </div>
    </template>
  </n-modal>
</template>

<style scoped>
.merge-dialog {
  max-width: 520px;
}
.lead {
  margin: 0 0 16px;
}
.hint {
  margin: 12px 0 0;
  font-size: 14px;
  color: var(--rs-muted);
}
.error {
  margin-top: 12px;
}
.actions {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
}
</style>
