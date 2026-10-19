// Small client for the app's own API (RS.MVC). The sign-in cookie is sent automatically (same origin).

export class ApiError extends Error {
  constructor(
    readonly status: number,
    message: string,
    /** The server's JSON answer, e.g. { error, fields } for a 400 with missing fields. */
    readonly details?: Record<string, unknown>,
  ) {
    super(message)
  }

  /** Field path → message, when the server listed missing fields (400). */
  get fields(): Record<string, string> {
    return (this.details?.fields as Record<string, string> | undefined) ?? {}
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
    let details: Record<string, unknown> | undefined
    try {
      details = await response.json()
      message = (details?.error as string | undefined) ?? message
    } catch {
      // not JSON; keep the status text
    }
    throw new ApiError(response.status, message, details)
  }
  if (response.status === 204) {
    return undefined as T
  }
  return (await response.json()) as T
}
