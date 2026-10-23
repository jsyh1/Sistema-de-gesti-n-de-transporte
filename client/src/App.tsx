import { useEffect, useState } from 'react'
import { CrudPage } from './components/CrudPage'
import { DashboardPage } from './pages/DashboardPage'
import { DriverDashboardPage } from './pages/DriverDashboardPage'
import { LoginPage } from './pages/LoginPage'
import { ProgramacionesPage } from './pages/ProgramacionesPage'
import { busesDefinition, conductoresDefinition, rolesDefinition, usuariosDefinition } from './services/resourceDefinitions'
import { clearSession, readSession, saveSession } from './services/authSession'
import type { SesionDto } from './types/api'
import './App.css'

type Section = 'resumen' | 'buses' | 'conductores' | 'horarios' | 'usuarios' | 'roles'
const currentDate = new Intl.DateTimeFormat('es-MX', { weekday: 'long', day: 'numeric', month: 'long' }).format(new Date())

const menu: { id: Section; label: string; icon: string; group: string }[] = [
  { id: 'resumen', label: 'Resumen', icon: 'grid', group: 'GENERAL' },
  { id: 'buses', label: 'Buses', icon: 'bus', group: 'OPERACIÓN' },
  { id: 'conductores', label: 'Conductores', icon: 'users', group: 'OPERACIÓN' },
  { id: 'horarios', label: 'Programación', icon: 'calendar', group: 'OPERACIÓN' },
  { id: 'usuarios', label: 'Usuarios', icon: 'user', group: 'ADMINISTRACIÓN' },
  { id: 'roles', label: 'Roles', icon: 'shield', group: 'ADMINISTRACIÓN' },
]

function Icon({ name, size = 20 }: { name: string; size?: number }) {
  const common = { width: size, height: size, viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', strokeWidth: 1.8, strokeLinecap: 'round' as const, strokeLinejoin: 'round' as const, 'aria-hidden': true as const }

  switch (name) {
    case 'bus':
      return <svg {...common}><path d="M5 17V5.8A2.8 2.8 0 0 1 7.8 3h8.4A2.8 2.8 0 0 1 19 5.8V17" /><path d="M5 10h14M8 17v2m8-2v2M4 17h16M7 7h.01M17 7h.01" /><circle cx="8" cy="17" r="1.5" /><circle cx="16" cy="17" r="1.5" /></svg>
    case 'users':
      return <svg {...common}><path d="M16 20v-1.5a4 4 0 0 0-4-4H7a4 4 0 0 0-4 4V20" /><circle cx="9.5" cy="7" r="4" /><path d="M17 11a4 4 0 0 0 0-8m4 17v-1.5a4 4 0 0 0-3-3.87" /></svg>
    case 'calendar':
      return <svg {...common}><rect x="3" y="5" width="18" height="16" rx="2" /><path d="M16 3v4M8 3v4M3 10h18m-13 4h3m3 0h2m-8 3h3" /></svg>
    case 'user':
      return <svg {...common}><circle cx="12" cy="8" r="4" /><path d="M4 21a8 8 0 0 1 16 0" /></svg>
    case 'shield':
      return <svg {...common}><path d="M12 22s8-4 8-11V5l-8-3-8 3v6c0 7 8 11 8 11Z" /><path d="m9 12 2 2 4-4" /></svg>
    case 'search':
      return <svg {...common}><circle cx="10.8" cy="10.8" r="6.8" /><path d="m16 16 5 5" /></svg>
    case 'plus':
      return <svg {...common}><path d="M12 5v14M5 12h14" /></svg>
    case 'edit':
      return <svg {...common}><path d="m15 5 4 4M4 20l4.2-.9L19 8.3a2.1 2.1 0 0 0-3-3L5.2 16.1 4 20Z" /></svg>
    case 'trash':
      return <svg {...common}><path d="M4 7h16m-10 4v6m4-6v6M6 7l1 14h10l1-14M9 7V4h6v3" /></svg>
    case 'close':
      return <svg {...common}><path d="m6 6 12 12M18 6 6 18" /></svg>
    case 'arrow':
      return <svg {...common}><path d="M5 12h14m-6-6 6 6-6 6" /></svg>
    case 'clock':
      return <svg {...common}><circle cx="12" cy="12" r="9" /><path d="M12 7v5l3 2" /></svg>
    case 'refresh':
      return <svg {...common}><path d="M20 7v5h-5M4 17v-5h5" /><path d="M5.5 9A7 7 0 0 1 18 6l2 2m-14 8-2 2m.5-3A7 7 0 0 0 19 15" /></svg>
    case 'menu':
      return <svg {...common}><path d="M4 6h16M4 12h16M4 18h16" /></svg>
    default:
      return <svg {...common}><rect x="3" y="3" width="8" height="8" rx="1.5" /><rect x="13" y="3" width="8" height="8" rx="1.5" /><rect x="3" y="13" width="8" height="8" rx="1.5" /><rect x="13" y="13" width="8" height="8" rx="1.5" /></svg>
  }
}

function App() {
  const [activeSection, setActiveSection] = useState<Section>('resumen')
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false)
  const [session, setSession] = useState<SesionDto | null>(readSession)
  const current = menu.find((item) => item.id === activeSection)

  useEffect(() => {
    const handleExpired = () => setSession(null)
    window.addEventListener('ruta24-session-expired', handleExpired)
    return () => window.removeEventListener('ruta24-session-expired', handleExpired)
  }, [])

  const iniciarSesion = (nextSession: SesionDto) => {
    saveSession(nextSession)
    setSession(nextSession)
    setActiveSection('resumen')
  }

  const cerrarSesion = () => {
    clearSession()
    setSession(null)
    setActiveSection('resumen')
  }

  if (!session) return <LoginPage onLogin={iniciarSesion} />

  if (session.rolId === 2) {
    return (
      <main className="driver-shell">
        <header className="driver-topbar">
          <a className="brand" href="#" onClick={(event) => event.preventDefault()}>
            <span className="brand-mark"><Icon name="bus" size={23} /></span>
            <span className="brand-copy"><strong>RUTA<span>24</span></strong><small>GESTIÓN DE TRANSPORTE</small></span>
          </a>
          <div className="driver-account">
            <span>{session.nombreUsuario}</span>
            <button type="button" onClick={cerrarSesion}>Cerrar sesión</button>
          </div>
        </header>
        <DriverDashboardPage nombreUsuario={session.nombreUsuario} />
        <footer className="app-footer"><span>RUTA24 · Portal del conductor</span><span>Solo horarios asignados</span></footer>
      </main>
    )
  }

  if (session.rolId !== 1) return <LoginPage onLogin={iniciarSesion} />

  const navigate = (section: Section) => {
    setActiveSection(section)
    setMobileMenuOpen(false)
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  return (
    <div className="app-shell">
      <aside className={`sidebar ${mobileMenuOpen ? 'sidebar-open' : ''}`}>
        <a className="brand" href="#" onClick={(event) => { event.preventDefault(); navigate('resumen') }}>
          <span className="brand-mark"><Icon name="bus" size={23} /></span>
          <span className="brand-copy"><strong>RUTA<span>24</span></strong><small>GESTIÓN DE TRANSPORTE</small></span>
        </a>
        <div className="workspace-label">CENTRO DE OPERACIONES</div>
        {['GENERAL', 'OPERACIÓN', 'ADMINISTRACIÓN'].map((group) => (
          <nav className="nav-group" aria-label={group.toLowerCase()} key={group}>
            <p className="nav-heading">{group}</p>
            {menu.filter((item) => item.group === group).map((item) => (
              <button className={`nav-link ${activeSection === item.id ? 'nav-link-active' : ''}`} key={item.id} onClick={() => navigate(item.id)}>
                <Icon name={item.icon} size={19} />
                <span>{item.label}</span>
                {item.id === 'horarios' && <span className="nav-link-dot" />}
              </button>
            ))}
          </nav>
        ))}
        <div className="sidebar-bottom">
          <div className="service-indicator">API .NET · proxy local</div>
          <p>Gestión clara para cada recorrido.</p>
          <small>RUTA24 · SISTEMA DE TRANSPORTE</small>
        </div>
      </aside>

      <div className="main-area">
        <header className="topbar">
          <button className="icon-button menu-toggle" aria-label="Abrir menú" onClick={() => setMobileMenuOpen((open) => !open)}><Icon name="menu" /></button>
          <div className="breadcrumbs"><span>Operaciones</span><Icon name="arrow" size={14} /><strong>{current?.label}</strong></div>
          <div className="topbar-right"><span className="topbar-date">{currentDate}</span><span className="avatar">{session.nombreUsuario.slice(0, 1).toUpperCase()}</span><button className="logout-button" type="button" onClick={cerrarSesion}>Cerrar sesión</button></div>
        </header>
        <main className="page-content">
          {activeSection === 'resumen' && <DashboardPage onNavigate={navigate} />}
          {activeSection === 'buses' && <CrudPage definition={busesDefinition} />}
          {activeSection === 'conductores' && <CrudPage definition={conductoresDefinition} />}
          {activeSection === 'horarios' && <ProgramacionesPage />}
          {activeSection === 'usuarios' && <CrudPage definition={usuariosDefinition} />}
          {activeSection === 'roles' && <CrudPage definition={rolesDefinition} />}
        </main>
        <footer className="app-footer"><span>RUTA24 · Operación de transporte</span><span>Hecho para moverse mejor</span></footer>
      </div>
      {mobileMenuOpen && <button className="mobile-scrim" aria-label="Cerrar menú" onClick={() => setMobileMenuOpen(false)} />}
    </div>
  )
}

export default App
