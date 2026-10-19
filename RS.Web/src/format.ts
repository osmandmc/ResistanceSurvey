// Formats shown to users (Turkish, Turkey time).

const dateFormat = new Intl.DateTimeFormat('tr-TR', {
  day: 'numeric',
  month: 'short',
  year: 'numeric',
  timeZone: 'Europe/Istanbul',
})

/** "3 Kas 2026"; "—" when there is no date. */
export function formatDate(value: string | null | undefined) {
  return value ? dateFormat.format(new Date(value)) : '—'
}
