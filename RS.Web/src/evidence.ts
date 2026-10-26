// Evidence highlighting (R27): the sentences of an article the AI took its values from, found in the text even when
// case, spacing or quote marks differ.

export interface AiEvidence {
  /** "case.company", "protest.types" … */
  field: string
  /** As in the check table: "Şirket / kurum", "Eylem türleri" … */
  label: string
  quote: string
}

/** A piece of text to show, highlighted when it carries one or more quotes. */
export interface TextSegment {
  text: string
  /** Fields whose quote this piece is; empty for plain text. */
  fields: string[]
  labels: string[]
}

const quoteMarks = /[‘’‚‛“”„«»"'`´]/

/** The text in comparable form, with the original index of every kept character. */
function normalize(text: string) {
  let out = ''
  const map: number[] = []
  let lastSpace = true
  for (let i = 0; i < text.length; i++) {
    const c = text[i]
    if (quoteMarks.test(c)) continue
    if (/\s/.test(c)) {
      if (!lastSpace) {
        out += ' '
        map.push(i)
        lastSpace = true
      }
      continue
    }
    out += c.toLocaleLowerCase('tr')
    map.push(i)
    lastSpace = false
  }
  return { out, map }
}

/** Where the quote is in the text ([start, end) of the original), or null. Trailing punctuation is ignored. */
export function findQuote(text: string, quote: string): [number, number] | null {
  const needle = normalize(quote).out.replace(/^[\s.…,;:!?-]+|[\s.…,;:!?-]+$/g, '')
  if (needle.length < 3) return null
  const { out, map } = normalize(text)
  const at = out.indexOf(needle)
  if (at < 0) return null
  return [map[at], map[at + needle.length - 1] + 1]
}

/** The text split into plain and highlighted pieces; overlapping quotes share one highlight. */
export function highlight(text: string, evidence: AiEvidence[]): TextSegment[] {
  const ranges = evidence
    .map((e) => ({ e, at: findQuote(text, e.quote) }))
    .filter((r): r is { e: AiEvidence; at: [number, number] } => r.at !== null)
    .sort((a, b) => a.at[0] - b.at[0])
  // Merge overlaps into one range carrying every field.
  const merged: { start: number; end: number; fields: string[]; labels: string[] }[] = []
  for (const { e, at } of ranges) {
    const last = merged[merged.length - 1]
    if (last && at[0] < last.end) {
      last.end = Math.max(last.end, at[1])
      last.fields.push(e.field)
      last.labels.push(e.label)
    } else {
      merged.push({ start: at[0], end: at[1], fields: [e.field], labels: [e.label] })
    }
  }
  const segments: TextSegment[] = []
  let pos = 0
  for (const m of merged) {
    if (m.start > pos) segments.push({ text: text.slice(pos, m.start), fields: [], labels: [] })
    segments.push({ text: text.slice(m.start, m.end), fields: m.fields, labels: m.labels })
    pos = m.end
  }
  if (pos < text.length) segments.push({ text: text.slice(pos), fields: [], labels: [] })
  return segments
}

/** The form field an evidence key belongs to, to drop its highlight once the researcher changes the value. */
export const evidenceFormField: Record<string, { part: 'case' | 'protest'; key: string }> = {
  'case.category': { part: 'case', key: 'categoryId' },
  'case.company': { part: 'case', key: 'companyId' },
  'case.main_company': { part: 'case', key: 'mainCompanyId' },
  'case.organizations': { part: 'case', key: 'corporations' },
  'case.reasons': { part: 'case', key: 'reasons' },
  'case.employment_types': { part: 'case', key: 'employmentTypeIds' },
  'case.employee_count': { part: 'case', key: 'employeeCount' },
  'case.fired_count': { part: 'case', key: 'firedEmployeeCount' },
  'protest.start_date': { part: 'protest', key: 'startDate' },
  'protest.end_date': { part: 'protest', key: 'endDate' },
  'protest.types': { part: 'protest', key: 'protestTypeIds' },
  'protest.places': { part: 'protest', key: 'placeIds' },
  'protest.locations': { part: 'protest', key: 'locations' },
  'protest.participant_count': { part: 'protest', key: 'employeeCount' },
  'protest.custody_count': { part: 'protest', key: 'custodyCount' },
  'protest.interventions': { part: 'protest', key: 'interventionTypeIds' },
}
