<script setup lang="ts">
import { NButton, NTag } from 'naive-ui'
import type { RouteLocationRaw } from 'vue-router'
import { formatDate } from '@/format'
import type { InboxItem } from '@/inboxApi'
import { verdictText, verdictType } from '@/relevance'

/** One page of inbox articles with the AI's verdict under each title. */
const props = defineProps<{
  items: InboxItem[]
  /** The inbox's filters, carried to the review screen and back. */
  query: Record<string, string>
}>()

const emit = defineEmits<{ dismiss: [item: InboxItem] }>()

const review = (item: InboxItem): RouteLocationRaw => ({ path: `/review/${encodeURIComponent(item.articleId)}`, query: props.query })
</script>

<template>
  <div class="table-wrap">
    <table class="inbox-table">
      <thead>
        <tr>
          <th>Başlık</th>
          <th>Kaynak</th>
          <th>Tarih</th>
          <th><span class="visually-hidden">İşlemler</span></th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="item in items" :key="item.articleId">
          <td class="title">
            <a :href="item.url" target="_blank" rel="noopener">{{ item.title || item.url }}</a>
            <div v-if="item.ai" class="ai" :title="item.ai.reason ?? undefined">
              <n-tag :type="verdictType[item.ai.verdict]" size="small" :bordered="false">AI: {{ verdictText[item.ai.verdict] }}</n-tag>
              <span v-if="item.ai.verdict !== 'Relevant' && item.ai.reason" class="reason">{{ item.ai.reason }}</span>
            </div>
            <div v-else class="ai"><span class="reason">AI değerlendirmedi</span></div>
          </td>
          <td>{{ item.sourceName }}</td>
          <td class="nowrap">{{ formatDate(item.publishedAt) }}</td>
          <td class="actions">
            <router-link :to="review(item)" custom v-slot="{ navigate }">
              <n-button type="primary" size="small" @click="navigate">Aç</n-button>
            </router-link>
            <n-button size="small" @click="emit('dismiss', item)">İlgisiz</n-button>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<style scoped>
.table-wrap {
  overflow-x: auto;
}
.inbox-table {
  width: 100%;
  min-width: 720px;
  border-collapse: collapse;
}
.inbox-table th {
  text-align: left;
  font-size: 12px;
  letter-spacing: 0.06em;
  text-transform: uppercase;
  color: var(--rs-muted);
  font-weight: 600;
  padding: 12px 16px;
  border-bottom: 1px solid var(--rs-border);
}
.inbox-table td {
  padding: 14px 16px;
  border-bottom: 1px solid #eef0f2;
  font-size: 15px;
  vertical-align: middle;
}
.title {
  font-weight: 500;
}
.title a {
  color: var(--rs-ink);
  text-decoration: none;
}
.title a:hover {
  text-decoration: underline;
}
.ai {
  display: flex;
  flex-wrap: wrap;
  align-items: baseline;
  gap: 4px 8px;
  margin-top: 4px;
  font-weight: 400;
}
.reason {
  font-size: 13px;
  color: var(--rs-muted);
}
.nowrap {
  white-space: nowrap;
}
.actions {
  white-space: nowrap;
  text-align: right;
}
.actions > * + * {
  margin-left: 8px;
}
.visually-hidden {
  position: absolute;
  width: 1px;
  height: 1px;
  overflow: hidden;
  clip: rect(0 0 0 0);
}
</style>
