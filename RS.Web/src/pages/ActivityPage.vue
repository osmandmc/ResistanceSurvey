<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { NAlert, NButton, NEmpty, NPagination, NSelect, NSpin } from 'naive-ui'
import { errorMessage } from '@/api'
import { actionLabel, activityLink, areas, listActivity, type ActivityArea, type ActivityPage } from '@/activityApi'
import { formatDateTime } from '@/format'

// Tanımlar → Kayıtlar (F4b), yönetici only: everything people did that changed data, newest first.
const user = ref<string | null>(null)
const area = ref<ActivityArea | null>(null)
const page = ref(1)
const result = ref<ActivityPage | null>(null)
const loading = ref(false)
const loadError = ref('')

const userOptions = computed(() => (result.value?.usernames ?? []).map((u) => ({ label: u, value: u })))
const pageCount = computed(() => (result.value ? Math.max(1, Math.ceil(result.value.total / result.value.pageSize)) : 1))

async function load() {
  loading.value = true
  loadError.value = ''
  try {
    result.value = await listActivity({ user: user.value, area: area.value, page: page.value })
  } catch (error) {
    loadError.value = errorMessage(error, 'Kayıtlar yüklenemedi.')
  } finally {
    loading.value = false
  }
}

watch([user, area], () => {
  if (page.value !== 1) page.value = 1
  else void load()
})
watch(page, load, { immediate: true })
</script>

<template>
  <p class="back"><router-link to="/definitions">← Tanımlar</router-link></p>
  <h1 class="heading">Kayıtlar</h1>
  <p class="description">
    Kimin ne zaman ne yaptığı: haber kararları, vaka ve eylem değişiklikleri, tanımlar, hesaplar ve yüklemeler. Yalnızca
    bir değişiklik olduğunu kaydeder, eski ve yeni değerleri değil. Kullanıcı hesaplarından önceki işler burada yok.
  </p>

  <div class="filters">
    <n-select v-model:value="user" :options="userOptions" clearable placeholder="Herkes" class="filter" aria-label="Kişi" />
    <n-select v-model:value="area" :options="areas" clearable placeholder="Her yer" class="filter" aria-label="Bölüm" />
  </div>

  <section class="panel">
    <n-alert v-if="loadError" type="error" :show-icon="false">
      {{ loadError }} <n-button size="small" @click="load">Tekrar dene</n-button>
    </n-alert>
    <div v-else-if="!result" class="state"><n-spin /></div>
    <div v-else-if="result.items.length === 0" class="state"><n-empty description="Kayıt yok." /></div>
    <template v-else>
      <p class="count">{{ result.total }} kayıt</p>
      <div class="table-wrap" :class="{ dim: loading }">
        <table class="activity-table">
          <thead>
            <tr>
              <th>Zaman</th>
              <th>Kişi</th>
              <th>İşlem</th>
              <th>Ayrıntı</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="r in result.items" :key="r.id">
              <td class="when">{{ formatDateTime(r.at) }}</td>
              <td>{{ r.username ?? '—' }}</td>
              <td>{{ actionLabel(r.action) }}</td>
              <td>
                <router-link v-if="activityLink(r)" :to="activityLink(r)!">{{ r.summary || (r.caseId ? `Vaka #${r.caseId}` : 'Aç') }}</router-link>
                <template v-else>{{ r.summary }}</template>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <div v-if="pageCount > 1" class="pages"><n-pagination v-model:page="page" :page-count="pageCount" /></div>
    </template>
  </section>
</template>

<style scoped>
.back {
  margin: 0 0 8px;
  font-size: 14px;
}
.back a {
  color: var(--rs-accent);
}
.heading {
  margin: 0;
  font-size: 30px;
}
.description {
  margin: 6px 0 20px;
  color: var(--rs-muted);
  font-size: 16px;
  max-width: 760px;
}
.filters {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-bottom: 16px;
}
.filter {
  width: 200px;
}
.panel {
  background: var(--rs-surface);
  border: 1px solid var(--rs-border);
  border-radius: 10px;
  padding: 16px 20px;
  margin-bottom: 16px;
}
.state {
  padding: 40px 16px;
}
.count {
  margin: 0 0 8px;
  font-size: 13px;
  color: var(--rs-muted);
}
.table-wrap {
  overflow-x: auto;
}
.dim {
  opacity: 0.6;
}
.activity-table {
  width: 100%;
  min-width: 720px;
  border-collapse: collapse;
  font-size: 14px;
}
.activity-table th {
  text-align: left;
  font-size: 12px;
  letter-spacing: 0.06em;
  text-transform: uppercase;
  color: var(--rs-muted);
  font-weight: 600;
  padding: 10px 12px;
  border-bottom: 1px solid var(--rs-border);
}
.activity-table td {
  padding: 8px 12px;
  border-bottom: 1px solid #eef0f2;
  vertical-align: top;
}
.activity-table a {
  color: var(--rs-accent);
}
.when {
  white-space: nowrap;
  color: var(--rs-muted);
  font-variant-numeric: tabular-nums;
}
.pages {
  display: flex;
  justify-content: center;
  margin-top: 16px;
}
</style>
