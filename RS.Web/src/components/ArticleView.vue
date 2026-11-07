<script setup lang="ts">
import { ref, watch } from 'vue'
import type { TextSegment } from '@/evidence'
import { formatDate } from '@/format'
import type { ReviewArticle } from '@/reviewApi'

/**
 * A pipeline article as the review screen shows it: source, date, link, title, picture and text. The review screen
 * passes the AI evidence marks (segments) and puts its AI lines in the slot after the title.
 */
const props = withDefaults(
  defineProps<{
    article: Pick<ReviewArticle, 'articleId' | 'title' | 'url' | 'sourceName' | 'publishedAt' | 'body' | 'imageUrl'>
    titleSegments?: TextSegment[] | null
    bodySegments?: TextSegment[] | null
    activeEvidence?: string | null
    /** h1 on the review screen; lower inside a page that has its own heading. */
    headingLevel?: 'h1' | 'h3'
  }>(),
  { titleSegments: null, bodySegments: null, activeEvidence: null, headingLevel: 'h1' },
)

const imageFailed = ref(false)
watch(() => props.article.articleId, () => (imageFailed.value = false))
</script>

<template>
  <div class="meta">
    <span>{{ article.sourceName }}</span>
    <span>{{ formatDate(article.publishedAt) }}</span>
    <a :href="article.url" target="_blank" rel="noopener">Kaynağa git ↗</a>
  </div>
  <component :is="headingLevel" class="article-title" :class="headingLevel">
    <template v-if="article.title">
      <template v-if="titleSegments">
        <template v-for="(s, i) in titleSegments" :key="i">
          <mark v-if="s.fields.length" class="evidence" :class="{ active: s.fields.includes(activeEvidence ?? '') }" :data-evidence="s.fields.join(' ')" :title="s.labels.join(', ')">{{ s.text }}</mark>
          <template v-else>{{ s.text }}</template>
        </template>
      </template>
      <template v-else>{{ article.title }}</template>
    </template>
    <template v-else>{{ article.url }}</template>
  </component>
  <slot />
  <!-- From the news site itself (not copied); hidden if the site no longer serves it. -->
  <img
    v-if="article.imageUrl && !imageFailed"
    :src="article.imageUrl"
    alt=""
    class="article-image"
    loading="lazy"
    referrerpolicy="no-referrer"
    @error="imageFailed = true"
  />
  <p v-if="article.body" class="body">
    <template v-if="bodySegments">
      <template v-for="(s, i) in bodySegments" :key="i">
        <mark v-if="s.fields.length" class="evidence" :class="{ active: s.fields.includes(activeEvidence ?? '') }" :data-evidence="s.fields.join(' ')" :title="s.labels.join(', ')">{{ s.text }}</mark>
        <template v-else>{{ s.text }}</template>
      </template>
    </template>
    <template v-else>{{ article.body }}</template>
  </p>
  <p v-else class="no-body">Haberin metni henüz okunmadı; kaynağa giderek okuyabilirsiniz.</p>
</template>

<style scoped>
.meta {
  display: flex;
  flex-wrap: wrap;
  gap: 8px 16px;
  font-size: 13px;
  color: var(--rs-muted);
}
.article-title {
  margin: 12px 0 16px;
  font-size: 24px;
  line-height: 1.3;
}
.article-title.h3 {
  font-size: 19px;
}
.article-image {
  display: block;
  width: 100%;
  max-height: 360px;
  object-fit: cover;
  border-radius: 8px;
  margin: 0 0 16px;
}
.body {
  margin: 0;
  font-size: 16px;
  line-height: 1.7;
  white-space: pre-line;
}
.no-body {
  color: var(--rs-muted);
}
.evidence {
  background: #efeafb;
  color: inherit;
  border-bottom: 2px solid #b8a6ec;
  border-radius: 2px;
  padding: 0 1px;
  transition: background 0.3s;
}
.evidence.active {
  background: #d9cdf7;
  border-bottom-color: #6d4bc4;
}
</style>
