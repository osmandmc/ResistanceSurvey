import { api } from './api'

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
