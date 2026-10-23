import { useEffect, useState } from 'react'
import { Icon } from '../components/Icon'
import { api, mensajeError } from '../services/api'
import { nombreDia, type Bus, type Conductor, type Programacion } from '../types/api'

type Section = 'resumen' | 'buses' | 'conductores' | 'horarios' | 'usuarios' | 'roles'
type DashboardData = { buses: Bus[]; conductores: Conductor[]; programaciones: Programacion[] }

const dayOrder = [1, 2, 3, 4, 5, 6, 7]

export function DashboardPage({ onNavigate }: { onNavigate: (section: Section) => void }) {
  const [data, setData] = useState<DashboardData | null>(null)
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    let active = true
    Promise.all([api.buses.list(), api.conductores.list(), api.programaciones.list()])
      .then(([buses, conductores, programaciones]) => {
        if (active) setData({ buses, conductores, programaciones })
      })
      .catch((cause: unknown) => {
        if (active) setError(mensajeError(cause))
      })
      .finally(() => {
        if (active) setLoading(false)
      })
    return () => { active = false }
  }, [])

  const busesDisponibles = data?.buses.filter((bus) => bus.estado === 0).length ?? 0
  const conductoresDisponibles = data?.conductores.filter((driver) => driver.estado === 0).length ?? 0
  const activeSchedules = data?.programaciones.filter((item) => item.activa) ?? []
  const unavailableText = loading ? 'Consultando API' : 'API sin conexión'

  return (
    <>
      <section className="welcome-banner">
        <div className="welcome-copy"><span className="welcome-tag"><span /> CENTRO DE CONTROL</span><h1>Todo en ruta.<br /><em>Todo bajo control.</em></h1><p>Administra tu flota y coordina los horarios de la semana desde un solo lugar.</p><button className="button button-light" onClick={() => onNavigate('horarios')}>Ver programación semanal<Icon name="arrow" size={17} /></button></div>
        <div className="welcome-art" aria-hidden="true"><div className="art-orbit orbit-one" /><div className="art-orbit orbit-two" /><div className="art-road"><span /><span /><span /></div><div className="art-bus"><Icon name="bus" size={92} /></div><span className="art-spark spark-one">✦</span><span className="art-spark spark-two">✦</span><span className="art-route">RUTA 24 <b>↗</b></span></div>
      </section>

      {error && <div className="notice notice-error" role="alert"><span>{error}</span><button className="button button-quiet" onClick={() => window.location.reload()}><Icon name="refresh" size={16} />Recargar</button></div>}
      <div className="section-title-row"><div><div className="eyebrow">VISTA GENERAL</div><h2>Tu operación de un vistazo</h2></div><span className="live-label"><span />Datos consultados a la API</span></div>

      <section className="metric-grid" aria-label="Resumen de la flota">
        <MetricCard icon="bus" label="Buses registrados" value={data?.buses.length ?? (loading ? '…' : '—')} detail={data ? `${busesDisponibles} disponibles` : unavailableText} tone="blue" onClick={() => onNavigate('buses')} />
        <MetricCard icon="users" label="Conductores" value={data?.conductores.length ?? (loading ? '…' : '—')} detail={data ? `${conductoresDisponibles} disponibles` : unavailableText} tone="green" onClick={() => onNavigate('conductores')} />
        <MetricCard icon="calendar" label="Horarios activos" value={data ? activeSchedules.length : (loading ? '…' : '—')} detail={data ? 'Programados esta semana' : unavailableText} tone="orange" onClick={() => onNavigate('horarios')} />
        <MetricCard icon="clock" label="Días programados" value={data ? new Set(activeSchedules.map((item) => item.diaSemana)).size : (loading ? '…' : '—')} detail={data ? 'De lunes a domingo' : unavailableText} tone="purple" onClick={() => onNavigate('horarios')} />
      </section>

      <div className="dashboard-grid">
        <section className="panel week-panel">
          <div className="panel-heading"><div><h2>Esta semana</h2><p>Próximos horarios programados</p></div><button className="text-button" onClick={() => onNavigate('horarios')}>Ver programación<Icon name="arrow" size={16} /></button></div>
          {!data && loading ? <div className="loading-state"><span className="spinner" />Consultando la operación…</div>
            : !data ? <div className="inline-empty"><span className="empty-icon"><Icon name="calendar" size={20} /></span><div><strong>No se cargaron los horarios</strong><span>Revisa la conexión con la API y vuelve a intentarlo.</span></div></div>
            : activeSchedules.length === 0 ? <div className="inline-empty"><span className="empty-icon"><Icon name="calendar" size={20} /></span><div><strong>Aún no hay horarios activos</strong><span>Agrega una programación para organizar la semana.</span></div><button className="button button-secondary" onClick={() => onNavigate('horarios')}>Crear horario</button></div>
              : <div className="week-list">{dayOrder.map((day) => {
                const schedules = activeSchedules.filter((item) => item.diaSemana === day).sort((a, b) => a.horaEntrada.localeCompare(b.horaEntrada))
                if (!schedules.length) return null
                return <div className="week-day-row" key={day}><div className="week-day-name"><span className="day-dot" />{nombreDia(day)}</div><div className="week-day-routes">{schedules.slice(0, 3).map((item) => <span className="route-chip" key={item.id}><Icon name="clock" size={14} />{item.horaEntrada.slice(0, 5)}–{item.horaSalida.slice(0, 5)}</span>)}{schedules.length > 3 && <span className="more-chip">+{schedules.length - 3}</span>}</div><span className="week-day-count">{schedules.length} {schedules.length === 1 ? 'ruta' : 'rutas'}</span></div>
              })}</div>}
        </section>
        <section className="panel fleet-panel">
          <div className="panel-heading"><div><h2>Estado de la flota</h2><p>Disponibilidad actual</p></div><span className="fleet-icon"><Icon name="bus" size={20} /></span></div>
          {!data && loading ? <div className="loading-state"><span className="spinner" />Cargando…</div> : !data ? <div className="inline-empty fleet-unavailable"><span className="empty-icon"><Icon name="bus" size={20} /></span><div><strong>Sin datos de la flota</strong><span>Verifica la conexión con la API.</span></div></div> : <div className="fleet-content">
            <div className="fleet-total"><strong>{data.buses.length}</strong><span>unidades registradas</span></div>
            <div className="fleet-bar" role="img" aria-label="Distribución del estado de los buses">
              {data.buses.length > 0 && data.buses.map((bus) => <span key={bus.id} className={`fleet-segment fleet-state-${bus.estado}`} title={`${bus.numeroIdentificacion}: ${busStateName(bus.estado)}`} />)}
            </div>
            <div className="fleet-legend"><span><i className="legend-dot legend-available" />Disponibles <b>{data.buses.filter((bus) => bus.estado === 0).length}</b></span><span><i className="legend-dot legend-assigned" />Asignados <b>{data.buses.filter((bus) => bus.estado === 1).length}</b></span><span><i className="legend-dot legend-other" />En revisión / fuera de servicio <b>{data.buses.filter((bus) => bus.estado > 1).length}</b></span></div>
          </div>}
          <button className="fleet-link" onClick={() => onNavigate('buses')}>Administrar buses<Icon name="arrow" size={16} /></button>
        </section>
      </div>

      <section className="quick-actions"><div><div className="eyebrow">ACCESO RÁPIDO</div><h2>¿Qué necesitas hacer?</h2></div><button className="quick-action" onClick={() => onNavigate('buses')}><span className="quick-icon quick-blue"><Icon name="plus" size={19} /></span><span><strong>Registrar un bus</strong><small>Añade una unidad a la flota</small></span><Icon name="arrow" size={17} /></button><button className="quick-action" onClick={() => onNavigate('conductores')}><span className="quick-icon quick-green"><Icon name="users" size={19} /></span><span><strong>Agregar conductor</strong><small>Registra un nuevo perfil</small></span><Icon name="arrow" size={17} /></button><button className="quick-action" onClick={() => onNavigate('horarios')}><span className="quick-icon quick-orange"><Icon name="calendar" size={19} /></span><span><strong>Programar horarios</strong><small>Organiza las rutas semanales</small></span><Icon name="arrow" size={17} /></button></section>
    </>
  )
}

function MetricCard({ icon, label, value, detail, tone, onClick }: { icon: string; label: string; value: number | string; detail: string; tone: string; onClick: () => void }) {
  return <button className="metric-card" onClick={onClick}><span className={`metric-icon metric-${tone}`}><Icon name={icon} size={20} /></span><span className="metric-label">{label}</span><strong className="metric-value">{value ?? '—'}</strong><span className="metric-detail">{detail}<Icon name="arrow" size={14} /></span></button>
}

function busStateName(state: number): string {
  return ['Disponible', 'Asignado', 'En mantenimiento', 'Fuera de servicio'][state] ?? 'Sin estado'
}
