import { api } from './api'

export interface InboxItem {
  articleId: string
  title: string | null
  sourceId: string
  sourceName: string
  publishedAt: string | null
  url: string
}

export interface InboxPage {
  items: InboxItem[]
  /** Pass back to load more; null when there is nothing more. */
  next: string | null
}

export interface DismissResponse {
  /** False when the app saved it but the pipeline could not be told yet (it is re-sent later). */
  outcomeSent: boolean
}

/** month: "yyyy-MM"; source: pipeline source id, or null for all. */
export function getInbox(month: string, source: string | null, next: string | null) {
  const params = new URLSearchParams({ month })
  if (source) params.set('source', source)
  if (next) params.set('next', next)
  return api<InboxPage>(`/api/inbox?${params}`)
}

export function dismissArticle(articleId: string, reason: string) {
  return api<DismissResponse>(`/api/inbox/${encodeURIComponent(articleId)}/dismiss`, {
    method: 'POST',
    body: JSON.stringify({ reason }),
  })
}
