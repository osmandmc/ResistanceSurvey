import { api } from './api'

export interface StatusDay {
  /** "yyyy-MM-dd", Turkey time. */
  day: string
  received: number
  parsed: number
  failed: number
  digested: number
  dismissed: number
  /** The AI's verdicts given that day, and the drafts it made. */
  assessedRelevant: number
  assessedUncertain: number
  assessedNotRelevant: number
  drafted: number
  /** Where the articles received that day stand now; null from an older pipeline. */
  now: ArticleStates | null
}

/** Articles by where they stand now; the counts add up to the day's received articles. */
export interface ArticleStates {
  linked: number
  dismissed: number
  withDraft: number
  toReview: number
  notRelevant: number
  failed: number
  processing: number
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
  /** Of those: with an AI draft, rejected by the AI, not assessed by the AI yet. */
  openWithDraft: number
  openNotRelevant: number
  openNotAssessed: number
  /** Judged by an older relevance prompt: can be judged again with the current one. */
  openWithOlderVerdict: number
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
  ai: StatusAi
}

/** The AI: whether a key is configured, and what it said about the articles researchers decided in the period. */
export interface StatusAi {
  enabled: boolean
  agreement: { verdict: 'Relevant' | 'NotRelevant' | 'Uncertain'; digested: number; dismissed: number }[]
}

/** "AI ile işle": articles queued for a verdict and for a draft, spacingSeconds apart. */
export interface QueuedForAi {
  sourceId: string
  sourceName: string
  toAssess: number
  toDraft: number
  /** More were waiting than one call queues: click again once these are done. */
  more: boolean
  enabled: boolean
  spacingSeconds: number
}

/** again: instead, the articles judged by an older relevance prompt, judged again with the current one. */
export function queueForAi(sourceId: string, again = false) {
  return api<QueuedForAi>(`/api/status/ai?source=${encodeURIComponent(sourceId)}${again ? '&again=true' : ''}`, { method: 'POST' })
}

/** "AI istemleri": the prompts the pipeline's AI stages run now. */
export interface AiPrompts {
  prompts: { name: 'relevance' | 'extraction'; version: string; model: string; text: string }[]
  /** Open articles judged by an older relevance prompt. */
  openWithOlderVerdict: number
  /** When the drafts' list values were last updated by the app; null: the pipeline's snapshot of 21.10.2026. */
  listsUpdatedAt: string | null
}

/** "Listeleri şimdi gönder": the app's list values to the pipeline now. changed: false when it already had them. */
export function sendLists() {
  return api<{ updatedAt: string; changed: boolean; counts: Record<string, number> }>('/api/status/lists', { method: 'POST' })
}

export function getAiPrompts() {
  return api<AiPrompts>('/api/status/prompts')
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

// "AI raporu" (R19d): how often researchers keep the AI's values, and the relevance verdicts against the decisions.

export interface AiReportItem {
  name: string
  kept: number
  removed: number
  added: number
}

export interface AiReportField {
  field: string
  label: string
  isList: boolean
  kept: number
  changed: number
  removed: number
  added: number
  items: AiReportItem[]
}

export interface AiReportGroup {
  origin: string
  promptVersion: string | null
  count: number
}

export interface AiReportVerdict {
  verdict: 'Relevant' | 'Uncertain' | 'NotRelevant' | 'NotAssessed'
  linked: number
  dismissed: Record<string, number>
}

export interface AiReport {
  from: string
  to: string
  decisions: number
  applied: number
  others: number
  groups: AiReportGroup[]
  fields: AiReportField[]
  relevance: { rows: AiReportVerdict[]; versions: string[]; truncated: boolean; error: string | null }
}

export function getAiReport(filter: { days: number; origin?: string | null; promptVersion?: string | null; relevanceVersion?: string | null }) {
  const query = new URLSearchParams({ days: String(filter.days) })
  if (filter.origin) query.set('origin', filter.origin)
  if (filter.promptVersion) query.set('promptVersion', filter.promptVersion)
  if (filter.relevanceVersion) query.set('relevanceVersion', filter.relevanceVersion)
  return api<AiReport>(`/api/status/ai-report?${query}`)
}
