<script setup lang="ts">
import { ref, watch } from 'vue'
import { NButton, NFormItem, NInput, NModal, useMessage } from 'naive-ui'
import { ApiError } from '@/api'
import { dismissArticle } from '@/inboxApi'

/** "Not relevant" with an optional reason, for one article. Used by the inbox and the review screen. */
const props = defineProps<{
  /** The article to dismiss; null closes the dialog. */
  article: { articleId: string; title: string | null } | null
}>()

const emit = defineEmits<{
  /** The article is handled now (dismissed here, or already by someone else): drop it from view. */
  handled: [articleId: string]
  close: []
}>()

const message = useMessage()
const reason = ref('')
const dismissing = ref(false)

watch(
  () => props.article,
  () => (reason.value = ''),
)

function close() {
  if (!dismissing.value) emit('close')
}

async function confirm() {
  const article = props.article
  if (!article) return
  dismissing.value = true
  try {
    const result = await dismissArticle(article.articleId, reason.value)
    if (result.outcomeSent) {
      message.success('İlgisiz olarak işaretlendi.')
    } else {
      message.warning('İlgisiz olarak işaretlendi; işleme hattına daha sonra bildirilecek.')
    }
    emit('handled', article.articleId)
  } catch (error) {
    if (error instanceof ApiError && (error.status === 409 || error.status === 404)) {
      message.warning(error.message)
      emit('handled', article.articleId)
    } else {
      message.error(error instanceof ApiError && error.status === 502 ? error.message : 'İşaretlenemedi.')
    }
  } finally {
    dismissing.value = false
  }
}
</script>

<template>
  <n-modal
    :show="article !== null"
    preset="card"
    title="İlgisiz olarak işaretle"
    :style="{ width: 'min(520px, calc(100vw - 32px))' }"
    :mask-closable="!dismissing"
    @update:show="(show: boolean) => { if (!show) close() }"
  >
    <p class="title">{{ article?.title }}</p>
    <n-form-item label="Neden (isteğe bağlı)" label-placement="top" :show-feedback="false">
      <n-input
        v-model:value="reason"
        type="textarea"
        :autosize="{ minRows: 2, maxRows: 5 }"
        maxlength="500"
        placeholder="Örn. işçi eylemi değil, görüş yazısı"
      />
    </n-form-item>
    <template #footer>
      <div class="actions">
        <n-button :disabled="dismissing" @click="close">Vazgeç</n-button>
        <n-button type="primary" :loading="dismissing" @click="confirm">İlgisiz olarak işaretle</n-button>
      </div>
    </template>
  </n-modal>
</template>

<style scoped>
.title {
  margin: 0 0 16px;
  font-weight: 500;
}
.actions {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
}
</style>
