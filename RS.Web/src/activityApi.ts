import { api } from './api'

// The activity log (F4b): who did what, when.

export interface ActivityRow {
  id: number
  at: string
  username: string | null
  action: string
  caseId: number | null
  articleId: string | null
  subject: string | null
  subjectId: number | null
  summary: string | null
}

export interface ActivityPage {
  items: ActivityRow[]
  total: number
  page: number
  pageSize: number
  usernames: string[]
}

export type ActivityArea = 'inbox' | 'cases' | 'definitions' | 'users' | 'uploads' | 'prompts'

export const areas: { value: ActivityArea; label: string }[] = [
  { value: 'inbox', label: 'Gelen Kutusu' },
  { value: 'cases', label: 'Vakalar' },
  { value: 'definitions', label: 'Tanımlar' },
  { value: 'users', label: 'Kullanıcılar' },
  { value: 'uploads', label: 'Yükleme' },
  { value: 'prompts', label: 'AI istemleri' },
]

/** What was done, as people read it. */
const actionLabels: Record<string, string> = {
  'article.new-case': 'Haberden yeni vaka',
  'article.add-protest': 'Haberden vakaya eylem',
  'article.link': 'Haberi vakaya bağladı',
  'article.dismiss': 'Haberi ilgisiz işaretledi',
  'case.create': 'Yeni vaka',
  'case.update': 'Vakayı düzenledi',
  'case.delete': 'Vakayı sildi',
  'case.attach-article': 'Vakaya haber ekledi',
  'case.detach-article': 'Vakadan haber çıkardı',
  'protest.add': 'Eylem ekledi',
  'protest.update': 'Eylemi düzenledi',
  'protest.delete': 'Eylemi sildi',
  'case.solidarity-resolve': 'Dayanışma grubunu bağladı',
  'case.target-check': 'Eski hedef kurumu kontrol etti',
  'definition.update': 'Tanımı düzenledi',
  'definition.approve': 'Tanımı onayladı',
  'definition.unapprove': 'Onayı kaldırdı',
  'definition.delete': 'Tanımı sildi',
  'definition.merge': 'Birleştirdi',
  'definition.not-duplicate': '“Aynı değil” dedi',
  'definition.company-create': 'Şirket ekledi',
  'definition.corporation-create': 'Kurum ekledi',
  'definition.workplace-create': 'İşyeri ekledi',
  'definition.company-link-add': 'Taşeron ilişkisi ekledi',
  'definition.company-link-remove': 'Taşeron ilişkisini kaldırdı',
  'definition.company-relation-add': 'Şirket ilişkisi ekledi',
  'definition.company-relation-change': 'Şirket ilişkisini değiştirdi',
  'definition.company-relation-remove': 'Şirket ilişkisini kaldırdı',
  'user.create': 'Kullanıcı davet etti',
  'user.link': 'Yeni bağlantı verdi',
  'user.role': 'Rolü değiştirdi',
  'user.disable': 'Hesabı devre dışı bıraktı',
  'user.enable': 'Hesabı etkinleştirdi',
  'user.password': 'Şifresini belirledi',
  'upload.excel': 'Excel yükledi',
  'upload.links': 'Bağlantı gönderdi',
  'upload.local-news': 'Eski haberleri gönderdi',
  'prompt.create': 'İstem sürümü kaydetti',
  'prompt.try': 'İstemi denedi',
  'prompt.activate': 'İstemi etkinleştirdi',
}

export const actionLabel = (action: string) => actionLabels[action] ?? action

/** Where the line's subject can be seen, if anywhere. */
export function activityLink(row: ActivityRow): string | null {
  if (row.caseId) return `/cases/${row.caseId}`
  if (row.subject === 'companies' && row.subjectId) return `/definitions/companies/${row.subjectId}`
  if (row.subject === 'user') return '/definitions/users'
  if (row.subject && row.action.startsWith('definition.')) return `/definitions/${row.subject}`
  if (row.action.startsWith('prompt.')) return '/definitions/prompts'
  return null
}

export function listActivity(filter: { user?: string | null; area?: ActivityArea | null; page?: number }) {
  const query = new URLSearchParams()
  if (filter.user) query.set('user', filter.user)
  if (filter.area) query.set('area', filter.area)
  if (filter.page && filter.page > 1) query.set('page', String(filter.page))
  const text = query.toString()
  return api<ActivityPage>(`/api/activity${text ? `?${text}` : ''}`)
}

export const caseActivity = (caseId: number) => api<ActivityRow[]>(`/api/activity/cases/${caseId}`)
