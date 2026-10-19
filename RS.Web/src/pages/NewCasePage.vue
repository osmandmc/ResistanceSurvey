<script setup lang="ts">
import { nextTick, ref } from 'vue'
import { useRouter } from 'vue-router'
import { NAlert, NButton, NSpin, useMessage } from 'naive-ui'
import { ApiError } from '@/api'
import { emptyCase, toCaseInput, type CaseFormValue } from '@/caseForm'
import { createCase } from '@/casesApi'
import CaseForm from '@/components/CaseForm.vue'
import ProtestForm from '@/components/ProtestForm.vue'
import { emptyProtest, toProtestInput, type ProtestFormValue } from '@/protestForm'
import { getFormOptions, type CaseFormOptions, type CaseOption } from '@/reviewApi'

/** A new case with its first protest, without an article (cases usually start from the inbox instead). */
const router = useRouter()
const message = useMessage()

const options = ref<CaseFormOptions | null>(null)
const optionsError = ref(false)
async function loadOptions() {
  optionsError.value = false
  try {
    options.value = await getFormOptions()
  } catch {
    optionsError.value = true
  }
}
void loadOptions()

const caseValue = ref<CaseFormValue>(emptyCase())
const protestValue = ref<ProtestFormValue>(emptyProtest())
const errors = ref<Record<string, string>>({})
const saving = ref(false)

async function save() {
  saving.value = true
  errors.value = {}
  try {
    const result = await createCase({ case: toCaseInput(caseValue.value), protest: toProtestInput(protestValue.value) })
    message.success('Vaka kaydedildi.')
    await router.push(`/cases/${result.id}`)
  } catch (error) {
    if (error instanceof ApiError && error.status === 400) {
      errors.value = error.fields
      await nextTick()
      document.querySelector('.n-form-item-feedback--error')?.scrollIntoView({ behavior: 'smooth', block: 'center' })
    }
    message.error(error instanceof ApiError ? error.message : 'Vaka kaydedilemedi.')
  } finally {
    saving.value = false
  }
}

// The duplicate warning's "Bu vakaya eylem ekle": go to that case, where protests are added.
async function onUseExisting(existing: CaseOption) {
  message.info('Mevcut vakaya eylem eklemek için vaka sayfasında “Eylem Ekle”yi kullanın.')
  await router.push(`/cases/${existing.id}`)
}
</script>

<template>
  <router-link to="/cases" class="back">← Vakalar</router-link>
  <h1 class="heading">Yeni vaka</h1>
  <p class="description">Habersiz yeni bir vaka ve ilk eylemi. Haberden vaka açmak için Gelen Kutusu’nu kullanın.</p>

  <n-alert v-if="optionsError" type="error" :show-icon="false">
    Form listeleri yüklenemedi.
    <n-button size="small" class="retry" @click="loadOptions">Tekrar dene</n-button>
  </n-alert>
  <div v-else-if="!options" class="state"><n-spin /></div>
  <section v-else class="panel">
    <case-form v-model="caseValue" :options="options" :errors="errors" @use-existing="onUseExisting" />
    <protest-form v-model="protestValue" :options="options" :errors="errors" />
    <div class="actions">
      <n-button type="primary" :loading="saving" @click="save">Kaydet</n-button>
      <n-button quaternary :disabled="saving" @click="router.push('/cases')">Vazgeç</n-button>
    </div>
  </section>
</template>

<style scoped>
.back {
  display: inline-block;
  margin-bottom: 16px;
  font-weight: 500;
}
.heading {
  margin: 0;
  font-size: 28px;
}
.description {
  margin: 6px 0 20px;
  color: var(--rs-muted);
}
.state {
  padding: 32px 16px;
}
.retry {
  margin-left: 12px;
}
.panel {
  background: var(--rs-surface);
  border: 1px solid var(--rs-border);
  border-radius: 10px;
  padding: 20px 24px;
}
.actions {
  display: flex;
  gap: 10px;
  padding-top: 16px;
}
</style>
