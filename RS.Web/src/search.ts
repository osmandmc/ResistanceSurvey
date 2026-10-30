import type { SelectOption } from 'naive-ui'

/**
 * Comparable form of a text: Turkish lower case ("İ" → "i", "I" → "ı"), then without accents and dotless i, so "isci"
 * matches "İşçi" and "disk" matches "DİSK". JavaScript's own toLowerCase turns "İ" into "i" plus a combining dot.
 */
export function fold(text: string) {
  return text
    .toLocaleLowerCase('tr-TR')
    .normalize('NFD')
    .replace(/\p{M}/gu, '')
    .replace(/ı/g, 'i')
}

/** The filter of every searchable dropdown: each typed word must appear in the option's name, in any order. */
export function turkishFilter(pattern: string, option: SelectOption) {
  const label = fold(String(option.label ?? ''))
  return fold(pattern)
    .split(/\s+/)
    .filter(Boolean)
    .every((word) => label.includes(word))
}
