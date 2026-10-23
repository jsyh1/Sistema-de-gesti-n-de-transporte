import type { SesionDto } from '../types/api'

const SESSION_KEY = 'ruta24-session'

export function readSession(): SesionDto | null {
  const value = sessionStorage.getItem(SESSION_KEY)
  if (!value) return null

  try {
    const session = JSON.parse(value) as SesionDto
    if (
      typeof session.accessToken !== 'string' ||
      typeof session.userId !== 'number' ||
      !['Administrador', 'Conductor'].includes(session.rol) ||
      Date.parse(session.expiresAt) <= Date.now()
    ) {
      clearSession()
      return null
    }
    return session
  } catch {
    clearSession()
    return null
  }
}

export function saveSession(session: SesionDto): void {
  sessionStorage.setItem(SESSION_KEY, JSON.stringify(session))
}

export function clearSession(): void {
  sessionStorage.removeItem(SESSION_KEY)
}

export function getAccessToken(): string | null {
  return readSession()?.accessToken ?? null
}
