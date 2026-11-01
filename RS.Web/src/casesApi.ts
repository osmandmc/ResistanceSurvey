import { api } from './api'
import type { CaseInput } from './reviewApi'

/** The case list's filters (all optional); also used for the Excel export. */
export interface CaseFilter {
  text?: string | null
  companyId?: number | null
  mainCompanyId?: number | null
  categoryId?: number | null
  year?: number | null
  month?: number | null
  cityId?: number | null
  hasNote?: boolean | null
  ongoingOnly?: boolean
}

export interface CaseListItem {
  id: number
  name: string | null
  /** The company, or the institution for cases against one. */
  target: string | null
  /** The case's place (R37), when it names one. */
  workplaceName: string | null
  mainCompanyName: string | null
  categoryName: string | null
  protestCount: number
  lastProtestDate: string | null
  ongoing: boolean
  cities: string[]
  articleCount: number
}

export interface CaseListPage {
  items: CaseListItem[]
  total: number
  page: number
  pageSize: number
}

export function filterParams(filter: CaseFilter) {
  const params = new URLSearchParams()
  for (const [key, value] of Object.entries(filter)) {
    if (value !== null && value !== undefined && value !== '' && value !== false) params.set(key, String(value))
  }
  return params
}

export function listCases(filter: CaseFilter, page: number) {
  const params = filterParams(filter)
  params.set('page', String(page))
  return api<CaseListPage>(`/api/cases?${params}`)
}

/** A plain link: the sign-in cookie goes along, so the browser downloads the file itself. */
export function exportUrl(filter: CaseFilter) {
  return `/api/cases/export?${filterParams(filter)}`
}

// One case (detail page).

export interface ProtestInput {
  protestTypeIds: number[]
  strikeDurationSeconds: number | null
  simpleDescription: string | null
  /** "2026-09-10T00:00:00" from the server; "2026-09-10" when sending. */
  startDate: string | null
  endDate: string | null
  placeIds: number[]
  genderId: number | null
  locations: { cityId: number | null; districtId: number | null; place: string | null }[]
  employeeCount: number | null
  employeeCountRangeId: number | null
  interventionTypeIds: number[]
  custodyCount: number | null
  note: string | null
}

export interface ProtestDetail {
  id: number
  protest: ProtestInput
  typeNames: string[]
  cityNames: string[]
}

export interface CaseArticle {
  newsId: number
  title: string | null
  link: string
  date: string
  /** Set for articles that came through the pipeline. */
  articleId: string | null
}

export interface CaseDetail {
  id: number
  case: CaseInput
  companyName: string | null
  mainCompanyName: string | null
  protests: ProtestDetail[]
  articles: CaseArticle[]
  creator: string | null
  createDate: string | null
  updater: string | null
  updateDate: string | null
  /** Dayanışma (R31): the supported case, for the form's picker. */
  supportedCase: { id: number; name: string | null } | null
  /** R37: the case's workplace, when it is not the employer itself. */
  workplaceName: string | null
}

export interface NewsOption {
  id: number
  title: string | null
  link: string
  date: string
  linked: boolean
}

export function getCase(id: number) {
  return api<CaseDetail>(`/api/cases/${id}`)
}

export function updateCase(id: number, input: CaseInput) {
  return api<{ id: number }>(`/api/cases/${id}`, { method: 'PUT', body: JSON.stringify(input) })
}

export function deleteCase(id: number) {
  return api<{ id: number }>(`/api/cases/${id}`, { method: 'DELETE' })
}

/** protestId null adds a protest. */
export function saveProtest(caseId: number, protestId: number | null, input: unknown) {
  return protestId === null
    ? api<{ id: number }>(`/api/cases/${caseId}/protests`, { method: 'POST', body: JSON.stringify(input) })
    : api<{ id: number }>(`/api/cases/${caseId}/protests/${protestId}`, { method: 'PUT', body: JSON.stringify(input) })
}

export function deleteProtest(caseId: number, protestId: number) {
  return api<{ id: number }>(`/api/cases/${caseId}/protests/${protestId}`, { method: 'DELETE' })
}

export function searchNews(query: string, year: number | null, month: number | null) {
  const params = new URLSearchParams()
  if (query) params.set('query', query)
  if (year) params.set('year', String(year))
  if (month) params.set('month', String(month))
  return api<NewsOption[]>(`/api/cases/news?${params}`)
}

export function attachArticle(caseId: number, newsId: number) {
  return api<{ id: number }>(`/api/cases/${caseId}/articles`, { method: 'POST', body: JSON.stringify({ newsId }) })
}

export function detachArticle(caseId: number, newsId: number) {
  return api<{ id: number }>(`/api/cases/${caseId}/articles/${newsId}`, { method: 'DELETE' })
}

/** A new case with its first protest, without an article. */
export function createCase(input: { case: CaseInput; protest: unknown }) {
  return api<{ id: number }>('/api/cases', { method: 'POST', body: JSON.stringify(input) })
}

// R31c: Dayanışma cases and the struggles they supported.

export interface SupportedSuggestion {
  caseId: number
  name: string | null
  companyName: string | null
  lastProtestDate: string | null
  ongoing: boolean
  /** "aynı işyeri" or "adında işyeri geçiyor". */
  reason: string
}

export interface SolidarityGroup {
  name: string
  oldCompanyId: number | null
  caseIds: number[]
  cities: string[]
  firstDate: string | null
  lastDate: string | null
  /** The name is a real workplace with İşletme cases. */
  oldCompanyUsedByIsletme: boolean
  /** An organization of that name exists. */
  looksLikeOrganization: boolean
  suggestion: SupportedSuggestion | null
}

export interface TargetCheck {
  caseId: number
  name: string | null
  companyName: string | null
  oldTargetName: string | null
}

export interface SolidarityLinksPage {
  groups: SolidarityGroup[]
  targetChecks: TargetCheck[]
  /** Dayanışma cases entered after the migration and not linked yet. */
  unlinkedNewCases: number
}

export type SolidarityAction = 'Link' | 'Organizer' | 'Workplace' | 'None'

export const getSolidarityLinks = () => api<SolidarityLinksPage>('/api/cases/solidarity-links')

export const resolveSolidarity = (caseIds: number[], action: SolidarityAction, supportedCaseId: number | null = null) =>
  api<void>('/api/cases/solidarity-links/resolve', { method: 'POST', body: JSON.stringify({ caseIds, action, supportedCaseId }) })

export const markTargetChecked = (caseId: number) => api<void>(`/api/cases/${caseId}/target-checked`, { method: 'POST' })

export interface CaseSolidarity {
  linked: { id: number; name: string | null }[]
  suggested: { id: number; name: string | null }[]
}

export const getCaseSolidarity = (caseId: number) => api<CaseSolidarity>(`/api/cases/${caseId}/solidarity`)

export const linkSupportedCase = (solidarityCaseId: number, supportedCaseId: number) =>
  api<void>(`/api/cases/${solidarityCaseId}/supported-case`, { method: 'POST', body: JSON.stringify({ supportedCaseId }) })

/** A protest as a line in the case list's opened row. */
export interface ProtestLine {
  id: number
  startDate: string
  endDate: string | null
  types: string[]
  /** "City / district · place". */
  places: string[]
  description: string | null
}

export const getProtestLines = (caseId: number) => api<ProtestLine[]>(`/api/cases/${caseId}/protest-lines`)
