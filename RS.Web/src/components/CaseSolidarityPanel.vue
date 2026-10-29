<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { NButton, useMessage } from 'naive-ui'
import { errorMessage } from '@/api'
import { getCaseSolidarity, linkSupportedCase, type CaseSolidarity } from '@/casesApi'
import { CaseCategory } from '@/reviewApi'

// On a case's page (R31c): a Dayanışma case's supported case; for any case, the Dayanışma cases linked to it and, for
// an İşletme case, unlinked ones that name its workplace, with "Bağla".
const props = defineProps<{
  caseId: number
  categoryId: number | null
  supportedCase: { id: number; name: string | null } | null
  /** Changes when the case was saved: reload. */
  version: unknown
}>()
const emit = defineEmits<{ changed: [] }>()
const message = useMessage()

const data = ref<CaseSolidarity | null>(null)
const linking = ref<number | null>(null)

async function load() {
  try {
    data.value = await getCaseSolidarity(props.caseId)
  } catch {
    data.value = null
  }
}
watch(() => [props.caseId, props.version], load, { immediate: true })

const solidarity = computed(() => props.categoryId === CaseCategory.Solidarity)
const visible = computed(() => solidarity.value || (data.value && (data.value.linked.length || data.value.suggested.length)))

async function link(solidarityCaseId: number) {
  linking.value = solidarityCaseId
  try {
    await linkSupportedCase(solidarityCaseId, props.caseId)
    message.success('Dayanışma vakası bu vakaya bağlandı.')
    await load()
    emit('changed')
  } catch (error) {
    message.error(errorMessage(error, 'Bağlanamadı.'))
  } finally {
    linking.value = null
  }
}
</script>

<template>
  <section v-if="visible" class="panel" aria-labelledby="solidarity-heading">
    <h2 id="solidarity-heading" class="section-title">Dayanışma</h2>

    <p v-if="solidarity" class="line">
      <strong>Desteklenen vaka:</strong>
      <router-link v-if="supportedCase" :to="`/cases/${supportedCase.id}`">{{ supportedCase.name || `Vaka #${supportedCase.id}` }}</router-link>
      <span v-else class="muted">henüz bağlanmadı. Yukarıdaki formda “Desteklenen Vaka” alanından seçip kaydedin.</span>
    </p>

    <template v-if="data?.linked.length">
      <p class="line"><strong>Bu vakayla dayanışma:</strong> {{ data.linked.length }} vaka</p>
      <ul class="list">
        <li v-for="c in data.linked" :key="c.id"><router-link :to="`/cases/${c.id}`">{{ c.name || `Vaka #${c.id}` }}</router-link></li>
      </ul>
    </template>

    <template v-if="data?.suggested.length">
      <p class="line hint">Bu vakanın işyerini anan, henüz bir vakaya bağlı olmayan {{ data.suggested.length }} dayanışma vakası var:</p>
      <ul class="list">
        <li v-for="c in data.suggested" :key="c.id">
          <router-link :to="`/cases/${c.id}`">{{ c.name || `Vaka #${c.id}` }}</router-link>
          <n-button size="tiny" :loading="linking === c.id" :disabled="linking !== null" @click="link(c.id)">Bu vakaya bağla</n-button>
        </li>
      </ul>
    </template>
  </section>
</template>

<style scoped>
.panel {
  background: var(--rs-surface);
  border: 1px solid var(--rs-border);
  border-radius: 10px;
  padding: 20px 24px;
  margin-bottom: 20px;
}
.section-title {
  margin: 0 0 10px;
  font-size: 18px;
}
.line {
  margin: 6px 0;
  font-size: 14px;
}
.line a,
.list a {
  color: var(--rs-accent);
}
.hint {
  padding: 6px 10px;
  border-left: 3px solid #d4a72c;
  background: #fff8e5;
}
.muted {
  color: var(--rs-muted);
}
.list {
  margin: 4px 0 8px;
  padding-left: 18px;
  font-size: 14px;
  display: flex;
  flex-direction: column;
  gap: 6px;
}
.list li > * + * {
  margin-left: 8px;
}
</style>
