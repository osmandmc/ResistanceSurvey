<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { NAlert, NButton, NFormItem, NInput, NInputNumber, NRadioButton, NRadioGroup, NSelect } from 'naive-ui'
import type { CaseFormValue } from '@/caseForm'
import { formatDate } from '@/format'
import { TargetType, resultOptions, searchCases, type CaseFormOptions, type CaseOption, type Option } from '@/reviewApi'
import CompanyPicker from './CompanyPicker.vue'

/** The "Vaka (Direniş)" part of the case form: every field of the old form, in the same order. */
const props = defineProps<{
  options: CaseFormOptions
  /** Field path ("case.categoryId") → message, from the server. */
  errors: Record<string, string>
  /** Names of the chosen companies, so the pickers show them (editing a case). */
  companyName?: string | null
  mainCompanyName?: string | null
  /** The duplicate warning, for a new case only (an existing case would find itself). */
  warnAboutExistingCases?: boolean
  /** Fields still holding what "AI ile doldur" put there: marked "AI" so the researcher checks them. */
  aiFields?: string[]
  /** Companies the AI named that are not registered: offered as "add" under the pickers. */
  suggestedCompanyName?: string | null
  suggestedMainCompanyName?: string | null
}>()

const value = defineModel<CaseFormValue>({ required: true })

const emit = defineEmits<{
  /** The researcher chose to add a protest to an existing case of this company instead of opening a new one. */
  useExisting: [existing: CaseOption]
}>()

const toOptions = (list: Option[]) => list.map((o) => ({ label: o.name, value: o.id }))

const targetOptions = [
  { label: 'Şirket', value: TargetType.Company },
  { label: 'Kurum', value: TargetType.Corporation },
]
const yesNo = [
  { label: 'Hayır', value: false },
  { label: 'Evet', value: true },
]
const rightOptions = [
  { label: 'Hak Geliştirme', value: true },
  { label: 'Hak Savunma', value: false },
]

const againstCompany = computed(() => value.value.targetType === TargetType.Company)
const againstCorporation = computed(() => value.value.targetType === TargetType.Corporation)

// The union-authority field appears once a union is among the chosen corporations.
const tradeUnionIds = computed(() => new Set(props.options.corporations.filter((c) => c.isTradeUnion).map((c) => c.id)))
const hasTradeUnion = computed(() => value.value.corporations.some((c) => typeof c === 'number' && tradeUnionIds.value.has(c)))

// Clear fields that no longer apply, so they are not saved by accident.
watch(againstCompany, (yes) => {
  if (!yes) Object.assign(value.value, { companyId: null, isOutsource: false, mainCompanyId: null })
})
watch(againstCorporation, (yes) => {
  if (!yes) value.value.targetCorporationId = null
})
watch(
  () => value.value.isOutsource,
  (yes) => {
    if (!yes) value.value.mainCompanyId = null
  },
)
watch(hasTradeUnion, (yes) => {
  if (!yes) value.value.tradeUnionAuthorityId = null
})

// Duplicate guard: once a company is chosen, show its existing cases (ongoing first).
const existingCases = ref<CaseOption[]>([])
watch(
  () => (againstCompany.value ? value.value.companyId : null),
  async (companyId) => {
    existingCases.value = []
    if (companyId === null || props.warnAboutExistingCases === false) return
    try {
      existingCases.value = await searchCases({ companyId })
    } catch {
      // only a hint; the form works without it
    }
  },
)

const ai = (field: string) => (props.aiFields?.includes(field) ? 'ai' : undefined)

function error(field: string) {
  return props.errors[`case.${field}`]
}
function status(field: string) {
  return error(field) ? ('error' as const) : undefined
}
</script>

<template>
  <fieldset class="section">
    <legend class="section-title">Vaka (Direniş)</legend>
    <div class="grid">
      <n-form-item label="Vaka İsmi" label-placement="top" class="wide" :show-feedback="false">
        <n-input v-model:value="value.name" placeholder="Örn. Örnek Tekstil işçilerinin ücret direnişi" />
      </n-form-item>

      <n-form-item :class="ai('description')" label="Kısa Açıklama" label-placement="top" class="wide" required :validation-status="status('description')" :feedback="error('description')">
        <n-input v-model:value="value.description" type="textarea" :autosize="{ minRows: 3, maxRows: 8 }" />
      </n-form-item>

      <n-form-item :class="ai('categoryId')" label="Vaka Niteliği" label-placement="top" required :validation-status="status('categoryId')" :feedback="error('categoryId')">
        <n-select v-model:value="value.categoryId" :options="toOptions(options.categories)" filterable placeholder="Seçiniz" />
      </n-form-item>

      <n-form-item :class="ai('targetType')" label="Kime Karşı" label-placement="top" required :validation-status="status('targetType')" :feedback="error('targetType')">
        <n-select v-model:value="value.targetType" :options="targetOptions" placeholder="Seçiniz" />
      </n-form-item>

      <template v-if="againstCompany">
        <n-form-item :class="ai('isOutsource')" label="Şirket Taşeron mu?" label-placement="top" :show-feedback="false">
          <n-radio-group v-model:value="value.isOutsource" name="is-outsource">
            <n-radio-button v-for="o in yesNo" :key="o.label" :value="o.value" :label="o.label" />
          </n-radio-group>
        </n-form-item>
        <n-form-item :class="ai('companyId')" label="Şirket" label-placement="top" class="wide" :validation-status="status('companyId')" :feedback="error('companyId')">
          <company-picker
            v-model="value.companyId"
            input-id="company"
            :selected-name="props.companyName"
            :suggested-name="props.suggestedCompanyName"
            add-label="Şirket Ekle"
            :company-types="options.companyTypes"
            :company-scales="options.companyScales"
            :company-worklines="options.companyWorklines"
            :status="status('companyId')"
          />
        </n-form-item>
        <n-alert v-if="existingCases.length" type="warning" :show-icon="false" class="wide existing">
          <p class="existing-text">
            Bu şirket için {{ existingCases.length }} vaka var. Aynı mücadeleyse yeni vaka yerine mevcut vakaya eylem ekleyin:
          </p>
          <ul class="existing-list">
            <li v-for="c in existingCases.slice(0, 3)" :key="c.id">
              <span>
                <strong>{{ c.name || `Vaka #${c.id}` }}</strong>
                · {{ c.ongoing ? 'devam ediyor' : `son eylem ${formatDate(c.lastProtestDate)}` }}
              </span>
              <n-button size="small" @click="emit('useExisting', c)">Bu vakaya eylem ekle</n-button>
            </li>
          </ul>
        </n-alert>
        <n-form-item v-if="value.isOutsource" :class="ai('mainCompanyId')" label="Ana Şirket" label-placement="top" class="wide" required :validation-status="status('mainCompanyId')" :feedback="error('mainCompanyId')">
          <company-picker
            v-model="value.mainCompanyId"
            input-id="main-company"
            :selected-name="props.mainCompanyName"
            :suggested-name="props.suggestedMainCompanyName"
            add-label="Ana Şirket Ekle"
            :company-types="options.companyTypes"
            :company-scales="options.companyScales"
            :company-worklines="options.companyWorklines"
            :status="status('mainCompanyId')"
          />
        </n-form-item>
      </template>

      <n-form-item v-if="againstCorporation" :class="ai('targetCorporationId')" label="Hedef Kurum" label-placement="top" class="wide" :show-feedback="false">
        <n-select v-model:value="value.targetCorporationId" :options="toOptions(options.corporations)" filterable clearable placeholder="Seçiniz" />
      </n-form-item>

      <n-form-item :class="ai('reasons')" label="Vaka Nedeni" label-placement="top" class="wide" required :validation-status="status('reasonIds')" :feedback="error('reasonIds') || 'Listede yoksa yazıp Enter’a basarak ekleyebilirsiniz.'">
        <n-select v-model:value="value.reasons" :options="toOptions(options.reasons)" multiple filterable tag placeholder="Seçiniz veya yazın" />
      </n-form-item>

      <n-form-item label="Hak Geliştirme/Hak Savunma Özelliği" label-placement="top" required :validation-status="status('developRight')" :feedback="error('developRight')">
        <n-radio-group v-model:value="value.developRight" name="develop-right">
          <n-radio-button v-for="o in rightOptions" :key="o.label" :value="o.value" :label="o.label" />
        </n-radio-group>
      </n-form-item>

      <n-form-item label="Sonuç" label-placement="top" required :validation-status="status('result')" :feedback="error('result')">
        <n-select v-model:value="value.result" :options="resultOptions" />
      </n-form-item>

      <n-form-item :class="ai('employeeCount')" label="İş Yerindeki İşçi Sayısı (Tam)" label-placement="top" :show-feedback="false">
        <n-input-number v-model:value="value.employeeCount" :min="0" clearable class="full" />
      </n-form-item>

      <n-form-item label="İş Yerindeki İşçi Sayısı" label-placement="top" :show-feedback="false">
        <n-select v-model:value="value.employeeCountRangeId" :options="toOptions(options.employeeCountRanges)" clearable placeholder="Seçiniz" />
      </n-form-item>

      <n-form-item :class="ai('corporations')" label="Kurumsallık" label-placement="top" class="wide" required :validation-status="status('corporationIds')" :feedback="error('corporationIds') || 'Listede yoksa yazıp Enter’a basarak ekleyebilirsiniz.'">
        <n-select v-model:value="value.corporations" :options="toOptions(options.corporations)" multiple filterable tag placeholder="Seçiniz veya yazın" />
      </n-form-item>

      <n-form-item v-if="hasTradeUnion" label="Sendikanın Yetki Durumu" label-placement="top" :show-feedback="false">
        <n-select v-model:value="value.tradeUnionAuthorityId" :options="toOptions(options.tradeUnionAuthorities)" clearable placeholder="Seçiniz" />
      </n-form-item>

      <n-form-item label="Tepki Gösterilen Sendika" label-placement="top" :show-feedback="false">
        <n-select v-model:value="value.tradeUnionId" :options="toOptions(options.tradeUnions)" filterable clearable placeholder="Seçiniz" />
      </n-form-item>

      <n-form-item :class="ai('employmentTypeIds')" label="İstihdam Türü" label-placement="top" class="wide" required :validation-status="status('employmentTypeIds')" :feedback="error('employmentTypeIds')">
        <n-select v-model:value="value.employmentTypeIds" :options="toOptions(options.employmentTypes)" multiple filterable placeholder="Seçiniz" />
      </n-form-item>

      <n-form-item :class="ai('firedEmployeeCount')" label="Mücadele Ettiği için İşten Atılan İşçi Sayısı" label-placement="top" :show-feedback="false">
        <n-input-number v-model:value="value.firedEmployeeCount" :min="0" clearable class="full" />
      </n-form-item>
    </div>
  </fieldset>
</template>

<style scoped>
/* "AI" after the label of a field filled by "AI ile doldur" and not changed since. */
.ai :deep(.n-form-item-label__text)::after {
  content: 'AI';
  margin-left: 6px;
  padding: 0 6px;
  border-radius: 999px;
  font-size: 11px;
  font-weight: 600;
  letter-spacing: 0.04em;
  color: #fff;
  background: #6d4bc4;
}
.section {
  border: 0;
  border-top: 1px solid var(--rs-border);
  margin: 0;
  padding: 16px 0 0;
}
.section-title {
  font-size: 17px;
  font-weight: 700;
  padding-right: 10px;
}
.grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
  gap: 0 18px;
}
.wide {
  grid-column: 1 / -1;
}
.full {
  width: 100%;
}
.existing {
  margin-bottom: 16px;
}
.existing-text {
  margin: 0 0 8px;
}
.existing-list {
  margin: 0;
  padding: 0;
  list-style: none;
  display: flex;
  flex-direction: column;
  gap: 6px;
}
.existing-list li {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
}
</style>
