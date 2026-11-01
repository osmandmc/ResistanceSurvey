import { api } from './api'

/** The lists in Tanımlar; the slug is also the address (/definitions/corporations). */
export type DefinitionKind = 'companies' | 'corporations' | 'reasons' | 'protest-types' | 'protest-places' | 'workplaces'

export const definitionKinds: { kind: DefinitionKind; label: string; usageUnit: 'vaka' | 'eylem'; hasApproval: boolean }[] = [
  { kind: 'companies', label: 'Şirketler', usageUnit: 'vaka', hasApproval: false },
  { kind: 'corporations', label: 'Kurumlar', usageUnit: 'vaka', hasApproval: true },
  { kind: 'reasons', label: 'Vaka nedenleri', usageUnit: 'vaka', hasApproval: true },
  { kind: 'protest-types', label: 'Eylem türleri', usageUnit: 'eylem', hasApproval: true },
  { kind: 'protest-places', label: 'Eylem mekanları', usageUnit: 'eylem', hasApproval: true },
  { kind: 'workplaces', label: 'İşyerleri', usageUnit: 'vaka', hasApproval: true },
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
  /** Companies (R37): “Kurum türü” and the civil servants' hizmet kolu. */
  institutionKind?: InstitutionKind | null
  serviceBranchId?: number | null
}

/** “Kurum türü” (R37), matching the server's InstitutionKind. */
export const InstitutionKind = {
  Company: 1, Holding: 2, PublicBody: 3, StateEnterprise: 4, Municipality: 5, MunicipalCompany: 6, Cooperative: 7, Other: 8,
} as const
export type InstitutionKind = (typeof InstitutionKind)[keyof typeof InstitutionKind]
export const institutionKindOptions: { value: InstitutionKind; label: string }[] = [
  { value: 1, label: 'Şirket' },
  { value: 2, label: 'Holding / şirketler grubu' },
  { value: 3, label: 'Kamu kurumu' },
  { value: 4, label: 'Kamu şirketi (KİT)' },
  { value: 5, label: 'Belediye' },
  { value: 6, label: 'Belediye şirketi' },
  { value: 7, label: 'Kooperatif / vakıf / dernek' },
  { value: 8, label: 'Diğer' },
]

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
  /** The workers' işkolları. */
  companyWorklines: Option[]
  confederations: Option[]
  /** The civil servants' hizmet kolları (R37). */
  serviceBranches: Option[] | null
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
      institutionKind: item.institutionKind ?? null,
      serviceBranchId: item.serviceBranchId ?? null,
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
  /** The company is the organization above the case's employer. */
  asMainCompany: boolean
}

/** A fact between companies (R37, part B), matching the server's CompanyRelationKind. */
export const CompanyRelationKind = { Group: 1, PublicBody: 2, Renamed: 3 } as const
export type CompanyRelationKind = (typeof CompanyRelationKind)[keyof typeof CompanyRelationKind]

/** Seen from the page's company. Outgoing: “this → other”. */
export interface CompanyRelationItem {
  id: number
  kind: CompanyRelationKind
  outgoing: boolean
  otherId: number
  otherName: string
  sharePercent: number | null
  startDate: string | null
  endDate: string | null
  sourceUrl: string | null
  note: string | null
}

/** How a relation reads from the page's company, by kind and direction (shown to users). */
export const relationChoices: { kind: CompanyRelationKind; outgoing: boolean; label: string; listTitle: string }[] = [
  { kind: 1, outgoing: true, label: 'Bu şirketin grubu (holding)', listTitle: 'Grubu' },
  { kind: 1, outgoing: false, label: 'Bu grubun bir şirketi', listTitle: 'Grup şirketleri' },
  { kind: 2, outgoing: true, label: 'Bu şirketin bağlı olduğu kamu kurumu', listTitle: 'Bağlı olduğu kamu kurumu' },
  { kind: 2, outgoing: false, label: 'Bu kuruma bağlı bir kuruluş', listTitle: 'Bağlı kuruluşları' },
  { kind: 3, outgoing: true, label: 'Bu şirketin yeni adı', listTitle: 'Yeni adı' },
  { kind: 3, outgoing: false, label: 'Bu şirketin eski adı', listTitle: 'Eski adı' },
]

/** A link read from the cases (R37, decision b). AsEmployer: this company is the employer, the other is above it. */
export interface CompanyJobLink {
  role: number
  asEmployer: boolean
  otherId: number
  otherName: string
  cases: number
  first: string | null
  last: string | null
}

export interface CompanyDetail {
  id: number
  name: string
  /** Sahiplik. */
  typeId: number | null
  scaleId: number | null
  worklineId: number | null
  institutionKind: InstitutionKind | null
  serviceBranchId: number | null
  cases: CompanyCase[]
  relations: CompanyRelationItem[]
  jobLinks: CompanyJobLink[]
}

export function getCompany(id: number) {
  return api<CompanyDetail>(`/api/definitions/companies/${id}`)
}

export interface CompanyRelationInput {
  kind: CompanyRelationKind
  otherCompanyId: number
  /** “This company → the other”. */
  thisIsFirst: boolean
  sharePercent: number | null
  startDate: string | null
  endDate: string | null
  sourceUrl: string | null
  note: string | null
}

export function addCompanyRelation(companyId: number, relation: CompanyRelationInput) {
  return api<{ id: number }>(`/api/definitions/companies/${companyId}/relations`, { method: 'POST', body: JSON.stringify(relation) })
}

export function changeCompanyRelation(companyId: number, relationId: number, relation: CompanyRelationInput) {
  return api<void>(`/api/definitions/companies/${companyId}/relations/${relationId}`, { method: 'PUT', body: JSON.stringify(relation) })
}

export function removeCompanyRelation(companyId: number, relationId: number) {
  return api<void>(`/api/definitions/companies/${companyId}/relations/${relationId}`, { method: 'DELETE' })
}
