import type { CaseInput } from './reviewApi'

/**
 * The case part of the form while it is being filled in. Reasons and corporations may hold ids (picked from the
 * list) or strings (typed as new values); toCaseInput splits them for the server.
 */
export interface CaseFormValue extends Omit<CaseInput, 'reasonIds' | 'newReasonNames' | 'corporationIds' | 'newCorporationNames'> {
  reasons: (number | string)[]
  corporations: (number | string)[]
}

export function emptyCase(): CaseFormValue {
  return {
    name: null,
    description: null,
    note: null,
    categoryId: null,
    companyId: null,
    isOutsource: false,
    mainCompanyId: null,
    supportedCaseId: null,
    reasons: [],
    developRight: null,
    employeeCount: null,
    employeeCountRangeId: null,
    corporations: [],
    tradeUnionAuthorityId: null,
    tradeUnionId: null,
    employmentTypeIds: [],
    firedEmployeeCount: null,
    result: 0,
  }
}

export function toCaseInput(value: CaseFormValue): CaseInput {
  const { reasons, corporations, ...rest } = value
  const ids = (list: (number | string)[]) => list.filter((v): v is number => typeof v === 'number')
  const names = (list: (number | string)[]) => list.filter((v): v is string => typeof v === 'string')
  return {
    ...rest,
    reasonIds: ids(reasons),
    newReasonNames: names(reasons),
    corporationIds: ids(corporations),
    newCorporationNames: names(corporations),
  }
}

/** A saved case back into the form (editing). */
export function fromCaseInput(input: CaseInput): CaseFormValue {
  const { reasonIds, corporationIds, ...rest } = input
  return { ...emptyCase(), ...rest, reasons: [...reasonIds], corporations: [...corporationIds] }
}
