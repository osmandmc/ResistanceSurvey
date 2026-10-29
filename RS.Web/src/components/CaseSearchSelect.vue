<script setup lang="ts">
import { ref, watch } from 'vue'
import { NSelect, type SelectOption } from 'naive-ui'
import { formatDate } from '@/format'
import { searchCases } from '@/reviewApi'

/** Picks a case by searching its name, company or description (R31: the supported case of a Dayanışma case). */
const props = defineProps<{
  /** The chosen case's name, so the field shows it before any search. */
  selectedName?: string | null
  /** Cases that cannot be chosen (the case itself). */
  exclude?: number[]
  placeholder?: string
}>()

const caseId = defineModel<number | null>({ required: true })

const options = ref<SelectOption[]>([])
const searching = ref(false)

watch(
  () => [caseId.value, props.selectedName] as const,
  ([id, name]) => {
    if (id !== null && !options.value.some((o) => o.value === id)) options.value = [{ label: name || `Vaka #${id}`, value: id }]
  },
  { immediate: true },
)

let latest = 0
async function search(query: string) {
  if (query.trim().length < 2) return
  const mine = ++latest
  searching.value = true
  try {
    const found = await searchCases({ query: query.trim() })
    if (mine !== latest) return
    const chosen = options.value.filter((o) => o.value === caseId.value)
    options.value = [
      ...chosen,
      ...found
        .filter((c) => c.id !== caseId.value && !(props.exclude ?? []).includes(c.id))
        .map((c) => ({
          label: [
            c.name || c.companyName || `Vaka #${c.id}`,
            c.ongoing ? 'devam ediyor' : c.lastProtestDate ? `son eylem ${formatDate(c.lastProtestDate)}` : null,
          ]
            .filter(Boolean)
            .join(' · '),
          value: c.id,
        })),
    ]
  } catch {
    // the field keeps its options
  } finally {
    if (mine === latest) searching.value = false
  }
}
</script>

<template>
  <n-select
    v-model:value="caseId"
    :options="options"
    :loading="searching"
    filterable
    remote
    clearable
    :placeholder="placeholder ?? 'Vaka ara: ad, işyeri, açıklama'"
    @search="search"
  />
</template>
