<script setup lang="ts">
import { ref, watch } from 'vue'
import { NSpin } from 'naive-ui'
import { actionLabel, caseActivity, type ActivityRow } from '@/activityApi'
import { formatDateTime } from '@/format'

// The case page's Geçmiş (F4b): who did what to the case, its protests and articles. Reloads when the case changes.
const props = defineProps<{ caseId: number; version: unknown }>()

const rows = ref<ActivityRow[] | null>(null)
const failed = ref(false)

async function load() {
  try {
    rows.value = await caseActivity(props.caseId)
    failed.value = false
  } catch {
    failed.value = true
  }
}
watch(() => [props.caseId, props.version], load, { immediate: true })
</script>

<template>
  <section class="panel" aria-labelledby="history-heading">
    <details>
      <summary>
        <h2 id="history-heading" class="section-title">Geçmiş<template v-if="rows"> · {{ rows.length }}</template></h2>
      </summary>
      <p v-if="failed" class="sub">Geçmiş yüklenemedi.</p>
      <div v-else-if="!rows" class="state"><n-spin size="small" /></div>
      <p v-else-if="rows.length === 0" class="sub">
        Kayıt yok. Geçmiş, kullanıcı hesaplarına geçildikten sonra yapılan değişiklikleri gösterir.
      </p>
      <ul v-else class="history">
        <li v-for="r in rows" :key="r.id">
          <span class="when">{{ formatDateTime(r.at) }}</span>
          <span><strong>{{ r.username ?? '—' }}</strong> · {{ actionLabel(r.action) }}<template v-if="r.summary">: {{ r.summary }}</template></span>
        </li>
      </ul>
    </details>
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
summary {
  cursor: pointer;
}
.section-title {
  display: inline;
  margin: 0;
  font-size: 18px;
}
.sub {
  margin: 12px 0 0;
  font-size: 13px;
  color: var(--rs-muted);
}
.state {
  padding: 12px 0;
}
.history {
  list-style: none;
  margin: 12px 0 0;
  padding: 0;
  font-size: 14px;
}
.history li {
  display: flex;
  flex-wrap: wrap;
  gap: 4px 16px;
  padding: 8px 0;
  border-bottom: 1px solid #eef0f2;
}
.when {
  min-width: 150px;
  color: var(--rs-muted);
  font-variant-numeric: tabular-nums;
}
</style>
