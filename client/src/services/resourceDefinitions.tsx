import type { ReactNode } from 'react'
import { api } from './api'
import { EstadoBus, EstadoConductor, type Bus, type Conductor, type Rol, type Usuario } from '../types/api'
import type { CrudDefinition } from '../components/CrudPage'

const busStates = [
  { value: String(EstadoBus.Disponible), label: 'Disponible' },
  { value: String(EstadoBus.Asignado), label: 'Asignado' },
  { value: String(EstadoBus.EnMantenimiento), label: 'En mantenimiento' },
  { value: String(EstadoBus.FueraDeServicio), label: 'Fuera de servicio' },
]
const driverStates = [
  { value: String(EstadoConductor.Disponible), label: 'Disponible' },
  { value: String(EstadoConductor.Asignado), label: 'Asignado' },
  { value: String(EstadoConductor.Descanso), label: 'En descanso' },
  { value: String(EstadoConductor.Inactivo), label: 'Inactivo' },
]

function statePill(label: string, state: 'good' | 'warning' | 'muted' | 'danger'): ReactNode {
  return <span className={`status-pill status-${state}`}><span />{label}</span>
}

function busState(value: EstadoBus) {
  const item = busStates.find((state) => Number(state.value) === value)
  const variant = value === EstadoBus.Disponible ? 'good' : value === EstadoBus.Asignado ? 'warning' : value === EstadoBus.EnMantenimiento ? 'muted' : 'danger'
  return statePill(item?.label ?? 'Sin estado', variant)
}

function driverState(value: EstadoConductor) {
  const item = driverStates.find((state) => Number(state.value) === value)
  const variant = value === EstadoConductor.Disponible ? 'good' : value === EstadoConductor.Asignado ? 'warning' : value === EstadoConductor.Descanso ? 'muted' : 'danger'
  return statePill(item?.label ?? 'Sin estado', variant)
}

function inputValue(form: Record<string, string>, name: string): string {
  return form[name] ?? ''
}

function parseBusState(value: string): EstadoBus {
  switch (Number(value)) {
    case EstadoBus.Disponible: return EstadoBus.Disponible
    case EstadoBus.Asignado: return EstadoBus.Asignado
    case EstadoBus.EnMantenimiento: return EstadoBus.EnMantenimiento
    case EstadoBus.FueraDeServicio: return EstadoBus.FueraDeServicio
    default: throw new Error('Selecciona un estado válido para el bus.')
  }
}

function parseDriverState(value: string): EstadoConductor {
  switch (Number(value)) {
    case EstadoConductor.Disponible: return EstadoConductor.Disponible
    case EstadoConductor.Asignado: return EstadoConductor.Asignado
    case EstadoConductor.Descanso: return EstadoConductor.Descanso
    case EstadoConductor.Inactivo: return EstadoConductor.Inactivo
    default: throw new Error('Selecciona un estado válido para el conductor.')
  }
}

export const busesDefinition: CrudDefinition<Bus> = {
  title: 'Buses',
  singular: 'bus',
  description: 'Administra la flota, su capacidad y disponibilidad.',
  searchPlaceholder: 'Buscar por identificación o modelo',
  list: api.buses.list,
  create: (form) => api.buses.create({
    numeroIdentificacion: inputValue(form, 'numeroIdentificacion').trim(),
    modelo: inputValue(form, 'modelo').trim(),
    capacidad: Number(form.capacidad),
    estado: parseBusState(form.estado),
  }),
  update: (id, form) => api.buses.update(id, {
    numeroIdentificacion: inputValue(form, 'numeroIdentificacion').trim(),
    modelo: inputValue(form, 'modelo').trim(),
    capacidad: Number(form.capacidad),
    estado: parseBusState(form.estado),
  }),
  remove: api.buses.remove,
  toForm: (bus) => bus ? {
    numeroIdentificacion: bus.numeroIdentificacion,
    modelo: bus.modelo,
    capacidad: String(bus.capacidad),
    estado: String(bus.estado),
  } : { numeroIdentificacion: '', modelo: '', capacidad: '', estado: String(EstadoBus.Disponible) },
  fields: [
    { name: 'numeroIdentificacion', label: 'Número de identificación', required: true, maxLength: 30, placeholder: 'Ej. BUS-042' },
    { name: 'modelo', label: 'Modelo', required: true, maxLength: 80, placeholder: 'Ej. Mercedes-Benz Sprinter' },
    { name: 'capacidad', label: 'Capacidad de pasajeros', type: 'number', required: true, min: 1 },
    { name: 'estado', label: 'Estado de la unidad', type: 'select', required: true, options: busStates },
  ],
  columns: [
    { title: 'Identificación', render: (bus) => <strong className="table-primary">{bus.numeroIdentificacion}</strong> },
    { title: 'Modelo', render: (bus) => bus.modelo },
    { title: 'Capacidad', render: (bus) => <span>{bus.capacidad} <small>pasajeros</small></span> },
    { title: 'Estado', render: (bus) => busState(bus.estado) },
  ],
  searchableText: (bus) => `${bus.numeroIdentificacion} ${bus.modelo} ${bus.estado}`,
}

export const conductoresDefinition: CrudDefinition<Conductor> = {
  title: 'Conductores',
  singular: 'conductor',
  description: 'Mantén al día las licencias y el estado de cada conductor.',
  searchPlaceholder: 'Buscar por nombre o licencia',
  list: api.conductores.list,
  create: (form) => api.conductores.create({
    nombre: inputValue(form, 'nombre').trim(),
    numeroLicencia: inputValue(form, 'numeroLicencia').trim(),
    estado: parseDriverState(form.estado),
    usuarioId: form.usuarioId ? Number(form.usuarioId) : null,
  }),
  update: (id, form) => api.conductores.update(id, {
    nombre: inputValue(form, 'nombre').trim(),
    numeroLicencia: inputValue(form, 'numeroLicencia').trim(),
    estado: parseDriverState(form.estado),
    usuarioId: form.usuarioId ? Number(form.usuarioId) : null,
  }),
  remove: api.conductores.remove,
  toForm: (driver) => driver ? {
    nombre: driver.nombre,
    numeroLicencia: driver.numeroLicencia,
    estado: String(driver.estado),
    usuarioId: driver.usuarioId ? String(driver.usuarioId) : '',
  } : { nombre: '', numeroLicencia: '', estado: String(EstadoConductor.Disponible), usuarioId: '' },
  fields: [
    { name: 'nombre', label: 'Nombre completo', required: true, maxLength: 100, placeholder: 'Ej. Mariana López' },
    { name: 'numeroLicencia', label: 'Número de licencia', required: true, maxLength: 30, placeholder: 'Ej. LIC-90214' },
    { name: 'estado', label: 'Estado del conductor', type: 'select', required: true, options: driverStates },
    { name: 'usuarioId', label: 'ID de usuario asociado (opcional)', type: 'number', min: 1, placeholder: 'Dejar vacío si no tiene cuenta' },
  ],
  columns: [
    { title: 'Conductor', render: (driver) => <div className="table-person"><span className="person-avatar">{driver.nombre.slice(0, 1).toUpperCase()}</span><strong className="table-primary">{driver.nombre}</strong></div> },
    { title: 'Licencia', render: (driver) => driver.numeroLicencia },
    { title: 'Usuario vinculado', render: (driver) => driver.usuarioId ? `#${driver.usuarioId}` : <span className="text-muted">Sin cuenta</span> },
    { title: 'Estado', render: (driver) => driverState(driver.estado) },
  ],
  searchableText: (driver) => `${driver.nombre} ${driver.numeroLicencia} ${driver.estado}`,
}

export const usuariosDefinition: CrudDefinition<Usuario> = {
  title: 'Usuarios',
  singular: 'usuario',
  description: 'Gestiona las cuentas del sistema y el rol asignado.',
  searchPlaceholder: 'Buscar por nombre de usuario',
  list: api.usuarios.list,
  create: (form) => api.usuarios.create({
    nombreUsuario: inputValue(form, 'nombreUsuario').trim(),
    password: inputValue(form, 'password'),
    rolId: Number(form.rolId),
  }),
  update: (id, form) => api.usuarios.update(id, {
    nombreUsuario: inputValue(form, 'nombreUsuario').trim(),
    password: inputValue(form, 'password') || null,
    activo: form.activo === 'true',
    rolId: Number(form.rolId),
  }),
  remove: api.usuarios.remove,
  toForm: (user) => user ? {
    nombreUsuario: user.nombreUsuario,
    password: '',
    rolId: String(user.rolId),
    activo: String(user.activo),
  } : { nombreUsuario: '', password: '', rolId: '1', activo: 'true' },
  fields: [
    { name: 'nombreUsuario', label: 'Nombre de usuario', required: true, maxLength: 50, placeholder: 'Ej. mlopez' },
    { name: 'password', label: 'Contraseña', type: 'password', required: true, minLength: 8, helpText: 'Al editar, déjala vacía para conservar la contraseña actual.' },
    { name: 'rolId', label: 'Rol', type: 'select', required: true, options: [{ value: '1', label: 'Administrador' }, { value: '2', label: 'Conductor' }] },
    { name: 'activo', label: 'Cuenta activa', type: 'checkbox' },
  ],
  columns: [
    { title: 'Usuario', render: (user) => <strong className="table-primary">{user.nombreUsuario}</strong> },
    { title: 'Rol', render: (user) => <span className="role-chip">{user.rolId === 1 ? 'Administrador' : user.rolId === 2 ? 'Conductor' : `Rol #${user.rolId}`}</span> },
    { title: 'Estado', render: (user) => statePill(user.activo ? 'Activo' : 'Inactivo', user.activo ? 'good' : 'muted') },
  ],
  searchableText: (user) => `${user.nombreUsuario} ${user.rolId} ${user.activo}`,
}

export const rolesDefinition: CrudDefinition<Rol> = {
  title: 'Roles',
  singular: 'rol',
  description: 'Consulta y administra los perfiles de acceso disponibles.',
  searchPlaceholder: 'Buscar por nombre del rol',
  list: api.roles.list,
  create: (form) => api.roles.create({ nombre: inputValue(form, 'nombre').trim() }),
  update: (id, form) => api.roles.update(id, { nombre: inputValue(form, 'nombre').trim() }),
  remove: api.roles.remove,
  toForm: (role) => ({ nombre: role?.nombre ?? '' }),
  fields: [{ name: 'nombre', label: 'Nombre del rol', required: true, maxLength: 30, placeholder: 'Ej. Supervisor' }],
  columns: [
    { title: 'Nombre del rol', render: (role) => <strong className="table-primary">{role.nombre}</strong> },
    { title: 'Identificador', render: (role) => <span className="id-chip">#{role.id}</span> },
  ],
  searchableText: (role) => `${role.nombre} ${role.id}`,
}
