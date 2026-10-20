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

const dateTimeFormat = new Intl.DateTimeFormat('tr-TR', {
  day: 'numeric',
  month: 'short',
  year: 'numeric',
  hour: '2-digit',
  minute: '2-digit',
  timeZone: 'Europe/Istanbul',
})

/** "3 Kas 2026 10:42"; "—" when there is no date. */
export function formatDateTime(value: string | null | undefined) {
  return value ? dateTimeFormat.format(new Date(value)) : '—'
}
