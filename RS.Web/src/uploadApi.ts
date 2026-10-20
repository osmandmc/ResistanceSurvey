import { api } from './api'

/** What the pipeline answered for the file's rows. */
export interface UploadResult {
  rows: number
  accepted: number
  duplicate: number
  rejected: string[] | null
}

/** Matches the server's upload sources. */
export const uploadSources = [
  { label: 'Evrensel', value: 1 },
  { label: 'Kızılbayrak', value: 2 },
]

export function uploadNews(file: File, source: number) {
  const form = new FormData()
  form.append('file', file)
  form.append('source', String(source))
  return api<UploadResult>('/api/upload', { method: 'POST', body: form })
}

// Links pasted by a researcher.

export interface LinkResult {
  url: string
  /** "Accepted", "Duplicate", "Rejected" (from the pipeline) or "Invalid" (not a link). */
  status: 'Accepted' | 'Duplicate' | 'Rejected' | 'Invalid'
  reason: string | null
}

export const maxLinks = 200

/** Same splitting as the server: lines, spaces, commas, semicolons; brackets, quotes and a final full stop removed. */
export function splitLinks(text: string) {
  const pieces = text
    .split(/[\s,;]+/)
    .map((p) => p.trim().replace(/^[(["'<]+/, '').replace(/[.)\]>"']+$/, ''))
    .filter((p) => p !== '')
  return [...new Set(pieces)]
}

export function isLink(piece: string) {
  try {
    const url = new URL(piece)
    return url.protocol === 'http:' || url.protocol === 'https:'
  } catch {
    return false
  }
}

export function submitLinks(text: string) {
  return api<{ links: LinkResult[] }>('/api/upload/links', { method: 'POST', body: JSON.stringify({ text }) })
}
