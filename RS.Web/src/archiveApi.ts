import { api } from './api'

/** Where an article stands; the app's decision comes first. */
export type ArchiveState = 'Processing' | 'Failed' | 'Open' | 'Linked' | 'Dismissed'

export type ArchiveFilter = 'all' | 'open' | 'handled' | 'failed'

export interface ArchiveEvent {
  /** A pipeline stage ("Submitted", "Fetched", "Parsed" …) or "Outcome". */
  step: string
  /** Stages: "Succeeded", "Failed", "Skipped"; outcome: "Digested", "Dismissed". */
  status: string
  at: string
  reason: string | null
}

export interface ArchiveItem {
  articleId: string
  title: string | null
  url: string
  sourceId: string
  sourceName: string
  publishedAt: string | null
  state: ArchiveState
  /** Why it failed or why it was dismissed. */
  reason: string | null
  cases: { id: number; name: string }[]
  timeline: ArchiveEvent[]
}

export interface ArchivePage {
  items: ArchiveItem[]
  next: string | null
}

export interface ArchiveQuery {
  /** "yyyy-MM", or null for all months. */
  month: string | null
  source: string | null
  /** Title text or a link. */
  search: string | null
  filter: ArchiveFilter
}

export const minSearchLength = 2

export function getArchive(query: ArchiveQuery, next: string | null) {
  const params = new URLSearchParams({ filter: query.filter })
  if (query.month) params.set('month', query.month)
  if (query.source) params.set('source', query.source)
  if (query.search) params.set('q', query.search)
  if (next) params.set('next', next)
  return api<ArchivePage>(`/api/archive?${params}`)
}
