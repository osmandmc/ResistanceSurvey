import type { AiUse } from './aiFillApi'
import { api } from './api'
import type { AiRelevance } from './relevance'

export interface ReviewArticle {
  articleId: string
  title: string | null
  url: string
  sourceName: string
  publishedAt: string | null
  /** Full text from the pipeline; null until it has been read. */
  body: string | null
  /** The AI's verdict (pipeline Relevance stage); null when not assessed. */
  ai: AiRelevance | null
}

export interface CaseOption {
  id: number
  name: string | null
  companyName: string | null
  lastProtestDate: string | null
  protestCount: number
  /** The latest protest has no end date. */
  ongoing: boolean
  /** Where its protests took place. */
  cities: string[]
}

export interface CaseSearch {
  query?: string
  companyId?: number | null
  cityId?: number | null
  /** "yyyy-MM-dd": only cases whose latest protest is on or after this day. */
  since?: string | null
}

/** Matches the server's ReviewAction. */
export type ReviewAction = 'newCase' | 'addProtest' | 'linkOnly'

export interface SaveReviewRequest {
  action: ReviewAction
  resistanceId?: number | null
  case?: unknown
  protest?: unknown
  /** The AI answer checked for this article, if any (R19c). */
  ai?: AiUse | null
}

export interface SaveReviewResponse {
  resistanceId: number
  /** False when saved but the pipeline could not be told yet (the inbox re-sends it). */
  outcomeSent: boolean
}

export function getReviewArticle(articleId: string) {
  return api<ReviewArticle>(`/api/review/${encodeURIComponent(articleId)}`)
}

export function searchCases(search: CaseSearch) {
  const params = new URLSearchParams()
  if (search.query) params.set('query', search.query)
  if (search.companyId) params.set('companyId', String(search.companyId))
  if (search.cityId) params.set('cityId', String(search.cityId))
  if (search.since) params.set('since', search.since)
  return api<CaseOption[]>(`/api/review/resistances?${params}`)
}

export function saveReview(articleId: string, request: SaveReviewRequest) {
  return api<SaveReviewResponse>(`/api/review/${encodeURIComponent(articleId)}`, {
    method: 'POST',
    body: JSON.stringify(request),
  })
}

// The case form.

export interface Option {
  id: number
  name: string
}

export interface CorporationOption extends Option {
  isTradeUnion: boolean
}

export interface DistrictOption extends Option {
  cityId: number
}

export interface CaseFormOptions {
  categories: Option[]
  reasons: Option[]
  employeeCountRanges: Option[]
  corporations: CorporationOption[]
  tradeUnions: Option[]
  tradeUnionAuthorities: Option[]
  employmentTypes: Option[]
  companyTypes: Option[]
  companyScales: Option[]
  companyWorklines: Option[]
  protestTypes: Option[]
  protestPlaces: Option[]
  genders: Option[]
  protestEmployeeCountRanges: Option[]
  cities: Option[]
  districts: DistrictOption[]
  interventionTypes: Option[]
  /** Organization types, for "Kurum Ekle" (R34). */
  corporationTypes: Option[]
  confederations: Option[]
}

/** Matches the server's KnownListIds: the types whose organizations are unions or confederations (R34). */
export const UnionTypes = { TradeUnion: 1, Confederation: 6 } as const

export interface CreateCorporationResult {
  corporation: CorporationOption
  /** An organization with that name already existed and is returned instead of a duplicate. */
  existed: boolean
}

/** "Kurum Ekle": from a case (waits for approval) or from Tanımlar (approved). */
export function createCorporation(corporation: { name: string; typeId: number | null; confederationId: number | null }, fromDefinitions = false) {
  return api<CreateCorporationResult>(fromDefinitions ? '/api/definitions/corporations' : '/api/review/corporations', {
    method: 'POST',
    body: JSON.stringify(corporation),
  })
}

/** The three case categories (R31, the server's KnownListIds): each shows its own fields. */
export const CaseCategory = { Workplace: 1, Solidarity: 2, General: 3 } as const

/** Matches the server's ResistanceResult; labels are shown to users. */
export const resultOptions = [
  { value: 0, label: 'Bilinmiyor' },
  { value: 1, label: 'Tam Kazanım' },
  { value: 2, label: 'Yarım Kazanım' },
  { value: 3, label: 'Sıfır Kazanım' },
]

/** The "Vaka (Direniş)" part as the server takes it (CaseInput). */
export interface CaseInput {
  name: string | null
  description: string | null
  categoryId: number | null
  /** The workplace of those who acted: İşletme required, Dayanışma optional, Genel none. */
  companyId: number | null
  isOutsource: boolean
  mainCompanyId: number | null
  /** Dayanışma: the case whose struggle the action supported; may stay empty. */
  supportedCaseId: number | null
  reasonIds: number[]
  newReasonNames: string[]
  developRight: boolean | null
  employeeCount: number | null
  employeeCountRangeId: number | null
  corporationIds: number[]
  newCorporationNames: string[]
  tradeUnionAuthorityId: number | null
  tradeUnionId: number | null
  employmentTypeIds: number[]
  firedEmployeeCount: number | null
  result: number | null
}

export interface CreateCompanyResult {
  company: Option
  /** A company with that name already existed and is returned instead of a duplicate. */
  existed: boolean
}

export function getFormOptions() {
  return api<CaseFormOptions>('/api/review/form-options')
}

export function searchCompanies(query: string) {
  return api<Option[]>(`/api/review/companies?${new URLSearchParams({ query })}`)
}

export function createCompany(company: { name: string; typeId: number | null; scaleId: number | null; worklineId: number | null }) {
  return api<CreateCompanyResult>('/api/review/companies', { method: 'POST', body: JSON.stringify(company) })
}

/** "Olası vakalar": a case of a company named in the article. */
export interface CaseSuggestion {
  case: CaseOption
  matchedCompanyId: number
  matchedCompany: string
  /** The company is named in the title, not only in the text. */
  inTitle: boolean
}

export function getSuggestions(articleId: string) {
  return api<CaseSuggestion[]>(`/api/review/${encodeURIComponent(articleId)}/suggestions`)
}
