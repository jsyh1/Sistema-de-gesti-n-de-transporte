import { useCallback, useEffect, useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import { Icon } from '../components/Icon'
import { api, mensajeError } from '../services/api'
import { diasSemana, nombreDia, type Bus, type Conductor, type DiaSemana, type Programacion, type ProgramacionActivaBusDia } from '../types/api'

type Tab = 'horarios' | 'disponibilidad' | 'vista-conductor'
type ScheduleForm = { busId: string; conductorId: string; diaSemana: string; horaEntrada: string; horaSalida: string; activa: string }
type ActiveForm = { busId: string; diaSemana: string }

const emptySchedule: ScheduleForm = { busId: '', conductorId: '', diaSemana: '1', horaEntrada: '08:00', horaSalida: '17:00', activa: 'true' }
const emptyActive: ActiveForm = { busId: '', diaSemana: '1' }

async function fetchProgramData() {
  const [schedules, buses, conductores, activeDays] = await Promise.all([
    api.programaciones.list(),
    api.buses.list(),
    api.conductores.list(),
    api.programacionesActivas.list(),
  ])
  return { schedules, buses, conductores, activeDays }
}

function parseDay(value: string): DiaSemana {
  const day = diasSemana.find((item) => String(item.value) === value)
  if (!day) throw new Error('Selecciona un día de la semana válido.')
  return day.value
}

export function ProgramacionesPage() {
  const [schedules, setSchedules] = useState<Programacion[]>([])
  const [buses, setBuses] = useState<Bus[]>([])
  const [conductores, setConductores] = useState<Conductor[]>([])
  const [activeDays, setActiveDays] = useState<ProgramacionActivaBusDia[]>([])
  const [selectedDriverId, setSelectedDriverId] = useState('')
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [notice, setNotice] = useState('')
  const [tab, setTab] = useState<Tab>('horarios')
  const [query, setQuery] = useState('')
  const [scheduleModal, setScheduleModal] = useState(false)
  const [activeModal, setActiveModal] = useState(false)
  const [scheduleEditing, setScheduleEditing] = useState<Programacion | null>(null)
  const [activeEditing, setActiveEditing] = useState<ProgramacionActivaBusDia | null>(null)
  const [scheduleForm, setScheduleForm] = useState<ScheduleForm>(emptySchedule)
  const [activeForm, setActiveForm] = useState<ActiveForm>(emptyActive)
  const [deletingKey, setDeletingKey] = useState('')

  const load = useCallback(async () => {
    setLoading(true)
    setError('')
    try {
      const data = await fetchProgramData()
      setSchedules(data.schedules)
      setBuses(data.buses)
      setConductores(data.conductores)
      setActiveDays(data.activeDays)
    } catch (cause) {
      setError(mensajeError(cause))
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    let active = true
    fetchProgramData().then((data) => {
      if (!active) return
      setSchedules(data.schedules)
      setBuses(data.buses)
      setConductores(data.conductores)
      setActiveDays(data.activeDays)
    }).catch((cause: unknown) => {
      if (active) setError(mensajeError(cause))
    }).finally(() => {
      if (active) setLoading(false)
    })
    return () => { active = false }
  }, [])
  useEffect(() => {
    if (!notice) return
    const timeout = window.setTimeout(() => setNotice(''), 3600)
    return () => window.clearTimeout(timeout)
  }, [notice])

  const filteredSchedules = useMemo(() => {
    const normalized = query.trim().toLocaleLowerCase('es')
    return schedules.filter((schedule) => {
      const bus = buses.find((item) => item.id === schedule.busId)
      const driver = conductores.find((item) => item.id === schedule.conductorId)
      return !normalized || `${bus?.numeroIdentificacion ?? schedule.busId} ${driver?.nombre ?? schedule.conductorId} ${nombreDia(schedule.diaSemana)}`.toLocaleLowerCase('es').includes(normalized)
    }).sort((a, b) => a.diaSemana - b.diaSemana || a.horaEntrada.localeCompare(b.horaEntrada))
  }, [buses, conductores, query, schedules])

  const visibleDriverId = selectedDriverId || (conductores[0] ? String(conductores[0].id) : '')
  const driverSchedules = useMemo(() => schedules
    .filter((schedule) => schedule.conductorId === Number(visibleDriverId) && schedule.activa)
    .sort((a, b) => a.diaSemana - b.diaSemana || a.horaEntrada.localeCompare(b.horaEntrada)), [schedules, visibleDriverId])

  const openSchedule = (schedule?: Programacion) => {
    setScheduleEditing(schedule ?? null)
    setScheduleForm(schedule ? {
      busId: String(schedule.busId),
      conductorId: String(schedule.conductorId),
      diaSemana: String(schedule.diaSemana),
      horaEntrada: schedule.horaEntrada.slice(0, 5),
      horaSalida: schedule.horaSalida.slice(0, 5),
      activa: String(schedule.activa),
    } : { ...emptySchedule, busId: buses[0] ? String(buses[0].id) : '', conductorId: conductores[0] ? String(conductores[0].id) : '' })
    setScheduleModal(true)
  }

  const saveSchedule = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setSaving(true)
    setError('')
    try {
      const body = {
        busId: Number(scheduleForm.busId),
        conductorId: Number(scheduleForm.conductorId),
        diaSemana: parseDay(scheduleForm.diaSemana),
        horaEntrada: `${scheduleForm.horaEntrada}:00`,
        horaSalida: `${scheduleForm.horaSalida}:00`,
        activa: scheduleForm.activa === 'true',
      }
      if (scheduleEditing) await api.programaciones.update(scheduleEditing.id, body)
      else await api.programaciones.create(body)
      setScheduleModal(false)
      setNotice(`Horario ${scheduleEditing ? 'actualizado' : 'creado'} correctamente.`)
      await load()
    } catch (cause) {
      setError(mensajeError(cause))
    } finally {
      setSaving(false)
    }
  }

  const removeSchedule = async (schedule: Programacion) => {
    if (!window.confirm('¿Eliminar esta programación? Esta acción no se puede deshacer.')) return
    setDeletingKey(`schedule-${schedule.id}`)
    setError('')
    try {
      await api.programaciones.remove(schedule.id)
      setSchedules((current) => current.filter((item) => item.id !== schedule.id))
      setNotice('Programación eliminada correctamente.')
    } catch (cause) {
      setError(mensajeError(cause))
    } finally {
      setDeletingKey('')
    }
  }

  const openActive = (entry?: ProgramacionActivaBusDia) => {
    setActiveEditing(entry ?? null)
    setActiveForm(entry
      ? { busId: String(entry.busId), diaSemana: String(entry.diaSemana) }
      : { ...emptyActive, busId: buses[0] ? String(buses[0].id) : '' })
    setActiveModal(true)
  }

  const saveActive = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setSaving(true)
    setError('')
    try {
      const body = { busId: Number(activeForm.busId), diaSemana: parseDay(activeForm.diaSemana) }
      if (activeEditing) await api.programacionesActivas.update(activeEditing.busId, activeEditing.diaSemana, body)
      else await api.programacionesActivas.create(body)
      setActiveModal(false)
      setNotice(`Disponibilidad ${activeEditing ? 'actualizada' : 'activada'} correctamente.`)
      await load()
    } catch (cause) {
      setError(mensajeError(cause))
    } finally {
      setSaving(false)
    }
  }

  const removeActive = async (entry: ProgramacionActivaBusDia) => {
    const busName = buses.find((bus) => bus.id === entry.busId)?.numeroIdentificacion ?? `#${entry.busId}`
    if (!window.confirm(`¿Desactivar ${busName} para el día ${nombreDia(entry.diaSemana)}?`)) return
    const key = `active-${entry.busId}-${entry.diaSemana}`
    setDeletingKey(key)
    setError('')
    try {
      await api.programacionesActivas.remove(entry.busId, entry.diaSemana)
      setActiveDays((current) => current.filter((item) => item.busId !== entry.busId || item.diaSemana !== entry.diaSemana))
      setNotice('Disponibilidad desactivada correctamente.')
    } catch (cause) {
      setError(mensajeError(cause))
    } finally {
      setDeletingKey('')
    }
  }

  const closeModals = () => {
    if (saving) return
    setScheduleModal(false)
    setActiveModal(false)
  }

  return (
    <>
      <section className="page-heading">
        <div><div className="eyebrow">OPERACIÓN SEMANAL</div><h1>Programación</h1><p>Coordina cada unidad y conductor para que cada ruta tenga su horario.</p></div>
        {tab !== 'vista-conductor' && <button className="button button-primary" onClick={() => tab === 'horarios' ? openSchedule() : openActive()} disabled={tab === 'horarios' ? buses.length === 0 || conductores.length === 0 : buses.length === 0}><Icon name="plus" size={18} />{tab === 'horarios' ? 'Nuevo horario' : 'Activar un bus'}</button>}
      </section>
      {notice && <div className="notice notice-success" role="status"><span>{notice}</span><button onClick={() => setNotice('')} aria-label="Cerrar aviso"><Icon name="close" size={16} /></button></div>}
      {error && <div className="notice notice-error" role="alert"><span>{error}</span><button className="button button-quiet" onClick={() => void load()}><Icon name="refresh" size={16} />Reintentar</button></div>}
      {(!buses.length || !conductores.length) && !loading && !error && <div className="notice notice-info"><span>Para crear horarios necesitas registrar al menos un bus y un conductor.</span></div>}
      <section className="panel schedule-panel">
        <div className="schedule-tabs" role="tablist" aria-label="Secciones de programación">
          <button role="tab" aria-selected={tab === 'horarios'} className={tab === 'horarios' ? 'schedule-tab-active' : ''} onClick={() => setTab('horarios')}><Icon name="calendar" size={17} />Horarios<span>{schedules.length}</span></button>
          <button role="tab" aria-selected={tab === 'disponibilidad'} className={tab === 'disponibilidad' ? 'schedule-tab-active' : ''} onClick={() => setTab('disponibilidad')}><Icon name="bus" size={17} />Buses activos por día<span>{activeDays.length}</span></button>
          <button role="tab" aria-selected={tab === 'vista-conductor'} className={tab === 'vista-conductor' ? 'schedule-tab-active' : ''} onClick={() => setTab('vista-conductor')}><Icon name="users" size={17} />Vista por conductor</button>
        </div>
        <div className="resource-toolbar schedule-toolbar">
          <div><h2>{tab === 'horarios' ? 'Programaciones semanales' : tab === 'disponibilidad' ? 'Disponibilidad de unidades' : 'Vista semanal del conductor'}</h2><p>{tab === 'horarios' ? 'Asignación de buses, conductores y horas' : tab === 'disponibilidad' ? 'Indica qué buses operan cada día de la semana' : 'Consulta los horarios activos asignados a una persona'}</p></div>
          {tab !== 'vista-conductor' && <label className="search-field"><Icon name="search" size={18} /><input aria-label="Buscar programación" placeholder={tab === 'horarios' ? 'Buscar bus, conductor o día' : 'Filtra los registros'} value={query} onChange={(event) => setQuery(event.target.value)} /></label>}
        </div>
        {loading ? <div className="loading-state"><span className="spinner" />Cargando programación…</div>
          : tab === 'vista-conductor' ? <div className="driver-view"><div className="driver-view-controls"><label className="form-field"><span>Conductor</span><select value={visibleDriverId} onChange={(event) => setSelectedDriverId(event.target.value)}><option value="">Selecciona un conductor</option>{conductores.map((driver) => <option key={driver.id} value={driver.id}>{driver.nombre} · {driver.numeroLicencia}</option>)}</select></label><div className="notice notice-info driver-view-note"><span>Vista de consulta. La API actual no ofrece inicio de sesión ni permisos por rol.</span></div></div>{!visibleDriverId ? <div className="empty-state"><div className="empty-icon"><Icon name="users" size={22} /></div><h3>No hay conductores disponibles</h3><p>Registra un conductor para consultar su programación.</p></div> : driverSchedules.length === 0 ? <div className="empty-state"><div className="empty-icon"><Icon name="calendar" size={22} /></div><h3>Sin horarios activos asignados</h3><p>Este conductor todavía no tiene turnos activos esta semana.</p></div> : <div className="driver-week-list">{driverSchedules.map((schedule) => {
              const bus = buses.find((item) => item.id === schedule.busId)
              return <article className="driver-shift" key={schedule.id}><div className="driver-shift-day"><span>{nombreDia(schedule.diaSemana)}</span><small>Programación semanal</small></div><div className="driver-shift-bus"><span className="fleet-icon"><Icon name="bus" size={18} /></span><span><strong>{bus?.numeroIdentificacion ?? `Bus #${schedule.busId}`}</strong><small>{bus?.modelo ?? 'Unidad asignada'}</small></span></div><div className="driver-shift-time"><Icon name="clock" size={16} /><span>{schedule.horaEntrada.slice(0, 5)} — {schedule.horaSalida.slice(0, 5)}</span></div></article>
            })}</div>}</div>
            : tab === 'horarios' ? filteredSchedules.length === 0 ? <div className="empty-state"><div className="empty-icon"><Icon name="calendar" size={22} /></div><h3>{schedules.length ? 'Sin resultados' : 'Todavía no hay horarios'}</h3><p>{schedules.length ? 'Prueba con otro término.' : 'Crea la primera programación para organizar las rutas.'}</p>{!schedules.length && <button className="button button-secondary" onClick={() => openSchedule()} disabled={!buses.length || !conductores.length}><Icon name="plus" size={17} />Crear horario</button>}</div>
            : <div className="table-scroll"><table><thead><tr><th>Día</th><th>Bus</th><th>Conductor</th><th>Entrada</th><th>Salida</th><th>Estado</th><th className="action-column">Acciones</th></tr></thead><tbody>{filteredSchedules.map((schedule) => {
              const bus = buses.find((item) => item.id === schedule.busId)
              const driver = conductores.find((item) => item.id === schedule.conductorId)
              return <tr key={schedule.id}><td><span className="day-chip">{nombreDia(schedule.diaSemana)}</span></td><td><strong className="table-primary">{bus?.numeroIdentificacion ?? `Bus #${schedule.busId}`}</strong><span className="table-subtitle">{bus?.modelo ?? 'Unidad'}</span></td><td>{driver?.nombre ?? `Conductor #${schedule.conductorId}`}<span className="table-subtitle">{driver?.numeroLicencia ?? 'Licencia no disponible'}</span></td><td><span className="time-cell"><Icon name="clock" size={15} />{schedule.horaEntrada.slice(0, 5)}</span></td><td><span className="time-cell">{schedule.horaSalida.slice(0, 5)}</span></td><td><span className={`status-pill ${schedule.activa ? 'status-good' : 'status-muted'}`}><span />{schedule.activa ? 'Activa' : 'Inactiva'}</span></td><td><div className="row-actions"><button className="icon-button action-edit" aria-label="Editar horario" title="Editar" onClick={() => openSchedule(schedule)}><Icon name="edit" size={17} /></button><button className="icon-button action-delete" aria-label="Eliminar horario" title="Eliminar" disabled={deletingKey === `schedule-${schedule.id}`} onClick={() => void removeSchedule(schedule)}><Icon name="trash" size={17} /></button></div></td></tr>
            })}</tbody></table></div>
            : activeDays.length === 0 ? <div className="empty-state"><div className="empty-icon"><Icon name="bus" size={22} /></div><h3>No hay buses activos por día</h3><p>Activa una unidad para registrar su disponibilidad semanal.</p><button className="button button-secondary" onClick={() => openActive()} disabled={!buses.length}><Icon name="plus" size={17} />Activar un bus</button></div>
              : <div className="table-scroll"><table><thead><tr><th>Unidad</th><th>Modelo</th><th>Día de operación</th><th className="action-column">Acciones</th></tr></thead><tbody>{activeDays.filter((entry) => {
                const bus = buses.find((item) => item.id === entry.busId)
                return !query.trim() || `${bus?.numeroIdentificacion ?? entry.busId} ${nombreDia(entry.diaSemana)}`.toLocaleLowerCase('es').includes(query.trim().toLocaleLowerCase('es'))
              }).sort((a, b) => a.diaSemana - b.diaSemana).map((entry) => {
                const bus = buses.find((item) => item.id === entry.busId)
                const key = `active-${entry.busId}-${entry.diaSemana}`
                return <tr key={key}><td><strong className="table-primary">{bus?.numeroIdentificacion ?? `Bus #${entry.busId}`}</strong></td><td>{bus?.modelo ?? 'Unidad'}</td><td><span className="day-chip">{nombreDia(entry.diaSemana)}</span></td><td><div className="row-actions"><button className="icon-button action-edit" aria-label="Editar disponibilidad" title="Editar" onClick={() => openActive(entry)}><Icon name="edit" size={17} /></button><button className="icon-button action-delete" aria-label="Desactivar bus" title="Desactivar" disabled={deletingKey === key} onClick={() => void removeActive(entry)}><Icon name="trash" size={17} /></button></div></td></tr>
              })}</tbody></table></div>}
      </section>
      <div className="schedule-footnote"><Icon name="clock" size={16} /><span>La hora de salida debe ser posterior a la hora de entrada dentro del mismo día.</span></div>
      {(scheduleModal || activeModal) && <div className="modal-backdrop" onMouseDown={(event) => { if (event.target === event.currentTarget) closeModals() }}>
        {scheduleModal ? <section className="modal-card" role="dialog" aria-modal="true" aria-labelledby="schedule-title"><header className="modal-header"><div><span className="eyebrow">{scheduleEditing ? 'EDITAR PROGRAMACIÓN' : 'NUEVA PROGRAMACIÓN'}</span><h2 id="schedule-title">{scheduleEditing ? 'Editar horario' : 'Programar un horario'}</h2></div><button className="icon-button" aria-label="Cerrar" onClick={closeModals}><Icon name="close" /></button></header>
          <form onSubmit={(event) => void saveSchedule(event)}>
            <label className="form-field"><span>Bus *</span><select required value={scheduleForm.busId} onChange={(event) => setScheduleForm((current) => ({ ...current, busId: event.target.value }))}><option value="" disabled>Selecciona un bus</option>{buses.map((bus) => <option key={bus.id} value={bus.id}>{bus.numeroIdentificacion} · {bus.modelo}</option>)}</select></label>
            <label className="form-field"><span>Conductor *</span><select required value={scheduleForm.conductorId} onChange={(event) => setScheduleForm((current) => ({ ...current, conductorId: event.target.value }))}><option value="" disabled>Selecciona un conductor</option>{conductores.map((driver) => <option key={driver.id} value={driver.id}>{driver.nombre} · {driver.numeroLicencia}</option>)}</select></label>
            <label className="form-field"><span>Día de la semana *</span><select required value={scheduleForm.diaSemana} onChange={(event) => setScheduleForm((current) => ({ ...current, diaSemana: event.target.value }))}>{diasSemana.map((day) => <option key={day.value} value={day.value}>{day.label}</option>)}</select></label>
            <div className="form-grid"><label className="form-field"><span>Hora de entrada *</span><input type="time" required value={scheduleForm.horaEntrada} onChange={(event) => setScheduleForm((current) => ({ ...current, horaEntrada: event.target.value }))} /></label><label className="form-field"><span>Hora de salida *</span><input type="time" required min={scheduleForm.horaEntrada} value={scheduleForm.horaSalida} onChange={(event) => setScheduleForm((current) => ({ ...current, horaSalida: event.target.value }))} /></label></div>
            <label className="form-field form-checkbox"><input type="checkbox" checked={scheduleForm.activa === 'true'} onChange={(event) => setScheduleForm((current) => ({ ...current, activa: String(event.target.checked) }))} /><span>Programación activa</span></label>
            <div className="modal-actions"><button className="button button-secondary" type="button" disabled={saving} onClick={closeModals}>Cancelar</button><button className="button button-primary" type="submit" disabled={saving}>{saving && <span className="spinner spinner-light" />}{saving ? 'Guardando…' : scheduleEditing ? 'Guardar cambios' : 'Crear horario'}</button></div>
          </form>
        </section> : <section className="modal-card" role="dialog" aria-modal="true" aria-labelledby="active-title"><header className="modal-header"><div><span className="eyebrow">{activeEditing ? 'EDITAR DISPONIBILIDAD' : 'NUEVA DISPONIBILIDAD'}</span><h2 id="active-title">{activeEditing ? 'Editar bus activo' : 'Activar un bus por día'}</h2></div><button className="icon-button" aria-label="Cerrar" onClick={closeModals}><Icon name="close" /></button></header>
          <form onSubmit={(event) => void saveActive(event)}>
            <label className="form-field"><span>Bus *</span><select required value={activeForm.busId} onChange={(event) => setActiveForm((current) => ({ ...current, busId: event.target.value }))}><option value="" disabled>Selecciona un bus</option>{buses.map((bus) => <option key={bus.id} value={bus.id}>{bus.numeroIdentificacion} · {bus.modelo}</option>)}</select></label>
            <label className="form-field"><span>Día de operación *</span><select required value={activeForm.diaSemana} onChange={(event) => setActiveForm((current) => ({ ...current, diaSemana: event.target.value }))}>{diasSemana.map((day) => <option key={day.value} value={day.value}>{day.label}</option>)}</select></label>
            <div className="modal-actions"><button className="button button-secondary" type="button" disabled={saving} onClick={closeModals}>Cancelar</button><button className="button button-primary" type="submit" disabled={saving}>{saving && <span className="spinner spinner-light" />}{saving ? 'Guardando…' : activeEditing ? 'Guardar cambios' : 'Activar disponibilidad'}</button></div>
          </form>
        </section>}
      </div>}
    </>
  )
}
