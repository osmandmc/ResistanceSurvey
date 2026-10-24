import type { AiUse } from './aiFillApi'
import { api } from './api'
import type { AiRelevance } from './relevance'

export interface InboxItem {
  articleId: string
  title: string | null
  sourceId: string
  sourceName: string
  publishedAt: string | null
  url: string
  /** The AI's verdict (pipeline Relevance stage); null when not assessed. */
  ai: AiRelevance | null
  /** The AI draft's one-line summary ("company · protest type · city · date"); null without a draft. */
  draft: string | null
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

/**
 * Which part of the inbox: with an AI draft (R11), what the AI judged not relevant (R21), or the rest to review.
 */
export type InboxPart = 'draft' | 'other' | 'notRelevant'

/** month: "yyyy-MM"; source: pipeline source id, or null for all. */
export function getInbox(month: string, source: string | null, next: string | null, part: InboxPart | null = null) {
  const params = new URLSearchParams({ month })
  if (source) params.set('source', source)
  if (part) params.set('relevance', part)
  if (next) params.set('next', next)
  return api<InboxPage>(`/api/inbox?${params}`)
}

/** Why an article is dismissed; matches the server's DismissKind. */
export const dismissKinds = [
  { value: 1, label: 'Emek hareketiyle ilgisiz', hint: 'Bir işçi ya da emekçi eylemi anlatmıyor (görüş yazısı, kaza haberi, parti açıklaması …).' },
  { value: 2, label: 'Zaten kayıtlı / tekrar haber', hint: 'Bu eylem ya da gelişme başka bir haberle kaydedildi.' },
  { value: 3, label: 'Kapsam dışı', hint: 'Emek haberi, ama ekibin kaydetmediği türden.' },
  { value: 4, label: 'Diğer', hint: 'Nedenini aşağıya yazın.' },
] as const

export type DismissKind = (typeof dismissKinds)[number]['value']

export function dismissArticle(articleId: string, reason: string, kind: DismissKind, ai?: AiUse | null) {
  return api<DismissResponse>(`/api/inbox/${encodeURIComponent(articleId)}/dismiss`, {
    method: 'POST',
    body: JSON.stringify({ reason, kind, ai: ai ?? null }),
  })
}
