// The "Eylem" (protest) part of the case form while it is being filled in, and its rules (as in the old form).

/** Protest types that are strikes: the duration field appears for them. */
export const strikeProtestTypeIds = [5, 6]

/** The protest type "simple workplace action": its description field appears for it. */
export const simpleActionProtestTypeId = 35

/** The intervention type "no intervention": with it there is no custody count. Same as the server's rule. */
export const noInterventionTypeId = 7

export interface LocationValue {
  cityId: number | null
  districtId: number | null
  place: string | null
}

export interface ProtestFormValue {
  protestTypeIds: number[]
  durationDays: number | null
  durationHours: number | null
  durationMinutes: number | null
  simpleDescription: string | null
  /** Date pickers work with timestamps (local midnight). */
  startDate: number | null
  endDate: number | null
  placeIds: number[]
  genderId: number | null
  locations: LocationValue[]
  employeeCount: number | null
  employeeCountRangeId: number | null
  interventionTypeIds: number[]
  custodyCount: number | null
  note: string | null
}

export function emptyProtest(): ProtestFormValue {
  return {
    protestTypeIds: [],
    durationDays: null,
    durationHours: null,
    durationMinutes: null,
    simpleDescription: null,
    startDate: null,
    endDate: null,
    placeIds: [],
    genderId: null,
    locations: [],
    employeeCount: null,
    employeeCountRangeId: null,
    interventionTypeIds: [],
    custodyCount: null,
    note: null,
  }
}

/** "2026-10-12": the picked calendar day, without time zone shifts. */
function toDay(timestamp: number | null) {
  if (timestamp === null) return null
  const d = new Date(timestamp)
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
}

/** The protest as the server takes it (ProtestInput). The duration is saved in seconds, as before. */
export function toProtestInput(value: ProtestFormValue) {
  const hasDuration = [value.durationDays, value.durationHours, value.durationMinutes].some((v) => v !== null)
  return {
    protestTypeIds: value.protestTypeIds,
    strikeDurationSeconds: hasDuration
      ? (value.durationDays ?? 0) * 86400 + (value.durationHours ?? 0) * 3600 + (value.durationMinutes ?? 0) * 60
      : null,
    simpleDescription: value.simpleDescription,
    startDate: toDay(value.startDate),
    endDate: toDay(value.endDate),
    placeIds: value.placeIds,
    genderId: value.genderId,
    locations: value.locations,
    employeeCount: value.employeeCount,
    employeeCountRangeId: value.employeeCountRangeId,
    interventionTypeIds: value.interventionTypeIds,
    custodyCount: value.custodyCount,
    note: value.note,
  }
}

/** "2026-09-10" or "2026-09-10T00:00:00" → that calendar day at local midnight (what the date picker shows). */
function fromDay(value: string | null) {
  const match = value ? /^(\d{4})-(\d{2})-(\d{2})/.exec(value) : null
  return match ? new Date(Number(match[1]), Number(match[2]) - 1, Number(match[3])).getTime() : null
}

/** A saved protest back into the form (editing). The duration comes in seconds. */
export function fromProtestInput(input: {
  protestTypeIds: number[]
  strikeDurationSeconds: number | null
  simpleDescription: string | null
  startDate: string | null
  endDate: string | null
  placeIds: number[]
  genderId: number | null
  locations: LocationValue[]
  employeeCount: number | null
  employeeCountRangeId: number | null
  interventionTypeIds: number[]
  custodyCount: number | null
  note: string | null
}): ProtestFormValue {
  const seconds = input.strikeDurationSeconds ?? 0
  return {
    protestTypeIds: [...input.protestTypeIds],
    durationDays: seconds ? Math.floor(seconds / 86400) : null,
    durationHours: seconds ? Math.floor((seconds % 86400) / 3600) : null,
    durationMinutes: seconds ? Math.floor((seconds % 3600) / 60) : null,
    simpleDescription: input.simpleDescription,
    startDate: fromDay(input.startDate),
    endDate: fromDay(input.endDate),
    placeIds: [...input.placeIds],
    genderId: input.genderId,
    locations: input.locations.map((l) => ({ ...l })),
    employeeCount: input.employeeCount,
    employeeCountRangeId: input.employeeCountRangeId,
    interventionTypeIds: [...input.interventionTypeIds],
    custodyCount: input.custodyCount,
    note: input.note,
  }
}
