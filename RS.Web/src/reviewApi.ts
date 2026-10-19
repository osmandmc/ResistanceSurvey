import { api } from './api'

export interface ReviewArticle {
  articleId: string
  title: string | null
  url: string
  sourceName: string
  publishedAt: string | null
  /** Full text from the pipeline; null until it has been read. */
  body: string | null
}

export interface CaseOption {
  id: number
  name: string | null
  companyName: string | null
  lastProtestDate: string | null
}

/** Matches the server's ReviewAction. */
export type ReviewAction = 'newCase' | 'addProtest' | 'linkOnly'

export interface SaveReviewRequest {
  action: ReviewAction
  resistanceId?: number | null
  case?: unknown
  protest?: unknown
}

export interface SaveReviewResponse {
  resistanceId: number
  /** False when saved but the pipeline could not be told yet (the inbox re-sends it). */
  outcomeSent: boolean
}

export function getReviewArticle(articleId: string) {
  return api<ReviewArticle>(`/api/review/${encodeURIComponent(articleId)}`)
}

export function searchCases(query: string) {
  return api<CaseOption[]>(`/api/review/resistances?${new URLSearchParams({ query })}`)
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
}

/** Matches the server's ResistanceTargetType. */
export const TargetType = { Company: 1, Corporation: 2 } as const

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
  targetType: number | null
  companyId: number | null
  isOutsource: boolean
  mainCompanyId: number | null
  targetCorporationId: number | null
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
