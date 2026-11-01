<script setup lang="ts">
import { turkishFilter } from '@/search'
import { computed, watch } from 'vue'
import { NButton, NCheckbox, NDatePicker, NFormItem, NInput, NInputNumber, NSelect } from 'naive-ui'
import {
  noInterventionTypeId,
  simpleActionProtestTypeId,
  strikeProtestTypeIds,
  type ProtestFormValue,
} from '@/protestForm'
import type { CaseFormOptions, Option } from '@/reviewApi'

/** The "Eylem" part of the case form: every field of the old form, in the same order, with the same rules. */
const props = defineProps<{
  options: CaseFormOptions
  /** Field path ("protest.startDate") → message, from the server. */
  errors: Record<string, string>
  /** Fields still holding what "AI ile doldur" put there: marked "AI" so the researcher checks them. */
  aiFields?: string[]
}>()

const value = defineModel<ProtestFormValue>({ required: true })

const toOptions = (list: Option[]) => list.map((o) => ({ label: o.name, value: o.id }))
const cityOptions = computed(() => toOptions(props.options.cities))
function districtOptions(cityId: number | null) {
  return props.options.districts.filter((d) => d.cityId === cityId).map((d) => ({ label: d.name, value: d.id }))
}

const showDuration = computed(() => value.value.protestTypeIds.some((id) => strikeProtestTypeIds.includes(id)))
const showSimpleDescription = computed(() => value.value.protestTypeIds.includes(simpleActionProtestTypeId))
const showCustody = computed(
  () => value.value.interventionTypeIds.length > 0 && !value.value.interventionTypeIds.includes(noInterventionTypeId),
)

// Hidden fields are cleared, so they are not saved by accident.
watch(showDuration, (shown) => {
  if (!shown) Object.assign(value.value, { durationDays: null, durationHours: null, durationMinutes: null })
})
watch(showSimpleDescription, (shown) => {
  if (!shown) value.value.simpleDescription = null
})
watch(showCustody, (shown) => {
  if (!shown) value.value.custodyCount = null
})

function addLocation() {
  value.value.locations.push({ cityId: null, districtId: null, place: null })
}
function removeLocation(index: number) {
  value.value.locations.splice(index, 1)
}
function onCityChange(index: number, cityId: number | null) {
  const location = value.value.locations[index]
  if (location.cityId !== cityId) location.districtId = null
  location.cityId = cityId
}

// The end date may not lie before the start date.
const isBeforeStart = (timestamp: number) => value.value.startDate !== null && timestamp < value.value.startDate

// Most protests last a day: a picked start date fills an empty end date (not when "Eylem sürüyor" is ticked).
watch(
  () => value.value.startDate,
  (start) => {
    if (start !== null && value.value.endDate === null && !value.value.ongoing) value.value.endDate = start
  },
)
// "Eylem sürüyor" clears the end date; unticking it starts again from the start day.
watch(
  () => value.value.ongoing,
  (ongoing) => {
    value.value.endDate = ongoing ? null : value.value.endDate ?? value.value.startDate
  },
)

// The duration is three inputs; it is marked while any of them still holds the AI's value.
const ai = (field: string) =>
  (field === 'duration' ? ['durationDays', 'durationHours'] : [field]).some((f) => props.aiFields?.includes(f)) ? 'ai' : undefined

function error(field: string) {
  return props.errors[`protest.${field}`]
}
function status(field: string) {
  return error(field) ? ('error' as const) : undefined
}
</script>

<template>
  <fieldset class="section">
    <legend class="section-title">Eylem</legend>
    <div class="grid">
      <n-form-item :class="ai('protestTypeIds')" label="Eylem Türleri" label-placement="top" class="wide" required :validation-status="status('protestTypeIds')" :feedback="error('protestTypeIds')">
        <n-select v-model:value="value.protestTypeIds" :options="toOptions(options.protestTypes)" multiple filterable :filter="turkishFilter" placeholder="Seçiniz" />
      </n-form-item>

      <n-form-item v-if="showDuration" :class="ai('duration')" label="Eylemin Süresi" label-placement="top" class="wide" :show-feedback="false">
        <div class="duration">
          <n-input-number v-model:value="value.durationDays" :min="0" placeholder="Gün" clearable aria-label="Gün" />
          <n-input-number v-model:value="value.durationHours" :min="0" :max="23" placeholder="Saat" clearable aria-label="Saat" />
          <n-input-number v-model:value="value.durationMinutes" :min="0" :max="59" placeholder="Dakika" clearable aria-label="Dakika" />
        </div>
      </n-form-item>

      <n-form-item v-if="showSimpleDescription" label="İş yerinde Basit Eylem Açıklama" label-placement="top" class="wide" :show-feedback="false">
        <n-input v-model:value="value.simpleDescription" />
      </n-form-item>

      <n-form-item :class="[ai('startDate'), 'top']" label="Başlangıç Tarihi" label-placement="top" required :validation-status="status('startDate')" :feedback="error('startDate')">
        <n-date-picker v-model:value="value.startDate" type="date" format="dd.MM.yyyy" clearable class="full" />
      </n-form-item>

      <n-form-item :class="[ai('endDate'), 'top']" label="Bitiş Tarihi" label-placement="top" :required="!value.ongoing" :validation-status="status('endDate')" :feedback="error('endDate')">
        <div class="end-date">
          <n-date-picker
            v-model:value="value.endDate"
            type="date"
            format="dd.MM.yyyy"
            clearable
            :disabled="value.ongoing"
            :placeholder="value.ongoing ? 'Sürüyor' : 'Tarih seçin'"
            :is-date-disabled="isBeforeStart"
            class="full"
          />
          <n-checkbox v-model:checked="value.ongoing">Eylem sürüyor</n-checkbox>
        </div>
      </n-form-item>

      <n-form-item :class="ai('placeIds')" label="Eylem Yerleri" label-placement="top" class="wide" required :validation-status="status('placeIds')" :feedback="error('placeIds')">
        <n-select v-model:value="value.placeIds" :options="toOptions(options.protestPlaces)" multiple filterable :filter="turkishFilter" placeholder="Seçiniz" />
      </n-form-item>

      <n-form-item label="Cinsiyet" label-placement="top" required :validation-status="status('genderId')" :feedback="error('genderId')">
        <n-select v-model:value="value.genderId" :options="toOptions(options.genders)" placeholder="Seçiniz" />
      </n-form-item>

      <n-form-item :class="ai('locations')" label="Lokasyonlar" label-placement="top" class="wide" :validation-status="status('locations')" :feedback="error('locations')">
        <div class="locations">
          <div v-for="(location, index) in value.locations" :key="index" class="location">
            <n-select
              :value="location.cityId"
              :options="cityOptions"
              filterable :filter="turkishFilter"
              placeholder="İl"
              aria-label="İl"
              @update:value="(cityId: number | null) => onCityChange(index, cityId)"
            />
            <n-select
              v-model:value="location.districtId"
              :options="districtOptions(location.cityId)"
              :disabled="location.cityId === null"
              filterable :filter="turkishFilter"
              placeholder="İlçe"
              aria-label="İlçe"
            />
            <n-input v-model:value="location.place" placeholder="Yer (örn. fabrika önü)" aria-label="Yer" />
            <n-button quaternary aria-label="Lokasyonu kaldır" @click="removeLocation(index)">Kaldır</n-button>
          </div>
          <n-button class="add-location" @click="addLocation">Lokasyon Ekle</n-button>
        </div>
      </n-form-item>

      <n-form-item :class="ai('employeeCount')" label="Eylemdeki İşçi Sayısı (Tam)" label-placement="top" :show-feedback="false">
        <n-input-number v-model:value="value.employeeCount" :min="0" clearable class="full" />
      </n-form-item>

      <n-form-item :class="ai('employeeCountRangeId')" label="Eylemdeki İşçi Sayısı" label-placement="top" :show-feedback="false">
        <n-select v-model:value="value.employeeCountRangeId" :options="toOptions(options.protestEmployeeCountRanges)" clearable placeholder="Seçiniz" />
      </n-form-item>

      <n-form-item :class="ai('interventionTypeIds')" label="Müdahale Tipi" label-placement="top" class="wide" required :validation-status="status('interventionTypeIds')" :feedback="error('interventionTypeIds')">
        <n-select v-model:value="value.interventionTypeIds" :options="toOptions(options.interventionTypes)" multiple filterable :filter="turkishFilter" placeholder="Seçiniz" />
      </n-form-item>

      <n-form-item v-if="showCustody" :class="ai('custodyCount')" label="Gözaltı Sayısı" label-placement="top" required :validation-status="status('custodyCount')" :feedback="error('custodyCount') || 'Gözaltı yoksa 0 yazın.'">
        <n-input-number v-model:value="value.custodyCount" :min="0" clearable class="full" />
      </n-form-item>

      <n-form-item label="Kontrol Kişisine Notlar (eylem)" label-placement="top" class="wide" :show-feedback="false">
        <n-input v-model:value="value.note" type="textarea" :autosize="{ minRows: 2, maxRows: 6 }" />
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
/* The end date has "Eylem sürüyor" under it: both dates keep to the top of their row. */
.top {
  align-self: start;
}
.full {
  width: 100%;
}
.end-date {
  display: flex;
  flex-direction: column;
  gap: 6px;
  width: 100%;
}
.duration {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 8px;
  width: 100%;
}
.locations {
  display: flex;
  flex-direction: column;
  gap: 8px;
  width: 100%;
}
.location {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: 8px;
  align-items: center;
}
.add-location {
  align-self: flex-start;
}
</style>
