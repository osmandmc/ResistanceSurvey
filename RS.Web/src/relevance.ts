// The pipeline's AI verdict on an article (its Relevance stage): how the screens name it.

export type AiVerdict = 'Relevant' | 'NotRelevant' | 'Uncertain'

/** The verdict and the AI's one-sentence reason; null when the article was not assessed. */
export interface AiRelevance {
  verdict: AiVerdict
  reason: string | null
  /** The prompt and model that judged it: "relevance-v3.1 · gemini-…". */
  version: string | null
}

/** The prompt part of a version ("relevance-v3.1 · gemini-…" → "relevance-v3.1"). */
export const promptOf = (version: string | null | undefined) => version?.split(' · ')[0] ?? null

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
