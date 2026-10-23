export const EstadoBus = {
  Disponible: 0,
  Asignado: 1,
  EnMantenimiento: 2,
  FueraDeServicio: 3,
} as const

export type EstadoBus = typeof EstadoBus[keyof typeof EstadoBus]

export const EstadoConductor = {
  Disponible: 0,
  Asignado: 1,
  Descanso: 2,
  Inactivo: 3,
} as const

export type EstadoConductor = typeof EstadoConductor[keyof typeof EstadoConductor]

export const DiaSemana = {
  Lunes: 1,
  Martes: 2,
  Miercoles: 3,
  Jueves: 4,
  Viernes: 5,
  Sabado: 6,
  Domingo: 7,
} as const

export type DiaSemana = typeof DiaSemana[keyof typeof DiaSemana]

export interface Bus {
  id: number
  numeroIdentificacion: string
  modelo: string
  capacidad: number
  estado: EstadoBus
}

export interface Conductor {
  id: number
  nombre: string
  numeroLicencia: string
  estado: EstadoConductor
  usuarioId: number | null
}

export interface Programacion {
  id: number
  busId: number
  conductorId: number
  diaSemana: DiaSemana
  horaEntrada: string
  horaSalida: string
  activa: boolean
  fechaCreacion: string
}

export interface ProgramacionActivaBusDia {
  busId: number
  diaSemana: DiaSemana
}

export interface Usuario {
  id: number
  nombreUsuario: string
  activo: boolean
  rolId: number
}

export interface Rol {
  id: number
  nombre: string
}

export interface IniciarSesionDto {
  nombreUsuario: string
  password: string
}

export interface SesionDto {
  accessToken: string
  expiresAt: string
  userId: number
  nombreUsuario: string
  rolId: number
  rol: 'Administrador' | 'Conductor'
}

export interface CrearBusDto {
  numeroIdentificacion: string
  modelo: string
  capacidad: number
  estado: EstadoBus
}

export interface CrearConductorDto {
  nombre: string
  numeroLicencia: string
  estado: EstadoConductor
  usuarioId: number | null
}

export interface CrearProgramacionDto {
  busId: number
  conductorId: number
  diaSemana: DiaSemana
  horaEntrada: string
  horaSalida: string
  activa: boolean
}

export interface CrearProgramacionActivaBusDiaDto {
  busId: number
  diaSemana: DiaSemana
}

export interface CrearUsuarioDto {
  nombreUsuario: string
  password: string
  rolId: number
}

export interface ActualizarUsuarioDto {
  nombreUsuario: string
  password: string | null
  activo: boolean
  rolId: number
}

export const diasSemana = [
  { value: DiaSemana.Lunes, label: 'Lunes' },
  { value: DiaSemana.Martes, label: 'Martes' },
  { value: DiaSemana.Miercoles, label: 'Miércoles' },
  { value: DiaSemana.Jueves, label: 'Jueves' },
  { value: DiaSemana.Viernes, label: 'Viernes' },
  { value: DiaSemana.Sabado, label: 'Sábado' },
  { value: DiaSemana.Domingo, label: 'Domingo' },
]

export function nombreDia(dia: number): string {
  return diasSemana.find((item) => item.value === dia)?.label ?? 'Día no válido'
}
