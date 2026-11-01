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
  /** The article's picture on the news site (loaded from there); null when there is none. */
  imageUrl: string | null
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
  /** The civil servants' hizmet kolları (R37). */
  serviceBranches?: Option[] | null
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
/** R37: how the employer works in an İşletme case (matches the server's EmployerRole). */
export const EmployerRole = { Unknown: 0, Direct: 1, Subcontractor: 2, Contractor: 3, Operator: 4 } as const
export type EmployerRole = (typeof EmployerRole)[keyof typeof EmployerRole]

/** The buttons, in order, and the name of the organization above for the roles that have one. */
export const employerRoleOptions: { value: EmployerRole; label: string; help: string; above: string | null }[] = [
  { value: EmployerRole.Direct, label: 'Doğrudan', help: 'İşçiler doğrudan bu işverende çalışıyor.', above: null },
  { value: EmployerRole.Subcontractor, label: 'Taşeron', help: 'İşveren bu işi başka bir kurum için taşeron olarak yapıyor (alt işveren).', above: 'Asıl İşveren' },
  { value: EmployerRole.Contractor, label: 'Yüklenici', help: 'Bütün iş (ör. bir inşaat) işverene verildi; işi veren iş sahibidir.', above: 'İş Sahibi' },
  { value: EmployerRole.Operator, label: 'İşletmeci', help: 'İşveren başkasına ait bir yeri işletiyor (rödovans, işletme hakkı).', above: 'Varlığın Sahibi' },
  { value: EmployerRole.Unknown, label: 'Bilinmiyor', help: 'Haber söylemiyor.', above: null },
]
export const hasAbove = (role: EmployerRole | null) =>
  role === EmployerRole.Subcontractor || role === EmployerRole.Contractor || role === EmployerRole.Operator

/** R37: a company's fact with another one (its group, the public body it belongs to, a name change), seen from it. */
export interface CompanyFact {
  id: number
  /** 1 group, 2 public body, 3 renamed. */
  kind: number
  outgoing: boolean
  otherId: number
  otherName: string
  sharePercent: number | null
  startDate: string | null
  endDate: string | null
}

export function getCompanyFacts(companyId: number) {
  return api<CompanyFact[]>(`/api/review/companies/${companyId}/facts`)
}

/** R37: a workplace (a place) as the form offers it, with whom it belongs to. */
export interface WorkplaceOption {
  id: number
  name: string
  cityName: string | null
  districtName: string | null
  ownerCompanyId: number | null
  ownerName: string | null
  approved: boolean
}

/** By name, or without a query the places known for that company (its cases, or belonging to it). */
export function searchWorkplaces(query: string, companyId: number | null = null) {
  const params = new URLSearchParams()
  if (query) params.set('query', query)
  if (companyId) params.set('companyId', String(companyId))
  return api<WorkplaceOption[]>(`/api/review/workplaces?${params}`)
}

export interface CreateWorkplaceResult {
  workplace: WorkplaceOption
  existed: boolean
}

export function createWorkplace(workplace: { name: string; cityId: number | null; districtId: number | null; ownerCompanyId: number | null }) {
  return api<CreateWorkplaceResult>('/api/review/workplaces', { method: 'POST', body: JSON.stringify(workplace) })
}

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
  /** “Kişisel Notlar”: the case's own note; each protest has its own note too. */
  note: string | null
  categoryId: number | null
  /** İşletme: the employer (“İşveren”), required unless employerUnknown. Dayanışma: the workplace whose workers acted. */
  companyId: number | null
  /** The old “Taşeron mu?”; kept equal to “the role has an organization above” (R37). */
  isOutsource: boolean
  /** İşletme (R37): how the employer works in this case (EmployerRole). */
  employerRole: EmployerRole | null
  /** “İşveren bilinmiyor”. */
  employerUnknown: boolean
  /** The organization above the employer: Asıl işveren, İş sahibi or Varlığın sahibi, by role. */
  mainCompanyId: number | null
  /** İşletme (R37): the place, when it is not the employer itself. */
  workplaceId: number | null
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

/** typeId: Sahiplik; institutionKind: Kurum türü; serviceBranchId: the civil servants' hizmet kolu (R37). */
export function createCompany(company: {
  name: string
  typeId: number | null
  scaleId: number | null
  worklineId: number | null
  institutionKind?: number | null
  serviceBranchId?: number | null
}) {
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
