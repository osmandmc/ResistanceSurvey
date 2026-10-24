// The pipeline's AI verdict on an article (its Relevance stage): how the screens name it.

export type AiVerdict = 'Relevant' | 'NotRelevant' | 'Uncertain'

/** The verdict and the AI's one-sentence reason; null when the article was not assessed. */
export interface AiRelevance {
  verdict: AiVerdict
  reason: string | null
}

export const verdictText: Record<AiVerdict, string> = {
  Relevant: 'ilgili',
  NotRelevant: 'ilgisiz görünüyor',
  Uncertain: 'belirsiz',
}

export const verdictType: Record<AiVerdict, 'success' | 'warning' | 'default'> = {
  Relevant: 'success',
  NotRelevant: 'default',
  Uncertain: 'warning',
}
