import React, { createContext, useContext, useEffect, useState } from 'react'
import ReactDOM from 'react-dom/client'
import { BrowserRouter, Navigate, NavLink, Outlet, Route, Routes, useNavigate } from 'react-router-dom'
import axios from 'axios'
import {
  Activity, AlertTriangle, ArrowUpRight, BarChart3, CalendarDays, CheckCircle2,
  ChevronLeft, ChevronRight, ClipboardList, Clock3, FileBarChart, FileSpreadsheet, FileText,
  Layers, LayoutDashboard, LogIn, LogOut, Menu, Pencil, Plus, Power, PowerOff, RefreshCw,
  Search, Settings, ShieldCheck, Trash2, UserCheck, Users, X
} from 'lucide-react'
import './styles.css'

const api = axios.create({ baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5000/api' })
api.interceptors.request.use(config => {
  const token = localStorage.getItem('wf_token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

const AuthContext = createContext(null)
function useAuth() { return useContext(AuthContext) }

function AuthProvider({ children }) {
  const [user, setUser] = useState(() => JSON.parse(localStorage.getItem('wf_user') || 'null'))
  const login = async (email, password) => {
    const { data } = await api.post('/Auth/login', { email, password })
    localStorage.setItem('wf_token', data.data.token)
    localStorage.setItem('wf_user', JSON.stringify(data.data.usuario))
    setUser(data.data.usuario)
  }
  const logout = () => {
    localStorage.removeItem('wf_token')
    localStorage.removeItem('wf_user')
    setUser(null)
  }
  return <AuthContext.Provider value={{ user, login, logout }}>{children}</AuthContext.Provider>
}

function RequireAuth() {
  return localStorage.getItem('wf_token') ? <Outlet /> : <Navigate to="/login" replace />
}

const esAdmin = user => user?.rol === 'Administrador'
const esSupervisor = user => user?.rol === 'Supervisor'
const esEmpleado = user => user?.rol === 'Empleado'
const puedeGestionar = user => esAdmin(user) || esSupervisor(user)

const navigation = [
  { to: '/', label: 'Resumen', icon: LayoutDashboard, end: true },
  { to: '/empleados', label: 'Empleados', icon: Users, roles: ['Administrador', 'Supervisor'] },
  { to: '/horarios', label: 'Horarios', icon: CalendarDays },
  { to: '/asistencia', label: 'Asistencia', icon: Clock3 },
  { to: '/reportes', label: 'Reportes', icon: FileBarChart, roles: ['Administrador', 'Supervisor'] },
  { to: '/configuracion', label: 'Configuración', icon: Settings, roles: ['Administrador'] },
]

function navItems(rol) {
  return navigation.filter(item => !item.roles || item.roles.includes(rol))
}

function AppShell() {
  const { user, logout } = useAuth()
  const [open, setOpen] = useState(false)
  const [userOpen, setUserOpen] = useState(false)
  const cerrarSesion = () => {
    logout()
    setOpen(false)
    setUserOpen(false)
    window.location.replace('/login')
  }

  useEffect(() => {
    if (!userOpen) return
    const close = () => setUserOpen(false)
    window.addEventListener('click', close)
    return () => window.removeEventListener('click', close)
  }, [userOpen])

  return <div className="app-shell">
    <aside className={`sidebar ${open ? 'sidebar-open' : ''}`}>
      <div className="brand">
        <div className="brand-mark">S</div>
        <div><strong>SIGTA</strong><span>SISTEMA DE GESTIÓN DE TURNOS Y ASISTENCIA</span></div>
        <button className="icon-button mobile-close" onClick={() => setOpen(false)}><X size={18} /></button>
      </div>
      <div className="workspace-label">CONTROL CENTER</div>
      <nav>
        {navItems(user?.rol).map(({ to, label, icon: Icon, end }) =>
          <NavLink key={to} to={to} end={end} onClick={() => setOpen(false)}>
            <Icon size={18} /><span>{label}</span>{label === 'Resumen' && <span className="nav-pulse" />}
          </NavLink>)}
      </nav>
      <div className="sidebar-bottom">
        <div className="security-note"><ShieldCheck size={18} /><div><strong>Entorno seguro</strong><span>Sesión protegida</span></div></div>
        <button type="button" className="logout-button" onClick={cerrarSesion}><LogOut size={17} /> Cerrar sesión</button>
      </div>
    </aside>
    <main className="main-area">
      <header className="topbar">
        <button className="icon-button mobile-menu" onClick={() => setOpen(true)}><Menu size={21} /></button>
        <div className="breadcrumb"><span>Workspace</span><ChevronRight size={14} /><strong>Operaciones</strong></div>
        <div className="topbar-actions">
          <div className="live-status"><i /> Sistema operativo</div>
          <div className="user-menu">
            <button className="user-chip" onClick={e => { e.stopPropagation(); setUserOpen(!userOpen) }} type="button">
              <div className="avatar">{user?.nombreCompleto?.[0] || 'A'}</div>
              <span className="user-chip-info">
                <strong>{user?.nombreCompleto || 'Usuario'}</strong>
                <small>{user?.rol || ''}</small>
              </span>
              <ChevronRight size={14} className={`user-chevron ${userOpen ? 'open' : ''}`} />
            </button>
            {userOpen && <div className="user-dropdown">
              <div className="user-dropdown-head">
                <strong>{user?.nombreCompleto}</strong>
                <span>{user?.email}</span>
                <em className={`role-tag ${user?.rol?.toLowerCase()}`}>{user?.rol}</em>
              </div>
              <button type="button" className="user-dropdown-logout" onClick={cerrarSesion}><LogOut size={15} /> Cerrar sesión</button>
            </div>}
          </div>
        </div>
      </header>
      <div className="content"><Outlet /></div>
    </main>
  </div>
}

function Login() {
  const { user, login } = useAuth()
  const navigate = useNavigate()
  const [email, setEmail] = useState('admin@workforce.local')
  const [password, setPassword] = useState('Admin123!')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)
  useEffect(() => { if (user) navigate('/') }, [user, navigate])
  async function submit(e) {
    e.preventDefault(); setError(''); setLoading(true)
    try { await login(email, password); navigate('/') }
    catch (err) { setError(err.response?.data?.message || 'No pudimos validar tus credenciales.') }
    finally { setLoading(false) }
  }
  return <div className="login-page">
    <div className="login-art">
      <div className="login-grid" />
      <div className="login-copy">
        <div className="brand brand-light"><div className="brand-mark">S</div><div><strong>SIGTA</strong><span>SISTEMA DE GESTIÓN DE TURNOS Y ASISTENCIA</span></div></div>
        <div className="login-hero-copy">
          <p className="eyebrow">SISTEMA DE GESTIÓN DE TURNOS Y ASISTENCIA</p>
          <h1>Donde tus turnos se convierten en datos.</h1>
          <p>Planifica jornadas, registra asistencia y analiza el rendimiento de tu equipo en tiempo real.</p>
        </div>
      </div>
    </div>
    <div className="login-panel">
      <div className="login-form">
        <div className="mobile-login-logo brand"><div className="brand-mark">S</div><div><strong>SIGTA</strong><span>SISTEMA DE GESTIÓN DE TURNOS Y ASISTENCIA</span></div></div>
        <p className="eyebrow">BIENVENIDO DE VUELTA</p>
        <h2>Inicia tu jornada.</h2>
        <p className="muted">Accede al centro de control de tu organización.</p>
        <form onSubmit={submit}>
          <label>Correo electrónico<input type="email" value={email} onChange={e => setEmail(e.target.value)} required /></label>
          <label>Contraseña<div className="password-field"><input type="password" value={password} onChange={e => setPassword(e.target.value)} required /><span>•••</span></div></label>
          {error && <div className="form-error">{error}</div>}
          <button className="primary-button" disabled={loading}>{loading ? 'Validando...' : 'Entrar al workspace'}<ArrowUpRight size={17} /></button>
        </form>
        <p className="login-foot">SIGTA <span>•</span> Sistema de Gestión de Turnos y Asistencia</p>
      </div>
    </div>
  </div>
}

function PageHeader({ eyebrow, title, description, action }) {
  return <div className="page-header">
    <div><p className="eyebrow">{eyebrow}</p><h1>{title}</h1><p className="muted">{description}</p></div>
    {action}
  </div>
}

function Pagination({ data, onPageChange }) {
  const totalPages = data?.totalPages ?? 0
  const currentPage = data?.page ?? 1
  if (totalPages <= 1) return null

  const pages = Array.from({ length: totalPages }, (_, index) => index + 1)
  return <div className="pagination">
    <span className="pagination-summary">
      Página {currentPage} de {totalPages} · {data.totalRecords} registros
    </span>
    <div className="pagination-controls">
      <button className="icon-action" disabled={currentPage === 1} onClick={() => onPageChange(currentPage - 1)} aria-label="Página anterior">
        <ChevronLeft size={15} />
      </button>
      {pages.map(page => <button key={page} className={`page-number ${page === currentPage ? 'active' : ''}`} onClick={() => onPageChange(page)}>{page}</button>)}
      <button className="icon-action" disabled={currentPage === totalPages} onClick={() => onPageChange(currentPage + 1)} aria-label="Página siguiente">
        <ChevronRight size={15} />
      </button>
    </div>
  </div>
}

function trendLabel(fecha) {
  if (!fecha) return ''
  const d = new Date(fecha + 'T00:00:00')
  const hoy = new Date()
  if (d.getFullYear() === hoy.getFullYear() && d.getMonth() === hoy.getMonth()) {
    return d.toLocaleDateString('es-ES', { day: '2-digit' })
  }
  return d.toLocaleDateString('es-ES', { month: 'short' })
}

function Sparkline({ points, tone }) {
  const data = points.slice(-6)
  if (data.length < 2) return null
  const max = Math.max(...data, 1)
  const min = Math.min(...data, 0)
  const range = max - min || 1
  const width = 72, height = 26
  const step = width / (data.length - 1)
  const path = data.map((v, i) => `${i * step},${height - ((v - min) / range) * (height - 4) - 2}`).join(' ')
  return <svg className={`spark ${tone}`} viewBox={`0 0 ${width} ${height}`} preserveAspectRatio="none" aria-hidden="true">
    <polyline points={path} fill="none" strokeWidth="2" vectorEffect="non-scaling-stroke" />
  </svg>
}

function KpiCard({ label, value, unit, detail, icon: Icon, tone, spark }) {
  return <article className={`kpi-card ${tone}`}>
    <div className="kpi-top"><span>{label}</span><div className="kpi-icon"><Icon size={18} /></div></div>
    <div className="kpi-value"><strong>{value}</strong>{unit && <em>{unit}</em>}</div>
    <div className="kpi-foot">
      <span className="kpi-detail">{detail}</span>
      {spark && <Sparkline points={spark} tone={tone} />}
    </div>
  </article>
}

function Donut({ value }) {
  const pct = Math.max(0, Math.min(100, Number(value) || 0))
  const radius = 54
  const circumference = 2 * Math.PI * radius
  const offset = circumference * (1 - pct / 100)
  return <svg viewBox="0 0 140 140" className="donut-svg" role="img" aria-label={`Cumplimiento ${pct}%`}>
    <defs>
      <linearGradient id="donutGrad" x1="0" y1="0" x2="1" y2="1">
        <stop offset="0%" stopColor="#dff9a6" />
        <stop offset="45%" stopColor="#c9f36d" />
        <stop offset="100%" stopColor="#7fc53d" />
      </linearGradient>
    </defs>
    <circle className="donut-track" cx="70" cy="70" r={radius} />
    <circle className="donut-progress" cx="70" cy="70" r={radius}
      strokeDasharray={circumference} strokeDashoffset={offset}
      transform="rotate(-90 70 70)" />
    <text className="donut-value" x="70" y="71" textAnchor="middle">{pct}<tspan className="donut-unit">%</tspan></text>
    <text className="donut-label" x="70" y="91" textAnchor="middle">cumplimiento</text>
  </svg>
}

function Metric({ tone, icon: Icon, label, value, unit }) {
  return <div className="metric">
    <span className={`metric-icon ${tone}`}><Icon size={16} /></span>
    <span className="metric-text">{label}</span>
    <strong className="metric-value">{value}{unit && <em>{unit}</em>}</strong>
  </div>
}

function MiDashboard() {
  const { user } = useAuth()
  const [estado, setEstado] = useState(null)
  const [resumen, setResumen] = useState(null)
  const [error, setError] = useState('')

  useEffect(() => {
    const now = new Date()
    const inicio = new Date(now.getFullYear(), now.getMonth(), 1).toISOString().slice(0, 10)
    const fin = now.toISOString().slice(0, 10)
    Promise.all([
      api.get(`/Asistencia/estado-hoy/${user.empleadoId}`),
      api.get(`/Asistencia/resumen/${user.empleadoId}`, { params: { fechaInicio: inicio, fechaFin: fin } })
    ])
      .then(([e, r]) => { setEstado(e.data.data); setResumen(r.data.data) })
      .catch(() => setError('No pudimos cargar tu información. Verifica que la API esté ejecutándose.'))
  }, [user.empleadoId])

  const hour = new Date().getHours()
  const greeting = hour < 12 ? 'Buenos días' : hour < 19 ? 'Buenas tardes' : 'Buenas noches'
  const firstName = user?.nombreCompleto?.split(' ')?.[0] || 'Colaborador'
  const turnoInicio = horaDe(estado?.horaInicioProgramada)
  const turnoFin = horaDe(estado?.horaFinProgramada)

  return <>
    <PageHeader
      eyebrow={`ROL · ${user.rol.toUpperCase()}`}
      title={`${greeting}, ${firstName}.`}
      description="Tu jornada de hoy y tu resumen del mes."
    />

    {error && <div className="notice error">{error}</div>}

    <section className="panel jornada-panel">
      <div className="panel-heading">
        <div><p className="eyebrow">TU JORNADA DE HOY</p><h2>Estado del día</h2></div>
        <span className={`status-pill ${estado?.tieneEntradaActiva ? 'success' : 'muted'}`}><i /> {estado?.tieneEntradaActiva ? 'En curso' : estado?.yaRegistroHoy ? (estado?.estadoAsistencia ?? 'Registrada') : 'Sin registro'}</span>
      </div>
      <div className="jornada-body">
        <Donut value={resumen?.cumplimientoPromedio ?? 0} />
        <div className="jornada-metrics">
          <Metric tone="slate" icon={CalendarDays} label="Turno de hoy" value={estado?.tieneHorarioHoy ? `${turnoInicio ?? ''} - ${turnoFin ?? ''}` : 'Sin horario'} />
          <Metric tone="lime" icon={CheckCircle2} label="Días con registro" value={resumen?.diasConRegistro ?? 0} />
          <Metric tone="orange" icon={AlertTriangle} label="Tardanzas del mes" value={resumen?.tardanzas ?? 0} />
          <Metric tone="blue" icon={Activity} label="Horas del mes" value={resumen?.totalHorasTrabajadas ?? 0} unit="h" />
        </div>
      </div>
      <div className="jornada-footer">
        <div className="jornada-footer-head"><span>Entrada registrada</span><strong>{horaCorta(estado?.fechaHoraEntrada) ?? '—'}</strong></div>
        <div className="progress"><i style={{ width: `${Math.min(resumen?.cumplimientoPromedio ?? 0, 100)}%` }} /></div>
      </div>
    </section>

    <section className="panel area-panel">
      <div className="panel-heading">
        <div><p className="eyebrow">TU RESUMEN DEL MES</p><h2>Detalle de asistencia</h2></div>
        <Clock3 size={20} className="heading-icon" />
      </div>
      <div className="resumen-grid">
        <div className="resumen-item"><span>Presentes</span><strong>{resumen?.presentes ?? 0}</strong></div>
        <div className="resumen-item"><span>Tardanzas</span><strong>{resumen?.tardanzas ?? 0}</strong></div>
        <div className="resumen-item"><span>Ausencias justificadas</span><strong>{resumen?.ausenciasJustificadas ?? 0}</strong></div>
        <div className="resumen-item"><span>Horas trabajadas</span><strong>{resumen?.totalHorasTrabajadas ?? 0}h</strong></div>
        <div className="resumen-item"><span>Horas extra</span><strong className="success-text">{resumen?.totalHorasExtras ?? 0}h</strong></div>
        <div className="resumen-item"><span>Horas faltantes</span><strong className="warning-text">{resumen?.totalHorasFaltantes ?? 0}h</strong></div>
        <div className="resumen-item wide"><span>Cumplimiento promedio</span><div className="progress"><i style={{ width: `${Math.min(resumen?.cumplimientoPromedio ?? 0, 100)}%` }} /></div><strong>{resumen?.cumplimientoPromedio ?? 0}%</strong></div>
      </div>
    </section>
  </>
}

function Dashboard() {
  const { user } = useAuth()
  const [summary, setSummary] = useState(null)
  const [areas, setAreas] = useState([])
  const [trend, setTrend] = useState([])
  const [error, setError] = useState('')
  const [range, setRange] = useState('today')
  const [refreshing, setRefreshing] = useState(false)

  function dateRange(sel) {
    const now = new Date()
    const toISO = d => d.toISOString().slice(0, 10)
    const today = new Date(now.getFullYear(), now.getMonth(), now.getDate())
    if (sel === 'today') return { fechaInicio: toISO(today), fechaFin: toISO(today) }
    if (sel === 'month') {
      const first = new Date(now.getFullYear(), now.getMonth(), 1)
      return { fechaInicio: toISO(first), fechaFin: toISO(today) }
    }
    const day = (now.getDay() + 6) % 7
    const monday = new Date(today)
    monday.setDate(today.getDate() - day)
    return { fechaInicio: toISO(monday), fechaFin: toISO(today) }
  }

  async function loadDashboard() {
    setRefreshing(true)
    setError('')
    try {
      const rango = dateRange(range)
      const [s, a, t] = await Promise.all([
        api.get('/Dashboard/resumen', { params: rango }),
        api.get('/Dashboard/areas', { params: rango }),
        api.get('/Dashboard/tendencia', { params: rango })
      ])
      setSummary(s.data); setAreas(a.data); setTrend(t.data)
    } catch {
      setError('No pudimos cargar los indicadores. Verifica que la API esté ejecutándose.')
    } finally { setRefreshing(false) }
  }

  useEffect(() => {
    loadDashboard()
    const interval = window.setInterval(loadDashboard, 30000)
    return () => window.clearInterval(interval)
  }, [range])

  const now = new Date()
  const dateLabel = now.toLocaleDateString('es-ES', { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' }).toUpperCase()
  const hour = now.getHours()
  const greeting = hour < 12 ? 'Buenos días' : hour < 19 ? 'Buenas tardes' : 'Buenas noches'
  const firstName = user?.nombreCompleto?.split(' ')?.[0] || 'Administrador'

  const rangeLabel = { today: 'Hoy', week: 'Esta semana', month: 'Este mes' }[range]
  const maxHours = Math.max(...trend.map(x => x.horasTrabajadas), 1)
  const programados = summary?.horariosProgramados ?? 0
  const registradas = summary?.asistenciasRegistradas ?? 0
  const cobertura = programados ? Math.min(Math.round(registradas / programados * 100), 100) : 0

  const totales = areas.reduce((acc, row) => ({
    horarios: acc.horarios + row.horariosProgramados,
    asistencias: acc.asistencias + row.asistencias,
    tardanzas: acc.tardanzas + row.tardanzas,
    horas: acc.horas + row.horasTrabajadas,
  }), { horarios: 0, asistencias: 0, tardanzas: 0, horas: 0 })
  const cumplimientoPromedio = areas.length
    ? Math.round(areas.reduce((acc, row) => acc + row.cumplimiento, 0) / areas.length)
    : 0

  return <>
    <PageHeader
      eyebrow={dateLabel}
      title={`${greeting}, ${firstName}.`}
      description={`Resumen operativo de tu equipo · ${rangeLabel}`}
      action={
        <div className="dashboard-actions">
          <div className="segmented">
            {[{ id: 'today', label: 'Hoy' }, { id: 'week', label: 'Semana' }, { id: 'month', label: 'Mes' }].map(opt =>
              <button key={opt.id} className={range === opt.id ? 'active' : ''} onClick={() => setRange(opt.id)}>
                {opt.label}
              </button>)}
          </div>
          <button className="icon-action" onClick={loadDashboard} title="Actualizar indicadores" disabled={refreshing}>
            <RefreshCw size={16} className={refreshing ? 'spin' : ''} />
          </button>
        </div>
      }
    />

    {error && <div className="notice error">{error}</div>}

    <div className="kpi-grid">
      <KpiCard label="Empleados activos" value={summary?.empleadosActivos ?? '—'} detail="Plantilla habilitada" icon={Users} tone="violet" />
      <KpiCard label="Cumplimiento" value={summary?.cumplimiento ?? 0} unit="%" detail={`Del ${summary?.fechaInicio ?? '—'} al ${summary?.fechaFin ?? '—'}`} icon={CheckCircle2} tone="lime" />
      <KpiCard label="Horas trabajadas" value={summary?.horasTrabajadas ?? 0} unit="h" detail="En el período seleccionado" icon={Activity} tone="blue" spark={trend.map(t => t.horasTrabajadas)} />
      <KpiCard label="Tardanzas" value={summary?.tardanzas ?? 0} detail="Entradas fuera de tolerancia" icon={Clock3} tone="orange" spark={trend.map(t => t.tardanzas)} />
    </div>

    <div className="dash-grid">
      <section className="panel chart-panel">
        <div className="panel-heading">
          <div><p className="eyebrow">TENDENCIA</p><h2>Horas productivas</h2></div>
          <span className="chart-legend"><i /> Horas trabajadas</span>
        </div>
        <div className="bar-chart">
          {trend.length
            ? trend.map(item => <div className="bar-column" key={item.periodo} title={`${item.horasTrabajadas} horas · ${item.asistencias} asistencias`}>
                <div className="bar-value">{item.horasTrabajadas}h</div>
                <div className="bar-track"><div className="bar-fill" style={{ height: `${Math.max(item.horasTrabajadas / maxHours * 100, 4)}%` }} /></div>
                <span>{trendLabel(item.fecha)}</span>
              </div>)
            : <div className="empty-state">Aún no hay registros en este período.</div>}
        </div>
      </section>

      <section className="panel jornada-panel">
        <div className="panel-heading">
          <div><p className="eyebrow">OPERACIÓN · {rangeLabel.toUpperCase()}</p><h2>Estado de jornada</h2></div>
          <span className="panel-badge"><i /> En vivo</span>
        </div>
        <div className="jornada-body">
          <Donut value={summary?.cumplimiento ?? 0} />
          <div className="jornada-metrics">
            <Metric tone="lime" icon={CheckCircle2} label="Registradas" value={registradas} />
            <Metric tone="orange" icon={AlertTriangle} label="Tardanzas" value={summary?.tardanzas ?? 0} />
            <Metric tone="slate" icon={CalendarDays} label="Programadas" value={programados} />
            <Metric tone="blue" icon={Activity} label="Horas registradas" value={summary?.horasTrabajadas ?? 0} unit="h" />
          </div>
        </div>
        <div className="jornada-footer">
          <div className="jornada-footer-head"><span>Cobertura de registros</span><strong>{registradas}/{programados}</strong></div>
          <div className="progress"><i style={{ width: `${cobertura}%` }} /></div>
        </div>
      </section>
    </div>

    <section className="panel area-panel">
      <div className="panel-heading">
        <div><p className="eyebrow">LECTURA POR EQUIPO · {rangeLabel.toUpperCase()}</p><h2>Rendimiento por área</h2></div>
        <BarChart3 size={20} className="heading-icon" />
      </div>
      <div className="table-wrap">
        <table>
          <thead><tr><th>Área</th><th>Horarios</th><th>Asistencias</th><th>Tardanzas</th><th>Horas</th><th>Cumplimiento</th></tr></thead>
          <tbody>
            {areas.length
              ? areas.map(row => <tr key={row.area}>
                  <td><strong>{row.area}</strong></td>
                  <td>{row.horariosProgramados}</td>
                  <td>{row.asistencias}</td>
                  <td><span className={row.tardanzas ? 'warning-text' : 'success-text'}>{row.tardanzas}</span></td>
                  <td>{row.horasTrabajadas}h</td>
                  <td><div className="progress-cell"><div className="progress"><i style={{ width: `${row.cumplimiento}%` }} /></div><span>{row.cumplimiento}%</span></div></td>
                </tr>)
              : <tr><td colSpan="6" className="empty-cell">No hay datos para el período seleccionado.</td></tr>}
          </tbody>
          {areas.length > 0 && <tfoot>
            <tr>
              <td>Total</td><td>{totales.horarios}</td><td>{totales.asistencias}</td>
              <td>{totales.tardanzas}</td><td>{Math.round(totales.horas)}h</td><td>{cumplimientoPromedio}%</td>
            </tr>
          </tfoot>}
        </table>
      </div>
    </section>
  </>
}

function Modal({ title, subtitle, onClose, children }) {
  useEffect(() => {
    const onKey = e => { if (e.key === 'Escape') onClose() }
    window.addEventListener('keydown', onKey)
    document.body.style.overflow = 'hidden'
    return () => { window.removeEventListener('keydown', onKey); document.body.style.overflow = '' }
  }, [onClose])
  return <div className="modal-overlay" onMouseDown={e => { if (e.target === e.currentTarget) onClose() }}>
    <div className="modal" role="dialog" aria-modal="true" aria-label={title}>
      <div className="modal-head">
        <div><p className="eyebrow">{subtitle}</p><h2>{title}</h2></div>
        <button className="icon-button" onClick={onClose} aria-label="Cerrar"><X size={18} /></button>
      </div>
      <div className="modal-body">{children}</div>
    </div>
  </div>
}

const emptyEmployee = () => ({
  numeroDocumento: '', nombres: '', apellidos: '', areaId: '', cargoId: '',
  fechaIngreso: new Date().toISOString().slice(0, 10)
})

function EmployeeForm({ catalogs, employee, onClose, onSaved }) {
  const editing = Boolean(employee)
  const [form, setForm] = useState(() => editing ? {
    numeroDocumento: employee.numeroDocumento,
    nombres: employee.nombres,
    apellidos: employee.apellidos,
    areaId: employee.areaId,
    cargoId: employee.cargoId,
    fechaIngreso: employee.fechaIngreso
  } : emptyEmployee())
  const [error, setError] = useState('')
  const [saving, setSaving] = useState(false)
  const set = (key, value) => setForm(prev => ({ ...prev, [key]: value }))

  async function submit(e) {
    e.preventDefault(); setError(''); setSaving(true)
    const payload = {
      nombres: form.nombres.trim(),
      apellidos: form.apellidos.trim(),
      areaId: Number(form.areaId),
      cargoId: Number(form.cargoId),
      fechaIngreso: form.fechaIngreso
    }
    try {
      if (editing) await api.put(`/Empleados/${employee.empleadoId}`, payload)
      else await api.post('/Empleados', { ...payload, numeroDocumento: form.numeroDocumento.trim() })
      onSaved(editing ? 'Empleado actualizado correctamente.' : 'Empleado creado correctamente.')
    } catch (err) {
      const data = err.response?.data
      const detail = data?.errors?.length ? data.errors.join(' ') : data?.message
      setError(detail || 'No se pudo guardar el empleado.')
    } finally { setSaving(false) }
  }

  return <form className="form-grid" onSubmit={submit}>
    {!editing && <label className="span-2">Número de documento
      <input value={form.numeroDocumento} onChange={e => set('numeroDocumento', e.target.value)} required maxLength={20} placeholder="Ej. 1029384756" />
    </label>}
    <label>Nombres
      <input value={form.nombres} onChange={e => set('nombres', e.target.value)} required maxLength={100} placeholder="Ej. Laura" />
    </label>
    <label>Apellidos
      <input value={form.apellidos} onChange={e => set('apellidos', e.target.value)} required maxLength={100} placeholder="Ej. Gómez" />
    </label>
    <label>Área
      <select value={form.areaId} onChange={e => set('areaId', e.target.value)} required>
        <option value="">Selecciona un área</option>
        {catalogs.areas.map(a => <option key={a.id} value={a.id}>{a.nombre}</option>)}
      </select>
    </label>
    <label>Cargo
      <select value={form.cargoId} onChange={e => set('cargoId', e.target.value)} required>
        <option value="">Selecciona un cargo</option>
        {catalogs.cargos.map(c => <option key={c.id} value={c.id}>{c.nombre}</option>)}
      </select>
    </label>
    <label className="span-2">Fecha de ingreso
      <input type="date" value={form.fechaIngreso} onChange={e => set('fechaIngreso', e.target.value)} required />
    </label>
    {error && <div className="form-error span-2">{error}</div>}
    <div className="modal-actions span-2">
      <button type="button" className="ghost-button" onClick={onClose}>Cancelar</button>
      <button type="submit" className="primary-button compact" disabled={saving}>
        {saving ? 'Guardando...' : editing ? 'Guardar cambios' : 'Crear empleado'}
      </button>
    </div>
  </form>
}

function Employees() {
  const [data, setData] = useState(null)
  const [query, setQuery] = useState('')
  const [catalogs, setCatalogs] = useState({ areas: [], cargos: [] })
  const [modal, setModal] = useState(null)
  const [feedback, setFeedback] = useState(null)
  const [loading, setLoading] = useState(true)
  const [page, setPage] = useState(1)

  const load = () => {
    setLoading(true)
    api.get('/Empleados', { params: { termino: query, page, pageSize: 10 } })
      .then(r => setData(r.data))
      .catch(() => setFeedback({ type: 'error', message: 'No pudimos cargar los empleados.' }))
      .finally(() => setLoading(false))
  }
  useEffect(() => { load() }, [query, page])
  useEffect(() => {
    Promise.all([api.get('/Catalogos/areas'), api.get('/Catalogos/cargos')])
      .then(([a, c]) => setCatalogs({ areas: a.data, cargos: c.data }))
      .catch(() => setFeedback({ type: 'error', message: 'No pudimos cargar áreas y cargos.' }))
  }, [])

  async function toggle(employee) {
    try {
      const action = employee.activo ? 'desactivar' : 'activar'
      await api.patch(`/Empleados/${employee.empleadoId}/${action}`)
      setFeedback({ type: 'ok', message: employee.activo ? 'Empleado desactivado.' : 'Empleado activado.' })
      load()
    } catch (err) {
      setFeedback({ type: 'error', message: err.response?.data?.message || 'No se pudo cambiar el estado.' })
    }
  }

  function saved(message) {
    setModal(null)
    setFeedback({ type: 'ok', message })
    load()
  }

  const rows = data?.data || []
  return <>
    <PageHeader eyebrow="DIRECTORIO DE PERSONAS" title="Empleados" description="Conoce y administra el talento que mueve la operación." action={<button className="primary-button compact" onClick={() => setModal({ employee: null })}><Plus size={16} /> Nuevo empleado</button>} />
    {feedback && <div className={`notice ${feedback.type}`}>{feedback.message}</div>}
    <section className="panel list-panel">
      <div className="list-toolbar">
        <div className="search-box"><Search size={17} /><input placeholder="Buscar por nombre o documento" value={query} onChange={e => { setQuery(e.target.value); setPage(1) }} /></div>
        <span className="result-count">{data?.totalRecords ?? 0} registros</span>
      </div>
      <div className="table-wrap">
        <table>
          <thead><tr><th>Empleado</th><th>Documento</th><th>Área</th><th>Cargo</th><th>Estado</th><th>Acciones</th></tr></thead>
          <tbody>
            {rows.map(row => <tr key={row.empleadoId}>
              <td><div className="person-cell"><span>{row.nombres?.[0]}{row.apellidos?.[0]}</span><strong>{row.nombres} {row.apellidos}</strong></div></td>
              <td>{row.numeroDocumento}</td><td>{row.area}</td><td>{row.cargo}</td>
              <td><span className={`status-pill ${row.activo ? 'success' : 'muted'}`}><i /> {row.activo ? 'Activo' : 'Inactivo'}</span></td>
              <td><div className="row-actions">
                <button className="icon-action" title="Editar" onClick={() => setModal({ employee: row })}><Pencil size={15} /></button>
                <button className="icon-action" title={row.activo ? 'Desactivar' : 'Activar'} onClick={() => toggle(row)}>{row.activo ? <PowerOff size={15} /> : <Power size={15} />}</button>
              </div></td>
            </tr>)}
            {!rows.length && <tr><td colSpan="6" className="empty-cell">{loading ? 'Cargando empleados...' : 'No se encontraron empleados.'}</td></tr>}
          </tbody>
        </table>
      </div>
      <Pagination data={data} onPageChange={setPage} />
    </section>
    {modal && <Modal
      title={modal.employee ? 'Editar empleado' : 'Nuevo empleado'}
      subtitle="DIRECTORIO DE PERSONAS"
      onClose={() => setModal(null)}>
      <EmployeeForm catalogs={catalogs} employee={modal.employee} onClose={() => setModal(null)} onSaved={saved} />
    </Modal>}
  </>
}

function AsignarHorarioModal({ catalogs, onClose, onDone }) {
  const [selected, setSelected] = useState(null)
  const [tipoTurnoId, setTipoTurnoId] = useState('')
  const [fecha, setFecha] = useState(new Date().toISOString().slice(0, 10))
  const [observaciones, setObservaciones] = useState('')
  const [error, setError] = useState('')
  const [saving, setSaving] = useState(false)

  const turno = catalogs.turnos.find(t => String(t.id) === String(tipoTurnoId))

  async function submit(e) {
    e.preventDefault()
    if (!selected) { setError('Selecciona un empleado.'); return }
    if (!tipoTurnoId) { setError('Selecciona un tipo de turno.'); return }
    setError(''); setSaving(true)
    try {
      const { data } = await api.post('/Horarios', {
        empleadoId: selected.empleadoId,
        tipoTurnoId: Number(tipoTurnoId),
        fecha,
        observaciones: observaciones.trim() || null
      })
      onDone(data.message || 'Horario asignado correctamente.')
    } catch (err) {
      const data = err.response?.data
      const detail = data?.errors?.length ? data.errors.join(' ') : data?.message
      setError(detail || 'No se pudo asignar el horario.')
    } finally { setSaving(false) }
  }

  return <Modal title="Asignar horario" subtitle="PLANIFICACIÓN OPERATIVA" onClose={onClose}>
    <form className="form-grid" onSubmit={submit}>
      <div className="span-2">
        <label className="field-label">Empleado</label>
        {selected
          ? <div className="selected-employee">
              <span className="picker-avatar">{inicialesDe(selected)}</span>
              <span><strong>{nombreDe(selected)}</strong><small>{selected.area}{selected.cargo ? ` · ${selected.cargo}` : ''}</small></span>
              <button type="button" className="ghost-button tiny" onClick={() => setSelected(null)}>Cambiar</button>
            </div>
          : <EmployeePicker onPick={setSelected} />}
      </div>

      <label>Tipo de turno
        <select value={tipoTurnoId} onChange={e => setTipoTurnoId(e.target.value)} required>
          <option value="">Selecciona un turno</option>
          {catalogs.turnos.map(t => <option key={t.id} value={t.id}>{t.nombre} · {horaDe(t.horaInicio)}–{horaDe(t.horaFin)}</option>)}
        </select>
      </label>
      <label>Fecha
        <input type="date" value={fecha} onChange={e => setFecha(e.target.value)} required />
      </label>

      {turno && <div className="summary-box span-2">
        <div><span>Turno</span><strong>{turno.nombre}</strong></div>
        <div><span>Jornada</span><strong>{horaDe(turno.horaInicio)} – {horaDe(turno.horaFin)}</strong></div>
        <div><span>Horas esperadas</span><strong>{turno.horasEsperadas}h</strong></div>
      </div>}

      <label className="span-2">Observaciones (opcional)
        <textarea value={observaciones} onChange={e => setObservaciones(e.target.value)} rows={2} placeholder="Notas sobre la asignación" />
      </label>

      {error && <div className="form-error span-2">{error}</div>}
      <div className="modal-actions span-2">
        <button type="button" className="ghost-button" onClick={onClose}>Cancelar</button>
        <button type="submit" className="primary-button compact" disabled={saving}>{saving ? 'Asignando...' : 'Asignar horario'}</button>
      </div>
    </form>
  </Modal>
}

function AsignacionMasivaModal({ catalogs, onClose, onDone }) {
  const [areaId, setAreaId] = useState('')
  const [tipoTurnoId, setTipoTurnoId] = useState('')
  const [fechaInicio, setFechaInicio] = useState(new Date().toISOString().slice(0, 10))
  const [fechaFin, setFechaFin] = useState(new Date().toISOString().slice(0, 10))
  const [sobreescribir, setSobre] = useState(false)
  const [error, setError] = useState('')
  const [saving, setSaving] = useState(false)

  const turno = catalogs.turnos.find(t => String(t.id) === String(tipoTurnoId))

  async function submit(e) {
    e.preventDefault()
    if (!areaId || !tipoTurnoId) { setError('Selecciona área y turno.'); return }
    if (fechaFin < fechaInicio) { setError('La fecha fin no puede ser menor a la fecha inicio.'); return }
    setError(''); setSaving(true)
    try {
      const { data } = await api.post('/Horarios/asignacion-masiva', {
        areaId: Number(areaId),
        tipoTurnoId: Number(tipoTurnoId),
        fechaInicio,
        fechaFin,
        sobreescribirExistentes: sobreescribir
      })
      onDone(data.message || `Se asignaron ${data.data?.horariosAsignados ?? 0} horarios.`)
    } catch (err) {
      const data = err.response?.data
      setError(data?.message || 'No se pudo realizar la asignación masiva.')
    } finally { setSaving(false) }
  }

  return <Modal title="Asignación masiva" subtitle="PLANIFICACIÓN OPERATIVA" onClose={onClose}>
    <form className="form-grid" onSubmit={submit}>
      <label className="span-2">Área
        <select value={areaId} onChange={e => setAreaId(e.target.value)} required>
          <option value="">Selecciona un área</option>
          {catalogs.areas.map(a => <option key={a.id} value={a.id}>{a.nombre}</option>)}
        </select>
      </label>
      <label className="span-2">Tipo de turno
        <select value={tipoTurnoId} onChange={e => setTipoTurnoId(e.target.value)} required>
          <option value="">Selecciona un turno</option>
          {catalogs.turnos.map(t => <option key={t.id} value={t.id}>{t.nombre} · {horaDe(t.horaInicio)}–{horaDe(t.horaFin)}</option>)}
        </select>
      </label>
      <label>Desde<input type="date" value={fechaInicio} max={fechaFin} onChange={e => setFechaInicio(e.target.value)} required /></label>
      <label>Hasta<input type="date" value={fechaFin} min={fechaInicio} onChange={e => setFechaFin(e.target.value)} required /></label>

      {turno && <div className="summary-box span-2">
        <div><span>Turno</span><strong>{turno.nombre}</strong></div>
        <div><span>Jornada</span><strong>{horaDe(turno.horaInicio)} – {horaDe(turno.horaFin)}</strong></div>
        <div><span>Horas esperadas</span><strong>{turno.horasEsperadas}h</strong></div>
      </div>}

      <label className="checkbox-field span-2">
        <input type="checkbox" checked={sobreescribir} onChange={e => setSobre(e.target.checked)} />
        <span><strong>Sobrescribir horarios existentes</strong><small>Reemplaza turnos ya asignados en el rango y evita duplicados.</small></span>
      </label>

      <p className="form-hint span-2">Se asignará el turno a todos los empleados activos del área, de lunes a viernes, en el rango seleccionado.</p>

      {error && <div className="form-error span-2">{error}</div>}
      <div className="modal-actions span-2">
        <button type="button" className="ghost-button" onClick={onClose}>Cancelar</button>
        <button type="submit" className="primary-button compact" disabled={saving}>{saving ? 'Procesando...' : 'Realizar asignación'}</button>
      </div>
    </form>
  </Modal>
}

const isoDate = d => { const x = new Date(d); x.setMinutes(x.getMinutes() - x.getTimezoneOffset()); return x.toISOString().slice(0, 10) }
const startOfWeek = d => { const x = new Date(d); const day = (x.getDay() + 6) % 7; x.setDate(x.getDate() - day); x.setHours(0, 0, 0, 0); return x }
const addDays = (d, n) => { const x = new Date(d); x.setDate(x.getDate() + n); return x }
const MESES = ['enero', 'febrero', 'marzo', 'abril', 'mayo', 'junio', 'julio', 'agosto', 'septiembre', 'octubre', 'noviembre', 'diciembre']
const DIAS = ['Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb', 'Dom']
const TONOS = ['var(--accent)', 'var(--warning)', 'var(--success)']

function ScheduleCalendar({ rows, anchor, vista, turnos, onPrev, onNext, onToday }) {
  const tone = nombre => { const i = turnos.findIndex(t => t.nombre === nombre); return i < 0 ? 0 : i % 3 }

  const inicio = vista === 'semana'
    ? startOfWeek(anchor)
    : startOfWeek(new Date(anchor.getFullYear(), anchor.getMonth(), 1))
  const total = vista === 'semana' ? 7 : 42
  const days = Array.from({ length: total }, (_, i) => addDays(inicio, i))

  const byDay = {}
  rows.forEach(r => { (byDay[r.fecha] ||= []).push(r) })

  const finSemana = addDays(startOfWeek(anchor), 6)
  const title = vista === 'semana'
    ? `${startOfWeek(anchor).getDate()} ${MESES[startOfWeek(anchor).getMonth()].slice(0, 3)} – ${finSemana.getDate()} ${MESES[finSemana.getMonth()].slice(0, 3)} ${finSemana.getFullYear()}`
    : `${MESES[anchor.getMonth()]} ${anchor.getFullYear()}`

  const maxChips = vista === 'semana' ? 99 : 3
  const hoyKey = isoDate(new Date())

  return <section className="panel calendar">
    <div className="cal-toolbar">
      <div className="cal-title"><h2>{title}</h2></div>
      <div className="cal-nav">
        <button className="icon-action" onClick={onPrev} title="Anterior"><ChevronLeft size={16} /></button>
        <button className="ghost-button tiny" onClick={onToday}>Hoy</button>
        <button className="icon-action" onClick={onNext} title="Siguiente"><ChevronRight size={16} /></button>
      </div>
    </div>
    {turnos.length > 0 && <div className="cal-legend">
      {turnos.map((t, i) => <span key={t.id}><i style={{ background: TONOS[i % 3] }} /> {t.nombre}</span>)}
    </div>}
    <div className="cal-dow">{DIAS.map(d => <span key={d}>{d}</span>)}</div>
    <div className={`cal-grid ${vista}`}>
      {days.map((d, i) => {
        const key = isoDate(d)
        const items = byDay[key] || []
        const fuera = vista === 'mes' && d.getMonth() !== anchor.getMonth()
        return <div key={i} className={`cal-cell ${fuera ? 'out' : ''} ${key === hoyKey ? 'today' : ''}`}>
          <div className="cal-date"><span>{d.getDate()}</span>{items.length > 0 && <span className="cal-more">{items.length}</span>}</div>
          <div className="cal-entries">
            {items.slice(0, maxChips).map(r => <div className={`cal-chip t${tone(r.tipoTurno)}`} key={r.horarioId}
              title={`${r.nombreCompleto} · ${r.tipoTurno} · ${horaDe(r.horaInicioProgramada)}–${horaDe(r.horaFinProgramada)}`}>
              <span className="cal-chip-avatar">{inicialesDe({ nombreCompleto: r.nombreCompleto })}</span>
              <span className="cal-chip-name">{r.nombreCompleto.split(' ')[0]}</span>
              <span className="cal-chip-time">{horaDe(r.horaInicioProgramada)}</span>
            </div>)}
            {items.length > maxChips && <span className="cal-more">+{items.length - maxChips} más</span>}
            {!items.length && vista === 'semana' && <span className="cal-empty">Sin horarios</span>}
          </div>
        </div>
      })}
    </div>
  </section>
}

function Schedules() {
  const { user } = useAuth()
  const hoy = new Date()
  const primerDia = isoDate(new Date(hoy.getFullYear(), hoy.getMonth(), 1))
  const [vista, setVista] = useState('lista')
  const [anchor, setAnchor] = useState(new Date())
  const [fechaInicio, setFechaInicio] = useState(primerDia)
  const [fechaFin, setFechaFin] = useState(isoDate(hoy))
  const [areaId, setAreaId] = useState('')
  const [data, setData] = useState(null)
  const [catalogs, setCatalogs] = useState({ areas: [], turnos: [] })
  const [feedback, setFeedback] = useState(null)
  const [modal, setModal] = useState(null)
  const [page, setPage] = useState(1)

  const rangoCalendario = () => {
    if (vista === 'semana') {
      const s = startOfWeek(anchor)
      return { fechaInicio: isoDate(s), fechaFin: isoDate(addDays(s, 6)) }
    }
    const s = startOfWeek(new Date(anchor.getFullYear(), anchor.getMonth(), 1))
    return { fechaInicio: isoDate(s), fechaFin: isoDate(addDays(s, 41)) }
  }

  const load = () => {
    const p = vista === 'lista' ? { fechaInicio, fechaFin } : rangoCalendario()
    api.get('/Horarios', { params: { ...p, areaId: areaId || undefined, page: vista === 'lista' ? page : 1, pageSize: vista === 'lista' ? 10 : 500 } })
      .then(r => setData(r.data))
      .catch(() => setFeedback({ type: 'error', message: 'No pudimos cargar los horarios.' }))
  }
  useEffect(() => { load() }, [vista, anchor, fechaInicio, fechaFin, areaId, page])
  useEffect(() => {
    Promise.all([api.get('/Catalogos/areas'), api.get('/Catalogos/tipos-turno')])
      .then(([a, t]) => setCatalogs({ areas: a.data, turnos: t.data }))
      .catch(() => setFeedback({ type: 'error', message: 'No pudimos cargar áreas y turnos.' }))
  }, [])

  function done(message) { setModal(null); setFeedback({ type: 'ok', message }); load() }

  async function eliminar(row) {
    if (!window.confirm(`¿Eliminar el horario de ${row.nombreCompleto} del ${row.fecha}?`)) return
    try {
      await api.delete(`/Horarios/${row.horarioId}`)
      setFeedback({ type: 'ok', message: 'Horario eliminado correctamente.' })
      load()
    } catch (err) {
      setFeedback({ type: 'error', message: err.response?.data?.message || 'No se pudo eliminar el horario.' })
    }
  }

  const shift = n => setAnchor(v => vista === 'mes' ? new Date(v.getFullYear(), v.getMonth() + n, 1) : addDays(v, n * 7))

  const rows = data?.data || []
  const esRolEmpleado = esEmpleado(user)
  const puedeAsignar = puedeGestionar(user)
  const esSoloAdmin = esAdmin(user)

  return <>
    <PageHeader
      eyebrow="PLANIFICACIÓN OPERATIVA"
      title={esRolEmpleado ? 'Mis horarios' : 'Horarios'}
      description={esRolEmpleado
        ? 'Consulta los turnos que tienes asignados.'
        : 'Diseña jornadas que se ajustan al ritmo de cada equipo.'}
      action={puedeAsignar && <div className="report-actions">
        <button className="ghost-button" onClick={() => setModal('masiva')}><Layers size={16} /> Asignación masiva</button>
        <button className="primary-button compact" onClick={() => setModal('individual')}><CalendarDays size={16} /> Asignar horario</button>
      </div>}
    />

    {feedback && <div className={`notice ${feedback.type}`}>{feedback.message}</div>}

    <section className="panel report-controls">
      <div className="segmented">
        {[{ id: 'lista', label: 'Lista' }, { id: 'semana', label: 'Semana' }, { id: 'mes', label: 'Mes' }].map(v =>
          <button key={v.id} className={vista === v.id ? 'active' : ''} onClick={() => { setVista(v.id); setPage(1) }}>{v.label}</button>)}
      </div>
      <div className="toolbar-filters">
        {vista === 'lista' && <>
          <label className="filter-field">Desde<input type="date" value={fechaInicio} max={fechaFin} onChange={e => { setFechaInicio(e.target.value); setPage(1) }} /></label>
          <label className="filter-field">Hasta<input type="date" value={fechaFin} min={fechaInicio} onChange={e => { setFechaFin(e.target.value); setPage(1) }} /></label>
        </>}
        <label className="filter-field">Área
          <select value={areaId} onChange={e => { setAreaId(e.target.value); setPage(1) }}>
            <option value="">Todas</option>
            {catalogs.areas.map(a => <option key={a.id} value={a.id}>{a.nombre}</option>)}
          </select>
        </label>
      </div>
    </section>

    {vista === 'lista'
      ? <section className="panel list-panel">
          <div className="list-toolbar">
            <span className="eyebrow">AGENDA DE TURNOS</span>
            <span className="result-count">{data?.totalRecords ?? 0} registros</span>
          </div>
          <div className="table-wrap">
            <table>
              <thead><tr><th>Empleado</th><th>Área</th><th>Fecha</th><th>Turno</th><th>Jornada</th><th>Horas</th><th>Asignado por</th>{esSoloAdmin && <th>Acciones</th>}</tr></thead>
              <tbody>
                {rows.map(row => <tr key={row.horarioId}>
                  <td><strong>{row.nombreCompleto}</strong></td>
                  <td>{row.area}</td>
                  <td>{row.fecha}</td>
                  <td><span className="turno-tag">{row.tipoTurno}</span></td>
                  <td>{horaDe(row.horaInicioProgramada)} – {horaDe(row.horaFinProgramada)}</td>
                  <td>{row.horasProgramadas}h</td>
                  <td>{row.asignadoPor}</td>
                  {esSoloAdmin && <td><div className="row-actions">
                    <button className="icon-action danger" title="Eliminar" onClick={() => eliminar(row)}><Trash2 size={15} /></button>
                  </div></td>}
                </tr>)}
                {!rows.length && <tr><td colSpan={esSoloAdmin ? 8 : 7} className="empty-cell">No hay horarios en el período seleccionado.</td></tr>}
              </tbody>
           </table>
          </div>
          <Pagination data={data} onPageChange={setPage} />
        </section>
      : <ScheduleCalendar
          rows={rows}
          anchor={anchor}
          vista={vista}
          turnos={catalogs.turnos}
          onPrev={() => shift(-1)}
          onNext={() => shift(1)}
          onToday={() => setAnchor(new Date())}
        />}

    {modal === 'individual' && <AsignarHorarioModal catalogs={catalogs} onClose={() => setModal(null)} onDone={done} />}
    {modal === 'masiva' && <AsignacionMasivaModal catalogs={catalogs} onClose={() => setModal(null)} onDone={done} />}
  </>
}

const nombreDe = e => e?.nombreCompleto || `${e?.nombres ?? ''} ${e?.apellidos ?? ''}`.trim()
const inicialesDe = e => e?.nombres
  ? `${e.nombres[0]}${e.apellidos?.[0] ?? ''}`
  : (e?.nombreCompleto?.split(' ').map(p => p[0]).slice(0, 2).join('') ?? '')
const horaCorta = dt => (dt ? dt.slice(11, 16) : null)
const horaDe = t => (t ? String(t).slice(0, 5) : null)

function EmployeePicker({ onPick, placeholder }) {
  const [termino, setTermino] = useState('')
  const [items, setItems] = useState([])
  const [open, setOpen] = useState(false)
  const [loading, setLoading] = useState(false)

  useEffect(() => {
    const t = setTimeout(() => {
      setLoading(true)
      api.get('/Empleados', { params: { termino, pageSize: 8, activo: true } })
        .then(r => setItems(r.data.data || []))
        .catch(() => setItems([]))
        .finally(() => setLoading(false))
    }, 250)
    return () => clearTimeout(t)
  }, [termino])

  return <div className="picker">
    <div className="search-box">
      <Search size={17} />
      <input
        placeholder={placeholder || 'Buscar por nombre o documento'}
        value={termino}
        onChange={e => { setTermino(e.target.value); setOpen(true) }}
        onFocus={() => setOpen(true)}
        onBlur={() => setTimeout(() => setOpen(false), 150)}
      />
    </div>
    {open && <div className="picker-results">
      {loading && <div className="picker-hint">Buscando...</div>}
      {!loading && !items.length && <div className="picker-hint">Sin resultados</div>}
      {items.map(e => <button key={e.empleadoId} type="button" className="picker-option" onMouseDown={() => onPick(e)}>
        <span className="picker-avatar">{inicialesDe(e)}</span>
        <span><strong>{e.nombres} {e.apellidos}</strong><small>{e.area} · {e.cargo}</small></span>
      </button>)}
    </div>}
  </div>
}

function RegistroAsistenciaModal({ mode, preselected, onClose, onDone }) {
  const esEntrada = mode === 'entrada'
  const [selected, setSelected] = useState(preselected || null)
  const [estado, setEstado] = useState(null)
  const [hora, setHora] = useState(() => {
    const d = new Date()
    d.setMinutes(d.getMinutes() - d.getTimezoneOffset())
    return d.toISOString().slice(0, 16)
  })
  const [error, setError] = useState('')
  const [saving, setSaving] = useState(false)

  useEffect(() => {
    if (!selected) { setEstado(null); return }
    api.get(`/Asistencia/estado-hoy/${selected.empleadoId}`)
      .then(r => setEstado(r.data.data))
      .catch(() => setEstado(null))
  }, [selected])

  const bloqueado = esEntrada ? estado?.yaRegistroHoy : (estado ? !estado.tieneEntradaActiva : false)

  async function submit(e) {
    e.preventDefault()
    if (!selected) { setError('Selecciona un empleado para continuar.'); return }
    setError(''); setSaving(true)
    const payload = { empleadoId: selected.empleadoId }
    payload[esEntrada ? 'fechaHoraEntrada' : 'fechaHoraSalida'] = hora.length === 16 ? `${hora}:00` : hora
    try {
      const { data } = await api.post(`/Asistencia/${mode}`, payload)
      onDone(data.message || 'Registro guardado correctamente.')
    } catch (err) {
      setError(err.response?.data?.message || 'No se pudo registrar la asistencia.')
    } finally { setSaving(false) }
  }

  return <Modal title={esEntrada ? 'Registrar entrada' : 'Registrar salida'} subtitle="CONTROL DE TIEMPO" onClose={onClose}>
    <form className="form-grid" onSubmit={submit}>
      <div className="span-2">
        <label className="field-label">Empleado</label>
        {selected
          ? <div className="selected-employee">
              <span className="picker-avatar">{inicialesDe(selected)}</span>
              <span><strong>{nombreDe(selected)}</strong><small>{selected.area}{selected.cargo ? ` · ${selected.cargo}` : ''}</small></span>
              <button type="button" className="ghost-button tiny" onClick={() => { setSelected(null); setEstado(null) }}>Cambiar</button>
            </div>
          : <EmployeePicker onPick={setSelected} />}
      </div>

      {estado && <div className="context-card span-2">
        <div><span>Horario de hoy</span><strong>{estado.tieneHorarioHoy ? `${horaDe(estado.horaInicioProgramada) ?? ''} - ${horaDe(estado.horaFinProgramada) ?? ''}` : 'Sin horario asignado'}</strong></div>
        <div><span>Turno</span><strong>{estado.tipoTurno || '—'}</strong></div>
        <div><span>Estado actual</span><strong>{estado.tieneEntradaActiva ? 'En curso' : estado.yaRegistroHoy ? estado.estadoAsistencia : 'Sin registro'}</strong></div>
        <div><span>Entrada</span><strong>{horaCorta(estado.fechaHoraEntrada) || '—'}</strong></div>
      </div>}

      <label className="span-2">{esEntrada ? 'Hora de entrada' : 'Hora de salida'}
        <input type="datetime-local" value={hora} onChange={e => setHora(e.target.value)} required />
      </label>

      {bloqueado && <div className="notice warning span-2">{esEntrada
        ? 'Este empleado ya tiene un registro de entrada hoy.'
        : 'No hay una entrada activa para este empleado.'}</div>}
      {error && <div className="form-error span-2">{error}</div>}

      <div className="modal-actions span-2">
        <button type="button" className="ghost-button" onClick={onClose}>Cancelar</button>
        <button type="submit" className="primary-button compact" disabled={saving || !selected || bloqueado}>
          {saving ? 'Guardando...' : esEntrada ? 'Registrar entrada' : 'Registrar salida'}
        </button>
      </div>
    </form>
  </Modal>
}

function ResumenEmpleadoModal({ empleado, onClose }) {
  const now = new Date()
  const inicio = new Date(now.getFullYear(), now.getMonth(), 1).toISOString().slice(0, 10)
  const fin = now.toISOString().slice(0, 10)
  const [data, setData] = useState(null)
  const [error, setError] = useState('')

  useEffect(() => {
    api.get(`/Asistencia/resumen/${empleado.empleadoId}`, { params: { fechaInicio: inicio, fechaFin: fin } })
      .then(r => setData(r.data.data))
      .catch(() => setError('No pudimos cargar el resumen.'))
  }, [empleado.empleadoId])

  return <Modal title={nombreDe(empleado)} subtitle="RESUMEN DEL MES" onClose={onClose}>
    {error && <div className="notice error">{error}</div>}
    {!data && !error && <div className="empty-state">Cargando resumen...</div>}
    {data && <div className="resumen-grid">
      <div className="resumen-item"><span>Días con registro</span><strong>{data.diasConRegistro}</strong></div>
      <div className="resumen-item"><span>Presentes</span><strong>{data.presentes}</strong></div>
      <div className="resumen-item"><span>Tardanzas</span><strong>{data.tardanzas}</strong></div>
      <div className="resumen-item"><span>Horas trabajadas</span><strong>{data.totalHorasTrabajadas}h</strong></div>
      <div className="resumen-item"><span>Horas extra</span><strong className="success-text">{data.totalHorasExtras}h</strong></div>
      <div className="resumen-item"><span>Horas faltantes</span><strong className="warning-text">{data.totalHorasFaltantes}h</strong></div>
      <div className="resumen-item wide"><span>Cumplimiento promedio</span><div className="progress"><i style={{ width: `${Math.min(data.cumplimientoPromedio, 100)}%` }} /></div><strong>{data.cumplimientoPromedio}%</strong></div>
    </div>}
  </Modal>
}

function Attendance() {
  const { user } = useAuth()
  const now = new Date()
  const primerDia = new Date(now.getFullYear(), now.getMonth(), 1).toISOString().slice(0, 10)
  const hoy = new Date().toISOString().slice(0, 10)
  const [fechaInicio, setFechaInicio] = useState(primerDia)
  const [fechaFin, setFechaFin] = useState(hoy)
  const [estadoFiltro, setEstadoFiltro] = useState('')
  const [data, setData] = useState(null)
  const [feedback, setFeedback] = useState(null)
  const [modal, setModal] = useState(null)
  const [page, setPage] = useState(1)

  const load = () => {
    api.get('/Asistencia', {
      params: { fechaInicio, fechaFin, estadoAsistencia: estadoFiltro || undefined, page, pageSize: 10 }
    })
      .then(r => setData(r.data))
      .catch(() => setFeedback({ type: 'error', message: 'No pudimos cargar la asistencia.' }))
  }
  useEffect(() => { load() }, [fechaInicio, fechaFin, estadoFiltro, page])

  function done(message) { setModal(null); setFeedback({ type: 'ok', message }); load() }

  const rows = data?.data || []
  const presentes = rows.filter(r => r.estadoAsistencia === 'Presente').length
  const tardanzas = rows.filter(r => r.estadoAsistencia === 'Tardanza').length
  const enCurso = rows.filter(r => !r.fechaHoraSalida).length
  const horas = Math.round(rows.reduce((a, r) => a + (r.horasTrabajadasReal || 0), 0) * 100) / 100

  const rolEmpleado = esEmpleado(user)
  const propio = { empleadoId: user.empleadoId, nombreCompleto: user.nombreCompleto }

  return <>
    <PageHeader
      eyebrow="CONTROL DE TIEMPO"
      title={rolEmpleado ? 'Mi asistencia' : 'Asistencia'}
      description={rolEmpleado
        ? 'Registra tu entrada y salida, y consulta tu histórico.'
        : 'Registra entradas y salidas, y compara las horas planificadas con el trabajo real.'}
      action={<div className="report-actions">
        {!rolEmpleado && <button className="ghost-button" onClick={() => setModal({ mode: 'salida', empleado: null })}><LogOut size={16} /> Registrar salida</button>}
        <button className={rolEmpleado ? 'ghost-button' : 'primary-button compact'} onClick={() => setModal({ mode: rolEmpleado ? 'salida' : 'entrada', empleado: rolEmpleado ? propio : null })}>
          <LogOut size={16} /> {rolEmpleado ? 'Mi salida' : 'Registrar entrada'}
        </button>
        {rolEmpleado && <button className="primary-button compact" onClick={() => setModal({ mode: 'entrada', empleado: propio })}><LogIn size={16} /> Mi entrada</button>}
      </div>}
    />

    {feedback && <div className={`notice ${feedback.type}`}>{feedback.message}</div>}

    <div className="kpi-grid">
      <KpiCard label="Presentes" value={presentes} detail="Registros al día" icon={UserCheck} tone="lime" />
      <KpiCard label="En curso" value={enCurso} detail="Sin salida registrada" icon={Clock3} tone="blue" />
      <KpiCard label="Tardanzas" value={tardanzas} detail="Fuera de tolerancia" icon={AlertTriangle} tone="orange" />
      <KpiCard label="Horas registradas" value={horas} unit="h" detail="En el período" icon={Activity} tone="violet" />
    </div>

    <section className="panel list-panel">
      <div className="list-toolbar">
        <div className="toolbar-filters">
          <label className="filter-field">Desde<input type="date" value={fechaInicio} max={fechaFin} onChange={e => { setFechaInicio(e.target.value); setPage(1) }} /></label>
          <label className="filter-field">Hasta<input type="date" value={fechaFin} min={fechaInicio} onChange={e => { setFechaFin(e.target.value); setPage(1) }} /></label>
          <label className="filter-field">Estado
            <select value={estadoFiltro} onChange={e => { setEstadoFiltro(e.target.value); setPage(1) }}>
              <option value="">Todos</option>
              {['Presente', 'Tardanza', 'SalidaTemprana', 'AusenciaJustificada', 'AusenciaInjustificada'].map(e => <option key={e} value={e}>{e}</option>)}
            </select>
          </label>
        </div>
        <span className="result-count">{data?.totalRecords ?? 0} registros</span>
      </div>
      <Pagination data={data} onPageChange={setPage} />

      <div className="table-wrap">
        <table>
          <thead><tr><th>Empleado</th><th>Fecha</th><th>Entrada</th><th>Salida</th><th>Horas</th><th>Retraso</th><th>Estado</th>{!rolEmpleado && <th>Acciones</th>}</tr></thead>
          <tbody>
            {rows.map(row => <tr key={row.asistenciaId}>
              <td><strong>{row.nombreCompleto}</strong>{!rolEmpleado && <small className="cell-subtitle">{row.area} · {row.cargo}</small>}</td>
              <td>{row.fecha}</td>
              <td>{horaCorta(row.fechaHoraEntrada)}</td>
              <td>{horaCorta(row.fechaHoraSalida) || <span className="in-progress">En curso</span>}</td>
              <td>{row.horasTrabajadasReal ?? '—'}</td>
              <td>{row.minutosRetraso > 0 ? <span className="warning-text">{Math.round(row.minutosRetraso)} min</span> : '—'}</td>
              <td><span className={`status-pill ${row.estadoAsistencia === 'Tardanza' ? 'warning' : 'success'}`}><i /> {row.estadoAsistencia}</span></td>
              {!rolEmpleado && <td><div className="row-actions">
                {!row.fechaHoraSalida && <button className="icon-action" title="Registrar salida" onClick={() => setModal({ mode: 'salida', empleado: { empleadoId: row.empleadoId, nombreCompleto: row.nombreCompleto, area: row.area, cargo: row.cargo } })}><LogOut size={15} /></button>}
                <button className="icon-action" title="Ver resumen" onClick={() => setModal({ resumen: { empleadoId: row.empleadoId, nombreCompleto: row.nombreCompleto } })}><ClipboardList size={15} /></button>
              </div></td>}
            </tr>)}
            {!rows.length && <tr><td colSpan={rolEmpleado ? 7 : 8} className="empty-cell">No hay registros de asistencia en el período seleccionado.</td></tr>}
          </tbody>
        </table>
      </div>
    </section>

    {modal?.resumen && <ResumenEmpleadoModal empleado={modal.resumen} onClose={() => setModal(null)} />}
    {modal?.mode && <RegistroAsistenciaModal mode={modal.mode} preselected={modal.empleado} onClose={() => setModal(null)} onDone={done} />}
  </>
}

const reportTypes = [
  { id: 'horas', label: 'Horas por empleado' },
  { id: 'asistencia', label: 'Asistencia detallada' },
]

function Reports() {
  const now = new Date()
  const firstDay = new Date(now.getFullYear(), now.getMonth(), 1).toISOString().slice(0, 10)
  const today = now.toISOString().slice(0, 10)
  const [type, setType] = useState('horas')
  const [fechaInicio, setFechaInicio] = useState(firstDay)
  const [fechaFin, setFechaFin] = useState(today)
  const [rows, setRows] = useState([])
  const [loading, setLoading] = useState(false)
  const [feedback, setFeedback] = useState(null)

  useEffect(() => {
    if (!fechaInicio || !fechaFin) return
    setLoading(true)
    api.get(`/Reportes/${type}`, { params: { fechaInicio, fechaFin } })
      .then(r => setRows(r.data || []))
      .catch(() => setFeedback({ type: 'error', message: 'No pudimos generar el reporte.' }))
      .finally(() => setLoading(false))
  }, [type, fechaInicio, fechaFin])

  async function download(format) {
    try {
      setFeedback(null)
      const res = await api.get(`/Reportes/${type}/${format}`, { params: { fechaInicio, fechaFin }, responseType: 'blob' })
      const url = URL.createObjectURL(new Blob([res.data]))
      const a = document.createElement('a')
      a.href = url
      a.download = `reporte-${type}-${fechaInicio}-${fechaFin}.${format === 'excel' ? 'xlsx' : 'pdf'}`
      document.body.appendChild(a); a.click(); a.remove()
      URL.revokeObjectURL(url)
      setFeedback({ type: 'ok', message: `Reporte ${format === 'excel' ? 'Excel' : 'PDF'} descargado correctamente.` })
    } catch {
      setFeedback({ type: 'error', message: 'No pudimos descargar el archivo. Revisa que la API esté activa.' })
    }
  }

  return <>
    <PageHeader
      eyebrow="CENTRO DE REPORTES"
      title="Reportes"
      description="Analiza horas y asistencia, y exporta la información para tu equipo."
      action={<div className="report-actions">
        <button className="ghost-button" onClick={() => download('excel')}><FileSpreadsheet size={16} /> Excel</button>
        <button className="primary-button compact" onClick={() => download('pdf')}><FileText size={16} /> PDF</button>
      </div>}
    />

    {feedback && <div className={`notice ${feedback.type}`}>{feedback.message}</div>}

    <section className="panel report-controls">
      <div className="segmented">
        {reportTypes.map(opt => <button key={opt.id} className={type === opt.id ? 'active' : ''} onClick={() => setType(opt.id)}>{opt.label}</button>)}
      </div>
      <div className="range-fields">
        <label>Desde<input type="date" value={fechaInicio} max={fechaFin} onChange={e => setFechaInicio(e.target.value)} /></label>
        <label>Hasta<input type="date" value={fechaFin} min={fechaInicio} onChange={e => setFechaFin(e.target.value)} /></label>
      </div>
    </section>

    <section className="panel list-panel">
      <div className="list-toolbar">
        <span className="eyebrow">{type === 'horas' ? 'HORAS POR EMPLEADO' : 'ASISTENCIA DETALLADA'}</span>
        <span className="result-count">{loading ? 'Generando...' : `${rows.length} registros`}</span>
      </div>
      <div className="table-wrap">
        {type === 'horas'
          ? <table>
              <thead><tr><th>Empleado</th><th>Área</th><th>Días</th><th>Horas prog.</th><th>Horas trabaj.</th><th>Extra</th><th>Faltante</th><th>Tardanzas</th><th>Cumplimiento</th></tr></thead>
              <tbody>
                {rows.map((r, i) => <tr key={i}>
                  <td><strong>{r.nombreCompleto}</strong><small className="cell-subtitle">{r.cargo}</small></td>
                  <td>{r.area}</td><td>{r.diasAsistidos}/{r.diasProgramados}</td>
                  <td>{r.horasProgramadas}</td><td><strong>{r.horasTrabajadas}</strong></td>
                  <td><span className="success-text">{r.horasExtras}</span></td>
                  <td><span className="warning-text">{r.horasFaltantes}</span></td>
                  <td>{r.tardanzas}</td>
                  <td><div className="progress-cell"><div className="progress"><i style={{ width: `${Math.min(r.cumplimiento, 100)}%` }} /></div><span>{r.cumplimiento}%</span></div></td>
                </tr>)}
                {!rows.length && <tr><td colSpan="9" className="empty-cell">{loading ? 'Generando reporte...' : 'No hay datos en el período seleccionado.'}</td></tr>}
              </tbody>
            </table>
          : <table>
              <thead><tr><th>Fecha</th><th>Empleado</th><th>Área</th><th>Entrada</th><th>Salida</th><th>Horas</th><th>Retraso</th><th>Estado</th></tr></thead>
              <tbody>
                {rows.map((r, i) => <tr key={i}>
                  <td>{r.fecha}</td>
                  <td><strong>{r.nombreCompleto}</strong></td>
                  <td>{r.area}</td>
                  <td>{r.horaEntrada?.slice(11, 16)}</td>
                  <td>{r.horaSalida?.slice(11, 16) || <span className="in-progress">En curso</span>}</td>
                  <td>{r.horasTrabajadas}</td>
                  <td>{r.minutosRetraso > 0 ? <span className="warning-text">{r.minutosRetraso} min</span> : '—'}</td>
                  <td><span className={`status-pill ${r.estado === 'Tardanza' ? 'warning' : 'success'}`}><i /> {r.estado}</span></td>
                </tr>)}
                {!rows.length && <tr><td colSpan="8" className="empty-cell">{loading ? 'Generando reporte...' : 'No hay datos en el período seleccionado.'}</td></tr>}
              </tbody>
            </table>}
      </div>
    </section>
  </>
}

const catalogDefinitions = {
  areas: { label: 'Áreas', endpoint: 'areas', fields: ['nombre', 'descripcion'] },
  cargos: { label: 'Cargos', endpoint: 'cargos', fields: ['nombre', 'descripcion'] },
  turnos: { label: 'Tipos de turno', endpoint: 'tipos-turno', fields: ['nombre', 'horaInicio', 'horaFin', 'horasEsperadas'] },
}

function CatalogoForm({ type, item, onClose, onSaved }) {
  const [form, setForm] = useState(item || {
    nombre: '', descripcion: '', horaInicio: '07:00', horaFin: '16:00', horasEsperadas: '8'
  })
  const [error, setError] = useState('')
  const [saving, setSaving] = useState(false)
  const def = catalogDefinitions[type]
  const set = (key, value) => setForm(prev => ({ ...prev, [key]: value }))

  async function submit(e) {
    e.preventDefault(); setError(''); setSaving(true)
    const payload = type === 'turnos'
      ? { nombre: form.nombre.trim(), horaInicio: `${form.horaInicio}:00`, horaFin: `${form.horaFin}:00`, horasEsperadas: Number(form.horasEsperadas) }
      : { nombre: form.nombre.trim(), descripcion: form.descripcion?.trim() || null }
    try {
      const url = `/Catalogos/${def.endpoint}${item ? `/${item.id}` : ''}`
      if (item) await api.put(url, payload)
      else await api.post(url, payload)
      onSaved(item ? 'Catálogo actualizado correctamente.' : 'Registro creado correctamente.')
    } catch (err) {
      setError(err.response?.data?.message || 'No se pudo guardar el registro.')
    } finally { setSaving(false) }
  }

  return <Modal title={item ? `Editar ${def.label.slice(0, -1)}` : `Nuevo registro`} subtitle="CONFIGURACIÓN" onClose={onClose}>
    <form className="form-grid" onSubmit={submit}>
      <label className="span-2">Nombre<input value={form.nombre} onChange={e => set('nombre', e.target.value)} required maxLength={100} /></label>
      {type !== 'turnos' && <label className="span-2">Descripción<textarea rows={3} value={form.descripcion || ''} onChange={e => set('descripcion', e.target.value)} /></label>}
      {type === 'turnos' && <>
        <label>Hora de inicio<input type="time" value={String(form.horaInicio).slice(0, 5)} onChange={e => set('horaInicio', e.target.value)} required /></label>
        <label>Hora de fin<input type="time" value={String(form.horaFin).slice(0, 5)} onChange={e => set('horaFin', e.target.value)} required /></label>
        <label className="span-2">Horas esperadas<input type="number" min="0.01" max="24" step="0.25" value={form.horasEsperadas} onChange={e => set('horasEsperadas', e.target.value)} required /></label>
      </>}
      {error && <div className="form-error span-2">{error}</div>}
      <div className="modal-actions span-2">
        <button type="button" className="ghost-button" onClick={onClose}>Cancelar</button>
        <button type="submit" className="primary-button compact" disabled={saving}>{saving ? 'Guardando...' : 'Guardar'}</button>
      </div>
    </form>
  </Modal>
}

function UsuarioForm({ options, onClose, onSaved }) {
  const [form, setForm] = useState({ empleadoId: '', rolId: '', password: '' })
  const [error, setError] = useState('')
  const [saving, setSaving] = useState(false)
  const set = (key, value) => setForm(prev => ({ ...prev, [key]: value }))
  const empleado = options.empleados.find(e => String(e.empleadoId) === String(form.empleadoId))

  async function submit(e) {
    e.preventDefault(); setError(''); setSaving(true)
    try {
      await api.post('/Auth/register', {
        empleadoId: Number(form.empleadoId),
        email: form.email,
        password: form.password,
        rolId: Number(form.rolId)
      })
      onSaved('Usuario registrado correctamente.')
    } catch (err) { setError(err.response?.data?.message || 'No se pudo registrar el usuario.') }
    finally { setSaving(false) }
  }

  return <Modal title="Nuevo usuario" subtitle="ACCESO AL SISTEMA" onClose={onClose}>
    <form className="form-grid" onSubmit={submit}>
      <label className="span-2">Empleado
        <select value={form.empleadoId} onChange={e => set('empleadoId', e.target.value)} required>
          <option value="">Selecciona un empleado sin usuario</option>
          {options.empleados.map(e => <option key={e.empleadoId} value={e.empleadoId}>{e.nombreCompleto} · {e.area}</option>)}
        </select>
      </label>
      {empleado && <div className="summary-box span-2"><div><span>Empleado</span><strong>{empleado.nombreCompleto}</strong></div><div><span>Área</span><strong>{empleado.area}</strong></div><div><span>Cargo</span><strong>{empleado.cargo}</strong></div></div>}
      <label className="span-2">Correo electrónico<input type="email" value={form.email || ''} onChange={e => set('email', e.target.value)} required maxLength={150} placeholder="persona@empresa.com" /></label>
      <label>Rol<select value={form.rolId} onChange={e => set('rolId', e.target.value)} required><option value="">Selecciona un rol</option>{options.roles.map(r => <option key={r.rolId} value={r.rolId}>{r.nombre}</option>)}</select></label>
      <label>Contraseña<input type="password" value={form.password} onChange={e => set('password', e.target.value)} required minLength={8} placeholder="Mínimo 8 caracteres" /></label>
      {error && <div className="form-error span-2">{error}</div>}
      <div className="modal-actions span-2"><button type="button" className="ghost-button" onClick={onClose}>Cancelar</button><button type="submit" className="primary-button compact" disabled={saving}>{saving ? 'Registrando...' : 'Registrar usuario'}</button></div>
    </form>
  </Modal>
}

function UsuariosPanel({ open, onClose }) {
  const [rows, setRows] = useState([])
  const [options, setOptions] = useState({ empleados: [], roles: [] })
  const [modal, setModal] = useState(false)
  const [feedback, setFeedback] = useState(null)
  const load = () => Promise.all([api.get('/Auth/usuarios'), api.get('/Auth/usuarios/opciones')]).then(([u, o]) => { setRows(u.data); setOptions(o.data) }).catch(() => setFeedback({ type: 'error', message: 'No pudimos cargar los usuarios.' }))
  useEffect(() => { load() }, [])
  useEffect(() => { if (open) setModal(true) }, [open])
  async function toggle(row) { try { await api.patch(`/Auth/usuarios/${row.usuarioId}/${row.activo ? 'desactivar' : 'activar'}`); setFeedback({ type: 'ok', message: row.activo ? 'Usuario desactivado.' : 'Usuario activado.' }); load() } catch (err) { setFeedback({ type: 'error', message: err.response?.data?.message || 'No se pudo cambiar el estado.' }) } }
  return <>
    {feedback && <div className={`notice ${feedback.type}`}>{feedback.message}</div>}
    <section className="panel list-panel"><div className="list-toolbar"><span className="eyebrow">CUENTAS REGISTRADAS</span><span className="result-count">{rows.length} usuarios</span></div><div className="table-wrap"><table><thead><tr><th>Usuario</th><th>Empleado</th><th>Rol</th><th>Correo</th><th>Estado</th><th>Acciones</th></tr></thead><tbody>{rows.map(row => <tr key={row.usuarioId}><td>#{row.usuarioId}</td><td><strong>{row.nombreCompleto}</strong></td><td><span className="role-tag">{row.rol}</span></td><td>{row.email}</td><td><span className={`status-pill ${row.activo ? 'success' : 'muted'}`}><i /> {row.activo ? 'Activo' : 'Inactivo'}</span></td><td><button className="icon-action" onClick={() => toggle(row)}>{row.activo ? <PowerOff size={15} /> : <Power size={15} />}</button></td></tr>)}{!rows.length && <tr><td colSpan="6" className="empty-cell">No hay usuarios registrados.</td></tr>}</tbody></table></div></section>
    {modal && <UsuarioForm options={options} onClose={() => { setModal(false); onClose() }} onSaved={message => { setModal(false); onClose(); setFeedback({ type: 'ok', message }); load() }} />}
  </>
}

function Configuracion() {
  const [type, setType] = useState('areas')
  const [rows, setRows] = useState([])
  const [modal, setModal] = useState(null)
  const [feedback, setFeedback] = useState(null)
  const def = catalogDefinitions[type]

  const load = () => def && api.get(`/Catalogos/admin/${def.endpoint}`).then(r => setRows(r.data)).catch(() => setFeedback({ type: 'error', message: 'No pudimos cargar el catálogo.' }))
  useEffect(() => { if (type !== 'usuarios') load() }, [type])

  function saved(message) { setModal(null); setFeedback({ type: 'ok', message }); load() }

  async function toggle(row) {
    try {
      await api.patch(`/Catalogos/${def.endpoint}/${row.id}/${row.activo ? 'desactivar' : 'activar'}`)
      setFeedback({ type: 'ok', message: row.activo ? 'Registro desactivado.' : 'Registro activado.' })
      load()
    } catch (err) { setFeedback({ type: 'error', message: err.response?.data?.message || 'No se pudo cambiar el estado.' }) }
  }

  const usuarios = type === 'usuarios'
  return <>
    <PageHeader eyebrow="CONFIGURACIÓN DEL SISTEMA" title={usuarios ? 'Usuarios' : 'Catálogos'} description={usuarios ? 'Crea y administra las cuentas de acceso al sistema.' : 'Administra las opciones que alimentan empleados, horarios y reportes.'} action={<button className="primary-button compact" onClick={() => usuarios ? setModal({ user: true }) : setModal({ item: null })}><Plus size={16} /> Nuevo registro</button>} />
    {feedback && <div className={`notice ${feedback.type}`}>{feedback.message}</div>}
    <section className="panel report-controls">
      <div className="segmented">
        {[...Object.entries(catalogDefinitions), ['usuarios', { label: 'Usuarios' }]].map(([id, value]) => <button key={id} className={type === id ? 'active' : ''} onClick={() => { setType(id); setModal(null); setFeedback(null) }}>{value.label}</button>)}
      </div>
      {!usuarios && <span className="result-count">{rows.length} registros</span>}
    </section>
    {usuarios ? <UsuariosPanel open={modal?.user} onClose={() => setModal(null)} /> : <section className="panel list-panel">
      <div className="table-wrap">
        <table>
          <thead><tr><th>Nombre</th>{type !== 'turnos' && <th>Descripción</th>}{type === 'turnos' && <><th>Inicio</th><th>Fin</th><th>Horas</th></>}<th>Estado</th><th>Acciones</th></tr></thead>
          <tbody>{rows.map(row => <tr key={row.id}>
            <td><strong>{row.nombre}</strong></td>
            {type !== 'turnos' && <td>{row.descripcion || '—'}</td>}
            {type === 'turnos' && <><td>{String(row.horaInicio).slice(0, 5)}</td><td>{String(row.horaFin).slice(0, 5)}</td><td>{row.horasEsperadas}h</td></>}
            <td><span className={`status-pill ${row.activo ? 'success' : 'muted'}`}><i /> {row.activo ? 'Activo' : 'Inactivo'}</span></td>
            <td><div className="row-actions"><button className="icon-action" title="Editar" onClick={() => setModal({ item: row })}><Pencil size={15} /></button><button className="icon-action" title={row.activo ? 'Desactivar' : 'Activar'} onClick={() => toggle(row)}>{row.activo ? <PowerOff size={15} /> : <Power size={15} />}</button></div></td>
          </tr>)}{!rows.length && <tr><td colSpan="6" className="empty-cell">No hay registros.</td></tr>}</tbody>
        </table>
      </div>
    </section>}
    {modal?.item !== undefined && <CatalogoForm type={type} item={modal.item} onClose={() => setModal(null)} onSaved={saved} />}
  </>
}

function RequireRole({ roles, children }) {
  const { user } = useAuth()
  if (!roles.includes(user?.rol)) return <Navigate to="/" replace />
  return children
}

function App() {
  return <AuthProvider>
    <Routes>
      <Route path="/login" element={<Login />} />
      <Route element={<RequireAuth />}>
        <Route element={<AppShell />}>
          <Route index element={<RoleHome />} />
          <Route path="empleados" element={<RequireRole roles={['Administrador', 'Supervisor']}><Employees /></RequireRole>} />
          <Route path="horarios" element={<Schedules />} />
          <Route path="asistencia" element={<Attendance />} />
          <Route path="reportes" element={<RequireRole roles={['Administrador', 'Supervisor']}><Reports /></RequireRole>} />
          <Route path="configuracion" element={<RequireRole roles={['Administrador']}><Configuracion /></RequireRole>} />
        </Route>
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  </AuthProvider>
}

function RoleHome() {
  const { user } = useAuth()
  return esEmpleado(user) ? <MiDashboard /> : <Dashboard />
}

ReactDOM.createRoot(document.getElementById('root')).render(<BrowserRouter><App /></BrowserRouter>)
