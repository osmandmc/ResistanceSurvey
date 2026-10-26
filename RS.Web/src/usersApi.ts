import { api } from './api'

// Tanımlar → Kullanıcılar (F4a): accounts and their one-time links. Yönetici only.

export type UserState = 'active' | 'invited' | 'linkExpired' | 'disabled'

export interface UserRow {
  id: number
  username: string
  role: string
  state: UserState
  lastSignInAt: string | null
  linkExpiresAt: string | null
  createdAt: string
  createdBy: string | null
}

/** The account and, for a new account or "Yeni bağlantı", the link's token (shown once). */
export interface LinkResult {
  user: UserRow
  token: string | null
}

export interface Invitation {
  username: string
  /** The account already has a password: the link sets a new one. */
  hasPassword: boolean
}

export const listUsers = () => api<UserRow[]>('/api/users')

export const createUser = (username: string, role: string) =>
  api<LinkResult>('/api/users', { method: 'POST', body: JSON.stringify({ username, role }) })

export const newLink = (id: number) => api<LinkResult>(`/api/users/${id}/link`, { method: 'POST' })

export const changeUser = (id: number, change: { role?: string; disabled?: boolean }) =>
  api<LinkResult>(`/api/users/${id}`, { method: 'PATCH', body: JSON.stringify(change) })

/** The whole link to send to the person. */
export const linkFor = (token: string) => `${window.location.origin}/app/welcome?token=${encodeURIComponent(token)}`

export const getInvitation = (token: string) => api<Invitation>(`/api/auth/invitation?token=${encodeURIComponent(token)}`)

export const acceptInvitation = (token: string, password: string) =>
  api<{ id: number; username: string; role: string }>('/api/auth/invitation', {
    method: 'POST',
    body: JSON.stringify({ token, password }),
  })
