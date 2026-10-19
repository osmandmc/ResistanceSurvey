// Small client for the app's own API (RS.MVC). The sign-in cookie is sent automatically (same origin).

export class ApiError extends Error {
  constructor(
    readonly status: number,
    message: string,
  ) {
    super(message)
  }
}

let onUnauthorized: () => void = () => {}

/** Called when the session has expired or is missing (any 401 outside /api/auth). */
export function setUnauthorizedHandler(handler: () => void) {
  onUnauthorized = handler
}

export async function api<T>(path: string, init: RequestInit = {}): Promise<T> {
  const response = await fetch(path, {
    ...init,
    credentials: 'same-origin',
    headers: {
      Accept: 'application/json',
      ...(init.body ? { 'Content-Type': 'application/json' } : {}),
      ...init.headers,
    },
  })

  if (response.status === 401 && !path.startsWith('/api/auth/')) {
    onUnauthorized()
  }
  if (!response.ok) {
    let message = response.statusText
    try {
      const body = await response.json()
      message = body.error ?? message
    } catch {
      // not JSON; keep the status text
    }
    throw new ApiError(response.status, message)
  }
  if (response.status === 204) {
    return undefined as T
  }
  return (await response.json()) as T
}
