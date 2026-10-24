<script setup lang="ts">
import { ref, watch } from 'vue'
import { NButton, NFormItem, NInput, NModal, NRadio, NRadioGroup, useMessage } from 'naive-ui'
import { ApiError, errorMessage } from '@/api'
import type { AiUse } from '@/aiFillApi'
import { dismissArticle, dismissKinds, type DismissKind } from '@/inboxApi'

/** "Not relevant" with an optional reason, for one article. Used by the inbox and the review screen. */
const props = defineProps<{
  /** The article to dismiss; null closes the dialog. */
  article: { articleId: string; title: string | null } | null
  /** A reason to start with (the AI's, from "AI ile doldur"); the researcher can change it. */
  suggestedReason?: string | null
  /** The AI answer checked for this article, kept with the dismissal (R19c). */
  ai?: AiUse | null
  /**
   * A reason chosen in advance: only where the researcher acts on the pipeline's own rejection (the inbox's "AI:
   * ilgisiz görünüyor" list, R21). Everywhere else nothing is preselected.
   */
  presetKind?: DismissKind | null
}>()

const emit = defineEmits<{
  /** The article is handled now (dismissed here, or already by someone else): drop it from view. */
  handled: [articleId: string]
  close: []
}>()

const message = useMessage()
const reason = ref('')
// Chosen every time, never preselected: it tells "not labour news" from "already recorded" when the AI is measured.
const kind = ref<DismissKind | null>(null)
const dismissing = ref(false)

watch(
  () => props.article,
  () => {
    reason.value = props.suggestedReason ?? ''
    kind.value = props.presetKind ?? null
  },
)

function close() {
  if (!dismissing.value) emit('close')
}

async function confirm() {
  const article = props.article
  if (!article || kind.value === null) return
  dismissing.value = true
  try {
    const result = await dismissArticle(article.articleId, reason.value, kind.value, props.ai)
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
      message.error(errorMessage(error, 'İşaretlenemedi.'))
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
    <n-form-item label="Neden" label-placement="top" required :show-feedback="false">
      <n-radio-group v-model:value="kind" name="dismiss-kind" class="kinds">
        <n-radio v-for="k in dismissKinds" :key="k.value" :value="k.value" class="kind">
          <span class="kind-label">{{ k.label }}</span>
          <span class="kind-hint">{{ k.hint }}</span>
        </n-radio>
      </n-radio-group>
    </n-form-item>
    <n-form-item label="Açıklama (isteğe bağlı)" label-placement="top" :show-feedback="false">
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
        <n-button type="primary" :loading="dismissing" :disabled="kind === null" @click="confirm">İlgisiz olarak işaretle</n-button>
      </div>
    </template>
  </n-modal>
</template>

<style scoped>
.title {
  margin: 0 0 16px;
  font-weight: 500;
}
.kinds {
  display: flex;
  flex-direction: column;
  gap: 8px;
}
.kind-label {
  display: block;
  font-weight: 500;
}
.kind-hint {
  display: block;
  font-size: 13px;
  color: var(--rs-muted);
}
.actions {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
}
</style>
