import { useCallback, useEffect, useState } from 'react'
import { api, mensajeError } from '../services/api'
import { nombreDia } from '../types/api'
import type { Programacion } from '../types/api'

export function DriverDashboardPage({ nombreUsuario }: { nombreUsuario: string }) {
  const [programaciones, setProgramaciones] = useState<Programacion[]>([])
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)

  const cargar = useCallback(async () => {
    setLoading(true)
    setError('')
    try {
      setProgramaciones(await api.programaciones.mine())
    } catch (requestError) {
      setError(mensajeError(requestError))
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    void cargar()
  }, [cargar])

  return (
    <section className="driver-page">
      <header className="driver-welcome">
        <div>
          <p className="driver-eyebrow">PORTAL DEL CONDUCTOR</p>
          <h1>Hola, {nombreUsuario}</h1>
          <p>Consulta únicamente los horarios que te fueron asignados.</p>
        </div>
        <button className="driver-refresh" type="button" onClick={() => void cargar()} disabled={loading}>
          {loading ? 'Actualizando…' : 'Actualizar horarios'}
        </button>
      </header>

      <section className="driver-schedule" aria-labelledby="driver-schedule-title">
        <div className="driver-section-heading">
          <div>
            <h2 id="driver-schedule-title">Mi programación</h2>
            <p>{programaciones.length} {programaciones.length === 1 ? 'turno activo' : 'turnos activos'}</p>
          </div>
          <span className="driver-readonly">Solo consulta</span>
        </div>

        {error && <p className="driver-error" role="alert">{error}</p>}
        {loading ? (
          <p className="driver-empty">Cargando tus horarios…</p>
        ) : !error && programaciones.length === 0 ? (
          <p className="driver-empty">No tienes horarios activos asignados.</p>
        ) : (
          <div className="driver-schedule-list">
            {programaciones.map((programacion) => (
              <article className="driver-schedule-card" key={programacion.id}>
                <span className="driver-day">{nombreDia(programacion.diaSemana)}</span>
                <span className="driver-bus">Bus #{programacion.busId}</span>
                <span className="driver-hours">
                  {programacion.horaEntrada.slice(0, 5)} – {programacion.horaSalida.slice(0, 5)}
                </span>
              </article>
            ))}
          </div>
        )}
      </section>
    </section>
  )
}
