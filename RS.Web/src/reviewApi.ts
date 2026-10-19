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
