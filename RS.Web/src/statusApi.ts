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

/** What one feed brought in. error: why it could not be read (then the counts are 0). */
export interface CollectedFeed {
  sourceId: string
  sourceName: string
  found: number
  accepted: number
  duplicate: number
  rejected: number
  error: string | null
}

/** "Şimdi topla": the pipeline reads the feeds now instead of waiting for its two-hourly timer. */
export function collectFeeds() {
  return api<CollectedFeed[]>('/api/status/collect', { method: 'POST' })
}
