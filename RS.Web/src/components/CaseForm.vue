<script setup lang="ts">
import { turkishFilter } from '@/search'
import { computed, ref, watch } from 'vue'
import { NAlert, NButton, NCheckbox, NFormItem, NInput, NInputNumber, NRadioButton, NRadioGroup, NSelect } from 'naive-ui'
import type { CaseFormValue } from '@/caseForm'
import { formatDate } from '@/format'
import {
  CaseCategory,
  EmployerRole,
  employerRoleOptions,
  hasAbove,
  resultOptions,
  searchCases,
  type CaseFormOptions,
  type CaseOption,
  type CorporationOption,
  type Option,
  type WorkplaceOption,
} from '@/reviewApi'
import CaseSearchSelect from './CaseSearchSelect.vue'
import CompanyPicker from './CompanyPicker.vue'
import CorporationAddDialog from './CorporationAddDialog.vue'
import WorkplacePicker from './WorkplacePicker.vue'

/**
 * The "Vaka (Direniş)" part of the case form. The category comes first and decides the fields (R31): İşletme/Kurum,
 * a workplace's struggle (workplace, main company, organizations, union, counts, result); Genel, an action
 * organizations called without a workplace (organizers; the issue is the case reason); Dayanışma, an action supporting
 * another struggle (organizers, optionally the workplace whose workers acted, and the supported case).
 */
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
  /** Name of the supported case (editing a Dayanışma case). */
  supportedCaseName?: string | null
  /** Name of the case's workplace (editing, R37). */
  workplaceName?: string | null
}>()

const value = defineModel<CaseFormValue>({ required: true })

const emit = defineEmits<{
  /** The researcher chose to add a protest to an existing case of this company instead of opening a new one. */
  useExisting: [existing: CaseOption]
}>()

const toOptions = (list: Option[]) => list.map((o) => ({ label: o.name, value: o.id }))

// The three categories in a fixed order, named as the list names them.
const categoryOptions = computed(() =>
  [CaseCategory.Workplace, CaseCategory.General, CaseCategory.Solidarity].map((id) => ({
    value: id,
    label: props.options.categories.find((c) => c.id === id)?.name ?? String(id),
  })),
)
const categoryHelp: Record<number, string> = {
  [CaseCategory.Workplace]: 'Bir işverenin işçilerinin mücadelesi: işveren, işyeri, çalışma biçimi, kurumsallık, istihdam türü, sonuç.',
  [CaseCategory.General]: 'İşyeri olmadan kurumların örgütlediği eylem: 1 Mayıs, asgari ücret, vergi … Gündem vaka nedeninde.',
  [CaseCategory.Solidarity]: 'Başka bir mücadeleye destek eylemi: örgütleyen kurumlar ve (varsa) desteklenen vaka.',
}
const rightOptions = [
  { label: 'Hak Geliştirme', value: true },
  { label: 'Hak Savunma', value: false },
]

const workplace = computed(() => value.value.categoryId === CaseCategory.Workplace)
const solidarity = computed(() => value.value.categoryId === CaseCategory.Solidarity)
const general = computed(() => value.value.categoryId === CaseCategory.General)

// The union-authority field appears once a union is among the chosen corporations.
const tradeUnionIds = computed(() => new Set(props.options.corporations.filter((c) => c.isTradeUnion).map((c) => c.id)))
const hasTradeUnion = computed(() => value.value.corporations.some((c) => typeof c === 'number' && tradeUnionIds.value.has(c)))

// Clear fields that no longer apply, so they are not saved by accident. A Dayanışma case keeps a workplace (its workers
// acted); a Genel case has none.
watch(workplace, (yes) => {
  if (!yes)
    Object.assign(value.value, {
      isOutsource: false, mainCompanyId: null, employmentTypeIds: [], employeeCount: null, employeeCountRangeId: null,
      firedEmployeeCount: null, tradeUnionId: null, tradeUnionAuthorityId: null, result: 0,
      employerRole: EmployerRole.Direct, employerUnknown: false, workplaceId: null,
    })
})
watch(general, (yes) => {
  if (yes) value.value.companyId = null
})
watch(solidarity, (yes) => {
  if (!yes) value.value.supportedCaseId = null
})
// R37: how the employer works decides whether an organization is above it; the old “Taşeron mu?” follows.
const role = computed(() => employerRoleOptions.find((o) => o.value === value.value.employerRole) ?? employerRoleOptions[4])
const above = computed(() => hasAbove(value.value.employerRole))
watch(
  () => value.value.employerRole,
  (r) => {
    value.value.isOutsource = hasAbove(r)
    if (!hasAbove(r)) value.value.mainCompanyId = null
  },
)
watch(
  () => value.value.employerUnknown,
  (unknown) => {
    if (unknown) value.value.companyId = null
  },
)

// Names of the chosen employer and organization above: the new-workplace dialog says whom a new place belongs to.
const employerName = ref<string | null>(null)
const aboveName = ref<string | null>(null)
const workplaceOwner = computed(() =>
  above.value ? { id: value.value.mainCompanyId, name: aboveName.value } : { id: value.value.companyId, name: employerName.value },
)
// The places to suggest: those of the organization above, else of the employer.
const suggestPlacesFor = computed(() => (above.value ? value.value.mainCompanyId ?? value.value.companyId : value.value.companyId))
const workplacePlaceholder = computed(() =>
  value.value.employerRole === EmployerRole.Direct && value.value.companyId !== null
    ? 'İşverenle aynı (isteğe bağlı: daha belirli bir yer seçin)'
    : 'Fabrika, hastane, şantiye, maden …',
)

// A place that belongs to someone suggests the organization above the employer, when that is still empty.
const aboveSuggested = ref(false)
function onWorkplacePicked(place: WorkplaceOption | null) {
  if (place?.ownerCompanyId && above.value && value.value.mainCompanyId === null && place.ownerCompanyId !== value.value.companyId) {
    value.value.mainCompanyId = place.ownerCompanyId
    aboveName.value = place.ownerName
    aboveSuggested.value = true
  }
}
watch(
  () => value.value.mainCompanyId,
  () => (aboveSuggested.value = false),
)

// “Taşeron” and the employment type should agree (Özel or Kamu taşeronu).
const subcontractTypeIds = computed(
  () => new Set(props.options.employmentTypes.filter((t) => t.name.toLocaleLowerCase('tr-TR').includes('taşeron')).map((t) => t.id)),
)
const employmentWarning = computed(() => {
  if (!workplace.value || value.value.employmentTypeIds.length === 0) return null
  const saysSubcontract = value.value.employmentTypeIds.some((id) => subcontractTypeIds.value.has(id))
  if (value.value.employerRole === EmployerRole.Subcontractor && !saysSubcontract)
    return 'İşveren taşeron: istihdam türünde “Özel taşeronu” ya da “Kamu taşeronu” beklenir.'
  if (saysSubcontract && value.value.employerRole !== EmployerRole.Subcontractor && value.value.employerRole !== EmployerRole.Unknown)
    return 'İstihdam türü taşeron diyor; işverenin çalışma biçimi de “Taşeron” olmalı mı?'
  return null
})
watch(hasTradeUnion, (yes) => {
  if (!yes) value.value.tradeUnionAuthorityId = null
})

// Duplicate guard: once a company is chosen, show its existing cases (ongoing first).
const existingCases = ref<CaseOption[]>([])
watch(
  () => (workplace.value ? value.value.companyId : null),
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

// "Kurum Ekle" (R34): the new (or existing) organization joins the list and the field.
const addingCorporation = ref(false)
function onCorporationCreated(corporation: CorporationOption) {
  if (!props.options.corporations.some((c) => c.id === corporation.id)) props.options.corporations.push(corporation)
  if (!value.value.corporations.includes(corporation.id)) value.value.corporations = [...value.value.corporations, corporation.id]
}

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
      <n-form-item :class="ai('categoryId')" label="Vaka Niteliği" label-placement="top" class="wide" required :validation-status="status('categoryId')" :feedback="error('categoryId') || (value.categoryId ? categoryHelp[value.categoryId] : 'Önce niteliği seçin: form ona göre alanları gösterir.')">
        <n-radio-group v-model:value="value.categoryId" name="category">
          <n-radio-button v-for="o in categoryOptions" :key="o.value" :value="o.value" :label="o.label" />
        </n-radio-group>
      </n-form-item>

      <n-form-item label="Vaka İsmi" label-placement="top" class="wide" :show-feedback="false">
        <n-input v-model:value="value.name" placeholder="Örn. Örnek Tekstil işçilerinin ücret direnişi" />
      </n-form-item>

      <n-form-item :class="ai('description')" label="Kısa Açıklama" label-placement="top" class="wide" required :validation-status="status('description')" :feedback="error('description')">
        <n-input v-model:value="value.description" type="textarea" :autosize="{ minRows: 3, maxRows: 8 }" />
      </n-form-item>
      <n-form-item label="Kişisel Notlar (vaka)" label-placement="top" class="wide" :show-feedback="false">
        <n-input v-model:value="value.note" type="textarea" :autosize="{ minRows: 2, maxRows: 6 }" placeholder="Vakaya dair notunuz; her eylemin kendi notu da var." />
      </n-form-item>

      <template v-if="workplace">
        <n-form-item :class="ai('companyId')" label="İşveren" label-placement="top" class="wide" :required="!value.employerUnknown" :validation-status="status('companyId')" :feedback="error('companyId') || 'İşçileri çalıştıran şirket ya da kurum.'">
          <div class="employer">
            <company-picker
              v-if="!value.employerUnknown"
              v-model="value.companyId"
              input-id="company"
              :selected-name="props.companyName"
              :suggested-name="props.suggestedCompanyName"
              add-label="İşveren Ekle"
              :company-types="options.companyTypes"
              :company-scales="options.companyScales"
              :company-worklines="options.companyWorklines"
              :status="status('companyId')"
              @named="employerName = $event"
            />
            <n-checkbox v-model:checked="value.employerUnknown">İşveren bilinmiyor (haber işverenin adını vermiyor)</n-checkbox>
          </div>
        </n-form-item>
        <n-alert v-if="existingCases.length" type="warning" :show-icon="false" class="wide existing">
          <p class="existing-text">
            Bu işveren için {{ existingCases.length }} vaka var. Aynı mücadeleyse yeni vaka yerine mevcut vakaya eylem ekleyin:
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

        <n-form-item label="İşyeri" label-placement="top" class="wide" :validation-status="status('workplaceId')" :feedback="error('workplaceId') || 'İşin yapıldığı yer (isteğe bağlı). Doğrudan işverende boş bırakılırsa işverenin kendisidir.'">
          <workplace-picker
            v-model="value.workplaceId"
            input-id="workplace"
            :selected-name="props.workplaceName"
            :suggest-for="suggestPlacesFor"
            :placeholder="workplacePlaceholder"
            :cities="options.cities"
            :districts="options.districts"
            :owner-company-id="workplaceOwner.id"
            :owner-name="workplaceOwner.name"
            @picked="onWorkplacePicked"
          />
        </n-form-item>

        <n-form-item :class="ai('employerRole')" label="İşveren bu işte nasıl çalışıyor?" label-placement="top" class="wide" :feedback="role.help">
          <n-radio-group v-model:value="value.employerRole" name="employer-role">
            <n-radio-button v-for="o in employerRoleOptions" :key="o.value" :value="o.value" :label="o.label" />
          </n-radio-group>
        </n-form-item>
        <n-form-item v-if="above" :class="ai('mainCompanyId')" :label="role.above ?? ''" label-placement="top" class="wide" required :validation-status="status('mainCompanyId')" :feedback="error('mainCompanyId') || (aboveSuggested ? 'Önerildi: seçilen işyerinin sahibi.' : '')">
          <company-picker
            v-model="value.mainCompanyId"
            input-id="main-company"
            :selected-name="aboveName ?? props.mainCompanyName"
            :suggested-name="props.suggestedMainCompanyName"
            :add-label="`${role.above} Ekle`"
            :company-types="options.companyTypes"
            :company-scales="options.companyScales"
            :company-worklines="options.companyWorklines"
            :status="status('mainCompanyId')"
            @named="aboveName = $event"
          />
        </n-form-item>
      </template>

      <n-form-item :class="ai('reasons')" label="Vaka Nedeni" label-placement="top" class="wide" required :validation-status="status('reasonIds')" :feedback="error('reasonIds') || (general ? 'Gündemi taşır: 1 Mayıs, vergi, asgari ücret … Listede yoksa yazıp Enter’a basın.' : 'Listede yoksa yazıp Enter’a basarak ekleyebilirsiniz.')">
        <n-select v-model:value="value.reasons" :options="toOptions(options.reasons)" multiple filterable :filter="turkishFilter" tag placeholder="Seçiniz veya yazın" />
      </n-form-item>

      <n-form-item v-if="workplace" :class="ai('corporations')" label="Kurumsallık" label-placement="top" class="wide" required :validation-status="status('corporationIds')" :feedback="error('corporationIds') || 'Listede yoksa yazıp Enter’a basarak ekleyebilirsiniz.'">
        <div class="with-add">
          <n-select v-model:value="value.corporations" :options="toOptions(options.corporations)" multiple filterable :filter="turkishFilter" tag placeholder="Seçiniz veya yazın" />
          <n-button @click="addingCorporation = true">Kurum Ekle</n-button>
        </div>
      </n-form-item>
      <n-form-item v-else-if="general || solidarity" :class="ai('corporations')" label="Örgütleyen Kurumlar" label-placement="top" class="wide" :feedback="'Ortak eylemlerde birden fazla. Boş bırakılırsa kurumsuz. Listede yoksa yazıp Enter’a basın.'">
        <div class="with-add">
          <n-select v-model:value="value.corporations" :options="toOptions(options.corporations)" multiple filterable :filter="turkishFilter" tag clearable placeholder="Seçiniz veya yeni kurum yazın" />
          <n-button @click="addingCorporation = true">Kurum Ekle</n-button>
        </div>
      </n-form-item>

      <template v-if="solidarity">
        <n-form-item label="Desteklenen Vaka" label-placement="top" class="wide" :validation-status="status('supportedCaseId')" :feedback="error('supportedCaseId') || 'Vaka henüz kayıtlı değilse boş bırakın; sonra vaka sayfasından bağlanabilir.'">
          <case-search-select v-model="value.supportedCaseId" :selected-name="props.supportedCaseName" />
        </n-form-item>
        <n-form-item label="Eylemi Yapan İşyeri" label-placement="top" class="wide" :feedback="'Yalnızca bir işyerinin işçileri eylem yaptıysa (örn. dayanışma grevi).'">
          <company-picker
            v-model="value.companyId"
            input-id="acting-company"
            :selected-name="props.companyName"
            add-label="İşyeri Ekle"
            :company-types="options.companyTypes"
            :company-scales="options.companyScales"
            :company-worklines="options.companyWorklines"
          />
        </n-form-item>
      </template>

      <n-form-item v-if="value.categoryId" label="Hak Geliştirme/Hak Savunma Özelliği" label-placement="top" required :validation-status="status('developRight')" :feedback="error('developRight')">
        <n-radio-group v-model:value="value.developRight" name="develop-right">
          <n-radio-button v-for="o in rightOptions" :key="o.label" :value="o.value" :label="o.label" />
        </n-radio-group>
      </n-form-item>

      <template v-if="workplace">
        <n-form-item label="Sonuç" label-placement="top" required :validation-status="status('result')" :feedback="error('result')">
          <n-select v-model:value="value.result" :options="resultOptions" />
        </n-form-item>

        <n-form-item :class="ai('employeeCount')" label="İş Yerindeki İşçi Sayısı (Tam)" label-placement="top" :show-feedback="false">
          <n-input-number v-model:value="value.employeeCount" :min="0" clearable class="full" />
        </n-form-item>

        <n-form-item label="İş Yerindeki İşçi Sayısı" label-placement="top" :show-feedback="false">
          <n-select v-model:value="value.employeeCountRangeId" :options="toOptions(options.employeeCountRanges)" clearable placeholder="Seçiniz" />
        </n-form-item>

        <n-form-item v-if="hasTradeUnion" label="Sendikanın Yetki Durumu" label-placement="top" :show-feedback="false">
          <n-select v-model:value="value.tradeUnionAuthorityId" :options="toOptions(options.tradeUnionAuthorities)" clearable placeholder="Seçiniz" />
        </n-form-item>

        <n-form-item label="Tepki Gösterilen Sendika" label-placement="top" :show-feedback="false">
          <n-select v-model:value="value.tradeUnionId" :options="toOptions(options.tradeUnions)" filterable :filter="turkishFilter" clearable placeholder="Seçiniz" />
        </n-form-item>

        <n-form-item :class="ai('employmentTypeIds')" label="İstihdam Türü" label-placement="top" class="wide" required :validation-status="status('employmentTypeIds') ?? (employmentWarning ? 'warning' : undefined)" :feedback="error('employmentTypeIds') || employmentWarning || undefined">
          <n-select v-model:value="value.employmentTypeIds" :options="toOptions(options.employmentTypes)" multiple filterable :filter="turkishFilter" placeholder="Seçiniz" />
        </n-form-item>

        <n-form-item :class="ai('firedEmployeeCount')" label="Mücadele Ettiği için İşten Atılan İşçi Sayısı" label-placement="top" :show-feedback="false">
          <n-input-number v-model:value="value.firedEmployeeCount" :min="0" clearable class="full" />
        </n-form-item>
      </template>
    </div>
    <corporation-add-dialog
      v-model:show="addingCorporation"
      :corporation-types="options.corporationTypes ?? []"
      :confederations="options.confederations ?? []"
      @created="onCorporationCreated"
    />
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
.with-add {
  display: flex;
  gap: 8px;
  width: 100%;
}
.with-add > :first-child {
  flex: 1;
  min-width: 0;
}
.employer {
  display: flex;
  flex-direction: column;
  gap: 6px;
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
