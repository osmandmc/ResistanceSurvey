import { computed, ref } from 'vue'
import { api, ApiError } from './api'

export interface CurrentUser {
  id: number
  username: string
  /** 'Admin' (yönetici) or 'User' (araştırmacı). */
  role: string
}

/** The signed-in user, or null. Loaded once on start, then kept in step with sign-in and sign-out. */
export const currentUser = ref<CurrentUser | null>(null)
let loaded = false

/** Yöneticiler also manage the accounts (Tanımlar → Kullanıcılar). */
export const isAdmin = computed(() => currentUser.value?.role === 'Admin')

export async function loadCurrentUser(): Promise<CurrentUser | null> {
  if (!loaded) {
    try {
      currentUser.value = await api<CurrentUser>('/api/auth/me')
    } catch (error) {
      if (!(error instanceof ApiError && error.status === 401)) {
        throw error
      }
      currentUser.value = null
    }
    loaded = true
  }
  return currentUser.value
}

export async function signIn(username: string, password: string) {
  currentUser.value = await api<CurrentUser>('/api/auth/login', {
    method: 'POST',
    body: JSON.stringify({ username, password }),
  })
  loaded = true
}

export async function signOut() {
  await api<void>('/api/auth/logout', { method: 'POST' })
  forgetCurrentUser()
}

/** After a 401: the session is gone, so the next protected page asks for sign-in again. */
export function forgetCurrentUser() {
  currentUser.value = null
  loaded = true
}

/** After setting a password through an invitation link, the server has signed the person in. */
export function setCurrentUser(user: CurrentUser) {
  currentUser.value = user
  loaded = true
}
