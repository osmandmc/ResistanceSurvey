import { api } from './api'

export interface UploadResult {
  /** True: rows went to the news pipeline; false: imported into the app. */
  toPipeline: boolean
  rows: number
  imported: number
  kept: number
  accepted: number
  duplicate: number
  rejected: string[] | null
}

/** Matches the server's upload sources. */
export const uploadSources = [
  { label: 'Evrensel', value: 1 },
  { label: 'Kızılbayrak', value: 2 },
]

export function uploadTarget(source: number, year: number, month: number) {
  return api<{ toPipeline: boolean }>(`/api/upload/target?${new URLSearchParams({ source: String(source), year: String(year), month: String(month) })}`)
}

export function uploadNews(file: File, source: number, year: number, month: number) {
  const form = new FormData()
  form.append('file', file)
  form.append('source', String(source))
  form.append('year', String(year))
  form.append('month', String(month))
  return api<UploadResult>('/api/upload', { method: 'POST', body: form })
}
