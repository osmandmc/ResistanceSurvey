import { api } from './api'
import type { CaseFormValue } from './caseForm'
import { fromDay, noInterventionTypeId, strikeProtestTypeIds, type ProtestFormValue } from './protestForm'

// "AI ile doldur" (R19): the prompt to copy, and the pasted answer checked by the server against the lists.

/** An answer the researcher checked, sent with the decision so it is kept beside it (R19c). */
export interface AiUse {
  answer: string
  promptVersion: string | null
  /** "Forma uygula" was used. */
  applied: boolean
}

export interface AiPrompt {
  text: string
  version: string
}

/** Matches the server's AiItemStatus. */
export type AiItemStatus = 'Matched' | 'Corrected' | 'New' | 'NotInList' | 'Skipped'

export interface AiItem {
  /** What the AI wrote. */
  answer: string
  /** What goes into the form (the list's own spelling), or null. */
  value: string | null
  status: AiItemStatus
  note: string | null
}

export interface AiField {
  /** e.g. "case.reasons", "protest.locations". */
  field: string
  label: string
  items: AiItem[]
}

export interface AiCaseValues {
  categoryId: number | null
  targetType: number | null
  companyId: number | null
  /** The AI named a company that is not registered: offered to the researcher, never created on its own. */
  newCompanyName: string | null
  isOutsource: boolean
  mainCompanyId: number | null
  newMainCompanyName: string | null
  targetCorporationId: number | null
  reasonIds: number[]
  corporationIds: number[]
  newCorporationNames: string[]
  employmentTypeIds: number[]
  employeeCount: number | null
  firedEmployeeCount: number | null
  description: string | null
}

export interface AiProtestValues {
  protestTypeIds: number[]
  /** "yyyy-MM-dd" */
  startDate: string | null
  endDate: string | null
  placeIds: number[]
  locations: { cityId: number; districtId: number | null; place: string | null }[]
  employeeCount: number | null
  employeeCountRangeId: number | null
  interventionTypeIds: number[]
  custodyCount: number | null
  strikeHours: number | null
}

export interface AiCheckResult {
  error: string | null
  relevant: boolean
  relevanceReason: string | null
  fields: AiField[]
  case: AiCaseValues | null
  protest: AiProtestValues | null
}

export function getAiPrompt(articleId: string) {
  return api<AiPrompt>(`/api/review/${encodeURIComponent(articleId)}/ai-prompt`)
}

export function checkAiAnswer(answer: string) {
  return api<AiCheckResult>('/api/review/ai-check', { method: 'POST', body: JSON.stringify({ answer }) })
}

/** The case fields the answer fills; fields it leaves empty are not touched. */
export function aiCaseValues(v: AiCaseValues): Partial<CaseFormValue> {
  const out: Partial<CaseFormValue> = {}
  if (v.categoryId !== null) out.categoryId = v.categoryId
  if (v.targetType !== null) out.targetType = v.targetType
  if (v.companyId !== null) out.companyId = v.companyId
  // A named main company opens the main company field, also when it still has to be added.
  if (v.isOutsource) out.isOutsource = true
  if (v.mainCompanyId !== null) out.mainCompanyId = v.mainCompanyId
  if (v.targetCorporationId !== null) out.targetCorporationId = v.targetCorporationId
  if (v.reasonIds.length) out.reasons = [...v.reasonIds]
  if (v.corporationIds.length || v.newCorporationNames.length) out.corporations = [...v.corporationIds, ...v.newCorporationNames]
  if (v.employmentTypeIds.length) out.employmentTypeIds = [...v.employmentTypeIds]
  if (v.employeeCount !== null) out.employeeCount = v.employeeCount
  if (v.firedEmployeeCount !== null) out.firedEmployeeCount = v.firedEmployeeCount
  if (v.description) out.description = v.description
  return out
}

/**
 * The protest fields the answer fills. The duration only for a strike and the custody count only with an
 * intervention, as the form shows them (hidden fields would otherwise be saved unseen).
 */
export function aiProtestValues(v: AiProtestValues): Partial<ProtestFormValue> {
  const out: Partial<ProtestFormValue> = {}
  if (v.protestTypeIds.length) out.protestTypeIds = [...v.protestTypeIds]
  if (v.startDate) out.startDate = fromDay(v.startDate)
  if (v.endDate) out.endDate = fromDay(v.endDate)
  if (v.placeIds.length) out.placeIds = [...v.placeIds]
  if (v.locations.length) out.locations = v.locations.map((l) => ({ cityId: l.cityId, districtId: l.districtId, place: l.place }))
  if (v.employeeCount !== null) out.employeeCount = v.employeeCount
  if (v.employeeCountRangeId !== null) out.employeeCountRangeId = v.employeeCountRangeId
  if (v.interventionTypeIds.length) out.interventionTypeIds = [...v.interventionTypeIds]
  const intervened = v.interventionTypeIds.some((id) => id !== noInterventionTypeId)
  if (v.custodyCount !== null && intervened) out.custodyCount = v.custodyCount
  if (v.strikeHours && v.protestTypeIds.some((id) => strikeProtestTypeIds.includes(id))) {
    out.durationDays = Math.floor(v.strikeHours / 24) || null
    out.durationHours = v.strikeHours % 24 || null
  }
  return out
}

/** Field names whose value is still what the answer put there (the AI mark goes once the researcher changes it). */
export function unchangedFields<T extends object>(current: T, applied: Partial<T> | null): string[] {
  if (!applied) return []
  return (Object.keys(applied) as (keyof T)[])
    .filter((key) => JSON.stringify(current[key]) === JSON.stringify(applied[key]))
    .map(String)
}
