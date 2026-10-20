import { api } from './api'

export interface StatusDay {
  /** "yyyy-MM-dd", Turkey time. */
  day: string
  received: number
  parsed: number
  failed: number
  digested: number
  dismissed: number
}

export interface StatusSource {
  sourceId: string
  sourceName: string
  /** Latest article in the period; null when nothing arrived. */
  lastReceivedAt: string | null
  /** "Collector" (RSS), "Csv" (Excel), "Manual" (pasted link). */
  lastReceivedOrigin: string | null
  received: number
  failed: number
  /** Waiting in the inbox now. */
  open: number
  /** Newest day first. */
  days: StatusDay[]
}

export interface StatusFailure {
  articleId: string
  sourceName: string
  url: string
  title: string | null
  /** Pipeline stage: "Fetched", "Parsed" … */
  stage: string
  at: string
  reason: string | null
}

export interface StatusPage {
  from: string
  to: string
  sources: StatusSource[]
  recentFailures: StatusFailure[]
}

export function getStatus(days: number) {
  return api<StatusPage>(`/api/status?days=${days}`)
}
