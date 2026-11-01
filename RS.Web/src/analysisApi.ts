import { api } from '@/api'
import { filterParams, type CaseFilter } from '@/casesApi'

// Analiz (R38): the numbers behind the status charts, for the case list's filters.

export interface AnalysisSeries {
  name: string
  /** One value per month or year, in the order of the chart's axis. */
  values: number[]
}

export interface AnalysisCount {
  name: string
  count: number
}

export interface AnalysisResults {
  group: string
  full: number
  partial: number
  zero: number
  unknown: number
}

export interface AnalysisCompleteness {
  year: number
  cases: number
  unknownResult: number
  noUnion: number
  protests: number
  noLocation: number
}

/** R36: per province (cityId is the plate code); a protest in several provinces counts in each, a case once. */
export interface AnalysisProvince {
  cityId: number
  name: string
  protests: number
  cases: number
}

export interface CaseAnalysis {
  totals: { cases: number; protests: number; provinces: number; knownResults: number }
  /** "2026-10" */
  months: string[]
  protestsByCategory: AnalysisSeries[]
  activeCases: number[]
  years: number[]
  reasonsByYear: AnalysisSeries[]
  protestTypesByYear: AnalysisSeries[]
  employmentByYear: AnalysisSeries[]
  confederations: AnalysisCount[]
  resultsByEmployment: AnalysisResults[]
  resultsByUnion: AnalysisResults[]
  durations: AnalysisCount[]
  completeness: AnalysisCompleteness[]
  provinces: AnalysisProvince[]
}

export function getAnalysis(filter: CaseFilter) {
  return api<CaseAnalysis>(`/api/analysis?${filterParams(filter)}`)
}
