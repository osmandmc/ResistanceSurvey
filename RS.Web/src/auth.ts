import { ref } from 'vue'
import { api, ApiError } from './api'

export interface CurrentUser {
  username: string
  firstName: string | null
  lastName: string | null
  role: string
}

/** The signed-in user, or null. Loaded once on start, then kept in step with sign-in and sign-out. */
export const currentUser = ref<CurrentUser | null>(null)
let loaded = false

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

export function displayName(user: CurrentUser) {
  const fullName = [user.firstName, user.lastName].filter(Boolean).join(' ')
  return fullName || user.username
}
