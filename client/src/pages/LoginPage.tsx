import { useState, type FormEvent } from 'react'
import { api, mensajeError } from '../services/api'
import type { SesionDto } from '../types/api'

export function LoginPage({ onLogin }: { onLogin: (session: SesionDto) => void }) {
  const [nombreUsuario, setNombreUsuario] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setLoading(true)
    setError('')
    try {
      onLogin(await api.auth.login({ nombreUsuario, password }))
    } catch (requestError) {
      setError(mensajeError(requestError))
    } finally {
      setLoading(false)
    }
  }

  return (
    <main className="auth-page">
      <section className="auth-card" aria-labelledby="login-title">
        <div className="auth-brand-mark" aria-hidden="true">R24</div>
        <p className="auth-eyebrow">SISTEMA DE GESTIÓN DE TRANSPORTE</p>
        <h1 id="login-title">Bienvenido</h1>
        <p className="auth-description">Inicia sesión para consultar tu espacio de trabajo.</p>
        <form className="auth-form" onSubmit={submit}>
          <label htmlFor="login-username">Usuario</label>
          <input
            id="login-username"
            autoComplete="username"
            value={nombreUsuario}
            onChange={(event) => setNombreUsuario(event.target.value)}
            required
          />
          <label htmlFor="login-password">Contraseña</label>
          <input
            id="login-password"
            type="password"
            autoComplete="current-password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            required
          />
          {error && <p className="auth-error" role="alert">{error}</p>}
          <button className="auth-submit" type="submit" disabled={loading}>
            {loading ? 'Validando…' : 'Iniciar sesión'}
          </button>
        </form>
        <p className="auth-footnote">Acceso habilitado para personal autorizado.</p>
      </section>
    </main>
  )
}
