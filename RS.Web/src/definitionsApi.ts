import { api } from './api'

/** The lists in Tanımlar; the slug is also the address (/definitions/corporations). */
export type DefinitionKind = 'companies' | 'corporations' | 'reasons' | 'protest-types' | 'protest-places'

export const definitionKinds: { kind: DefinitionKind; label: string; usageUnit: 'vaka' | 'eylem'; hasApproval: boolean }[] = [
  { kind: 'companies', label: 'Şirketler', usageUnit: 'vaka', hasApproval: false },
  { kind: 'corporations', label: 'Kurumlar', usageUnit: 'vaka', hasApproval: true },
  { kind: 'reasons', label: 'Vaka nedenleri', usageUnit: 'vaka', hasApproval: true },
  { kind: 'protest-types', label: 'Eylem türleri', usageUnit: 'eylem', hasApproval: true },
  { kind: 'protest-places', label: 'Eylem mekanları', usageUnit: 'eylem', hasApproval: true },
]

export interface DefinitionItem {
  id: number
  name: string
  /** null for companies (no approval). */
  approved: boolean | null
  /** Live cases (protests for protest types and places) that use it. */
  usage: number
  /** Corporation type, or company type. */
  typeId: number | null
  againstProduction: boolean | null
  simple: boolean | null
  scaleId: number | null
  worklineId: number | null
  /** A union's confederation (R34). */
  confederationId: number | null
}

export interface DefinitionPage {
  items: DefinitionItem[]
  total: number
  page: number
  pageSize: number
  /** Values waiting for approval (whatever the filter). */
  pendingCount: number
}

export interface Option {
  id: number
  name: string
}

export interface DefinitionOptions {
  corporationTypes: Option[]
  companyTypes: Option[]
  companyScales: Option[]
  companyWorklines: Option[]
  confederations: Option[]
}

export function getDefinitionOptions() {
  return api<DefinitionOptions>('/api/definitions/options')
}

/** unused: companies no live case uses (Şirketler only). */
export function listDefinitions(
  kind: DefinitionKind,
  filter: { text?: string; pending?: boolean; typeId?: number | null; unused?: boolean; page?: number },
) {
  const params = new URLSearchParams()
  if (filter.text) params.set('q', filter.text)
  if (filter.pending) params.set('pending', 'true')
  if (filter.typeId) params.set('typeId', String(filter.typeId))
  if (filter.unused) params.set('unused', 'true')
  if (filter.page && filter.page > 1) params.set('page', String(filter.page))
  return api<DefinitionPage>(`/api/definitions/${kind}?${params}`)
}

export function updateDefinition(kind: DefinitionKind, item: DefinitionItem) {
  return api<void>(`/api/definitions/${kind}/${item.id}`, {
    method: 'PUT',
    body: JSON.stringify({
      name: item.name,
      typeId: item.typeId,
      againstProduction: item.againstProduction ?? false,
      simple: item.simple ?? false,
      scaleId: item.scaleId,
      worklineId: item.worklineId,
      confederationId: item.confederationId,
    }),
  })
}

export function approveDefinition(kind: DefinitionKind, id: number) {
  return api<void>(`/api/definitions/${kind}/${id}/approve`, { method: 'POST' })
}

export function deleteDefinition(kind: DefinitionKind, id: number) {
  return api<void>(`/api/definitions/${kind}/${id}`, { method: 'DELETE' })
}

export function mergePreview(kind: DefinitionKind, id: number) {
  return api<{ affected: number }>(`/api/definitions/${kind}/${id}/merge-preview`)
}

export function mergeDefinitions(kind: DefinitionKind, fromId: number, intoId: number) {
  return api<{ affected: number }>(`/api/definitions/${kind}/${fromId}/merge`, { method: 'POST', body: JSON.stringify({ intoId }) })
}

/** Values whose names come down to the same key; the first item is the suggested one to keep. */
export interface DuplicateGroup {
  key: string
  items: DefinitionItem[]
}

export function listDuplicates(kind: DefinitionKind) {
  return api<DuplicateGroup[]>(`/api/definitions/${kind}/duplicates`)
}

/** All or nothing: every value in fromIds is merged into intoId. */
export function mergeGroup(kind: DefinitionKind, intoId: number, fromIds: number[]) {
  return api<{ affected: number }>(`/api/definitions/${kind}/duplicates/merge`, { method: 'POST', body: JSON.stringify({ intoId, fromIds }) })
}

/** "Aynı değil": not suggested together again. */
export function dismissDuplicates(kind: DefinitionKind, ids: number[]) {
  return api<void>(`/api/definitions/${kind}/duplicates/dismiss`, { method: 'POST', body: JSON.stringify({ ids }) })
}

export interface CompanyCase {
  id: number
  name: string
  startDate: string
  /** The company is the case's main company (the case is against its subcontractor). */
  asMainCompany: boolean
}

export interface CompanyDetail {
  id: number
  name: string
  typeId: number | null
  scaleId: number | null
  worklineId: number | null
  cases: CompanyCase[]
  /** It works for these as a subcontractor. */
  mainCompanies: Option[]
  /** These work for it. */
  subcontractors: Option[]
}

export function getCompany(id: number) {
  return api<CompanyDetail>(`/api/definitions/companies/${id}`)
}

/** relation: "subcontractors" (the other works for this one) or "main-companies" (this one works for the other). */
export function addCompanyLink(id: number, relation: 'subcontractors' | 'main-companies', companyId: number) {
  return api<void>(`/api/definitions/companies/${id}/${relation}`, { method: 'POST', body: JSON.stringify({ companyId }) })
}

export function removeCompanyLink(id: number, relation: 'subcontractors' | 'main-companies', companyId: number) {
  return api<void>(`/api/definitions/companies/${id}/${relation}/${companyId}`, { method: 'DELETE' })
}
