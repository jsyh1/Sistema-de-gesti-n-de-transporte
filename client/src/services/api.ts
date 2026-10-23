import axios from 'axios'
import type {
  ActualizarUsuarioDto,
  Bus,
  Conductor,
  CrearBusDto,
  CrearConductorDto,
  CrearProgramacionActivaBusDiaDto,
  CrearProgramacionDto,
  CrearUsuarioDto,
  DiaSemana,
  IniciarSesionDto,
  Programacion,
  ProgramacionActivaBusDia,
  Rol,
  SesionDto,
  Usuario,
} from '../types/api'
import { clearSession, getAccessToken } from './authSession'

const client = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || '/api',
  headers: { 'Content-Type': 'application/json' },
})

client.interceptors.request.use((config) => {
  const token = getAccessToken()
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

client.interceptors.response.use(
  (response) => response,
  (error: unknown) => {
    if (axios.isAxiosError(error) && error.response?.status === 401) {
      clearSession()
      window.dispatchEvent(new Event('ruta24-session-expired'))
    }
    return Promise.reject(error)
  },
)

const resource = <T>(path: string) => ({
  list: async () => (await client.get<T[]>(path)).data,
  get: async (id: number) => (await client.get<T>(`${path}/${id}`)).data,
  create: async (body: unknown) => (await client.post<T>(path, body)).data,
  update: async (id: number, body: unknown) => (await client.put<T>(`${path}/${id}`, body)).data,
  remove: async (id: number) => { await client.delete(`${path}/${id}`) },
})

export const api = {
  auth: {
    login: async (body: IniciarSesionDto) => (await client.post<SesionDto>('/Autenticacion/login', body)).data,
  },
  buses: {
    ...resource<Bus>('/Buses'),
    create: async (body: CrearBusDto) => (await client.post<Bus>('/Buses', body)).data,
    update: async (id: number, body: CrearBusDto) => (await client.put<Bus>(`/Buses/${id}`, body)).data,
  },
  conductores: {
    ...resource<Conductor>('/Conductores'),
    create: async (body: CrearConductorDto) => (await client.post<Conductor>('/Conductores', body)).data,
    update: async (id: number, body: CrearConductorDto) => (await client.put<Conductor>(`/Conductores/${id}`, body)).data,
  },
  programaciones: {
    ...resource<Programacion>('/Programaciones'),
    mine: async () => (await client.get<Programacion[]>('/Programaciones/mias')).data,
    create: async (body: CrearProgramacionDto) => (await client.post<Programacion>('/Programaciones', body)).data,
    update: async (id: number, body: CrearProgramacionDto) => (await client.put<Programacion>(`/Programaciones/${id}`, body)).data,
  },
  programacionesActivas: {
    list: async () => (await client.get<ProgramacionActivaBusDia[]>('/ProgramacionesActivasBusDia')).data,
    create: async (body: CrearProgramacionActivaBusDiaDto) => (await client.post<ProgramacionActivaBusDia>('/ProgramacionesActivasBusDia', body)).data,
    update: async (busId: number, dia: DiaSemana, body: CrearProgramacionActivaBusDiaDto) =>
      (await client.put<ProgramacionActivaBusDia>(`/ProgramacionesActivasBusDia/${busId}/${dia}`, body)).data,
    remove: async (busId: number, dia: DiaSemana) => {
      await client.delete(`/ProgramacionesActivasBusDia/${busId}/${dia}`)
    },
  },
  usuarios: {
    ...resource<Usuario>('/Usuarios'),
    create: async (body: CrearUsuarioDto) => (await client.post<Usuario>('/Usuarios', body)).data,
    update: async (id: number, body: ActualizarUsuarioDto) => (await client.put<Usuario>(`/Usuarios/${id}`, body)).data,
  },
  roles: {
    ...resource<Rol>('/Roles'),
    create: async (body: { nombre: string }) => (await client.post<Rol>('/Roles', body)).data,
    update: async (id: number, body: { nombre: string }) => (await client.put<Rol>(`/Roles/${id}`, body)).data,
  },
}

export function mensajeError(error: unknown): string {
  if (axios.isAxiosError(error)) {
    const mensaje = error.response?.data?.mensaje ?? error.response?.data?.title
    if (typeof mensaje === 'string' && mensaje.trim()) return mensaje
    if (error.code === 'ERR_NETWORK') {
      return 'No se pudo conectar con la API. Verifica que el backend esté iniciado en http://localhost:5267.'
    }
    return `La API respondió con un error${error.response?.status ? ` (${error.response.status})` : ''}.`
  }
  return error instanceof Error ? error.message : 'Ocurrió un error inesperado.'
}
