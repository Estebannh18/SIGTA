import React, { createContext, useContext, useEffect, useState } from 'react'
import ReactDOM from 'react-dom/client'
import { BrowserRouter, Navigate, NavLink, Outlet, Route, Routes, useNavigate } from 'react-router-dom'
import axios from 'axios'
import {
  Activity, AlertTriangle, ArrowUpRight, BarChart3, CalendarDays, CheckCircle2,
  ChevronRight, Clock3, LayoutDashboard, LogOut, Menu, Search, ShieldCheck, Users, X
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

const navigation = [
  { to: '/', label: 'Resumen', icon: LayoutDashboard, end: true },
  { to: '/empleados', label: 'Empleados', icon: Users },
  { to: '/horarios', label: 'Horarios', icon: CalendarDays },
  { to: '/asistencia', label: 'Asistencia', icon: Clock3 },
]

function AppShell() {
  const { user, logout } = useAuth()
  const [open, setOpen] = useState(false)
  return <div className="app-shell">
    <aside className={`sidebar ${open ? 'sidebar-open' : ''}`}>
      <div className="brand">
        <div className="brand-mark">W</div>
        <div><strong>WorkForce</strong><span>MANAGER PRO</span></div>
        <button className="icon-button mobile-close" onClick={() => setOpen(false)}><X size={18} /></button>
      </div>
      <div className="workspace-label">CONTROL CENTER</div>
      <nav>
        {navigation.map(({ to, label, icon: Icon, end }) =>
          <NavLink key={to} to={to} end={end} onClick={() => setOpen(false)}>
            <Icon size={18} /><span>{label}</span>{label === 'Resumen' && <span className="nav-pulse" />}
          </NavLink>)}
      </nav>
      <div className="sidebar-bottom">
        <div className="security-note"><ShieldCheck size={18} /><div><strong>Entorno seguro</strong><span>Sesión protegida</span></div></div>
        <button className="logout-button" onClick={logout}><LogOut size={17} /> Cerrar sesión</button>
      </div>
    </aside>
    <main className="main-area">
      <header className="topbar">
        <button className="icon-button mobile-menu" onClick={() => setOpen(true)}><Menu size={21} /></button>
        <div className="breadcrumb"><span>Workspace</span><ChevronRight size={14} /><strong>Operaciones</strong></div>
        <div className="topbar-actions">
          <div className="live-status"><i /> Sistema operativo</div>
          <div className="avatar">{user?.nombreCompleto?.[0] || 'A'}</div>
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
        <div className="brand brand-light"><div className="brand-mark">W</div><div><strong>WorkForce</strong><span>MANAGER PRO</span></div></div>
        <div>
          <p className="eyebrow">WORKFORCE INTELLIGENCE</p>
          <h1>El pulso de tu operación, en un solo lugar.</h1>
          <p>Planifica turnos, entiende la asistencia y toma decisiones con datos que se mueven al ritmo de tu equipo.</p>
        </div>
        <div className="login-stat"><span>08:42</span><small>horas productivas<br />promedio hoy</small><ArrowUpRight size={20} /></div>
      </div>
    </div>
    <div className="login-panel">
      <div className="login-form">
        <div className="mobile-login-logo brand"><div className="brand-mark">W</div><div><strong>WorkForce</strong><span>MANAGER PRO</span></div></div>
        <p className="eyebrow">BIENVENIDO DE VUELTA</p>
        <h2>Inicia tu jornada.</h2>
        <p className="muted">Accede al centro de control de tu organización.</p>
        <form onSubmit={submit}>
          <label>Correo electrónico<input type="email" value={email} onChange={e => setEmail(e.target.value)} required /></label>
          <label>Contraseña<div className="password-field"><input type="password" value={password} onChange={e => setPassword(e.target.value)} required /><span>•••</span></div></label>
          {error && <div className="form-error">{error}</div>}
          <button className="primary-button" disabled={loading}>{loading ? 'Validando...' : 'Entrar al workspace'}<ArrowUpRight size={17} /></button>
        </form>
        <p className="login-foot">WorkForce Manager Pro <span>•</span> Plataforma empresarial</p>
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

function Dashboard() {
  const { user } = useAuth()
  const [summary, setSummary] = useState(null)
  const [areas, setAreas] = useState([])
  const [trend, setTrend] = useState([])
  const [error, setError] = useState('')

  useEffect(() => {
    Promise.all([api.get('/Dashboard/resumen'), api.get('/Dashboard/areas'), api.get('/Dashboard/tendencia')])
      .then(([s, a, t]) => { setSummary(s.data); setAreas(a.data); setTrend(t.data) })
      .catch(() => setError('No pudimos cargar los indicadores. Verifica que la API esté ejecutándose.'))
  }, [])

  const now = new Date()
  const dateLabel = now.toLocaleDateString('es-ES', { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' }).toUpperCase()
  const hour = now.getHours()
  const greeting = hour < 12 ? 'Buenos días' : hour < 19 ? 'Buenas tardes' : 'Buenas noches'
  const firstName = user?.nombreCompleto?.split(' ')?.[0] || 'Administrador'

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
      description="Esta es la lectura operativa de tu equipo para hoy."
      action={<button className="date-button"><CalendarDays size={16} /> Hoy <ChevronRight size={15} /></button>}
    />

    {error && <div className="notice error">{error}</div>}

    <div className="kpi-grid">
      <KpiCard label="Empleados activos" value={summary?.empleadosActivos ?? '—'} detail="Plantilla habilitada" icon={Users} tone="violet" />
      <KpiCard label="Cumplimiento" value={summary?.cumplimiento ?? 0} unit="%" detail="Sobre horarios planificados" icon={CheckCircle2} tone="lime" />
      <KpiCard label="Horas trabajadas" value={summary?.horasTrabajadas ?? 0} unit="h" detail="Acumuladas hoy" icon={Activity} tone="blue" spark={trend.map(t => t.horasTrabajadas)} />
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
                <span>{item.periodo.slice(5)}</span>
              </div>)
            : <div className="empty-state">Aún no hay registros de asistencia.</div>}
        </div>
      </section>

      <section className="panel jornada-panel">
        <div className="panel-heading">
          <div><p className="eyebrow">OPERACIÓN DE HOY</p><h2>Estado de jornada</h2></div>
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
        <div><p className="eyebrow">LECTURA POR EQUIPO</p><h2>Rendimiento por área</h2></div>
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

function Employees() {
  const [data, setData] = useState(null)
  const [query, setQuery] = useState('')
  useEffect(() => { api.get('/Empleados', { params: { termino: query, pageSize: 50 } }).then(r => setData(r.data)) }, [query])
  const rows = data?.data || []
  return <>
    <PageHeader eyebrow="DIRECTORIO DE PERSONAS" title="Empleados" description="Conoce y administra el talento que mueve la operación." action={<button className="primary-button compact"><Users size={16} /> Nuevo empleado</button>} />
    <section className="panel list-panel">
      <div className="list-toolbar">
        <div className="search-box"><Search size={17} /><input placeholder="Buscar por nombre o documento" value={query} onChange={e => setQuery(e.target.value)} /></div>
        <span className="result-count">{data?.totalRecords ?? 0} registros</span>
      </div>
      <div className="table-wrap">
        <table>
          <thead><tr><th>Empleado</th><th>Documento</th><th>Área</th><th>Cargo</th><th>Estado</th></tr></thead>
          <tbody>
            {rows.map(row => <tr key={row.empleadoId}>
              <td><div className="person-cell"><span>{row.nombres?.[0]}{row.apellidos?.[0]}</span><strong>{row.nombres} {row.apellidos}</strong></div></td>
              <td>{row.numeroDocumento}</td><td>{row.area}</td><td>{row.cargo}</td>
              <td><span className={`status-pill ${row.activo ? 'success' : 'muted'}`}><i /> {row.activo ? 'Activo' : 'Inactivo'}</span></td>
            </tr>)}
            {!rows.length && <tr><td colSpan="5" className="empty-cell">No se encontraron empleados.</td></tr>}
          </tbody>
        </table>
      </div>
    </section>
  </>
}

function Schedules() {
  const [data, setData] = useState(null)
  useEffect(() => { api.get('/Horarios', { params: { pageSize: 50 } }).then(r => setData(r.data)) }, [])
  const rows = data?.data || []
  return <>
    <PageHeader eyebrow="PLANIFICACIÓN OPERATIVA" title="Horarios" description="Diseña jornadas que se ajustan al ritmo de cada equipo." action={<button className="primary-button compact"><CalendarDays size={16} /> Asignar horario</button>} />
    <section className="panel list-panel">
      <div className="list-toolbar"><span className="eyebrow">AGENDA DE TURNOS</span><span className="result-count">{data?.totalRecords ?? 0} registros</span></div>
      <div className="table-wrap">
        <table>
          <thead><tr><th>Empleado</th><th>Área</th><th>Fecha</th><th>Turno</th><th>Jornada</th><th>Asignado por</th></tr></thead>
          <tbody>
            {rows.map(row => <tr key={row.horarioId}>
              <td><strong>{row.nombreCompleto}</strong></td><td>{row.area}</td><td>{row.fecha}</td>
              <td><span className="turno-tag">{row.tipoTurno}</span></td><td>{row.horaInicioProgramada} - {row.horaFinProgramada}</td><td>{row.asignadoPor}</td>
            </tr>)}
            {!rows.length && <tr><td colSpan="6" className="empty-cell">No hay horarios registrados.</td></tr>}
          </tbody>
        </table>
      </div>
    </section>
  </>
}

function Attendance() {
  const [data, setData] = useState(null)
  useEffect(() => { api.get('/Asistencia', { params: { pageSize: 50 } }).then(r => setData(r.data)) }, [])
  const rows = data?.data || []
  return <>
    <PageHeader eyebrow="CONTROL DE TIEMPO" title="Asistencia" description="Compara las horas planificadas con el trabajo real." action={<button className="primary-button compact"><Clock3 size={16} /> Registrar entrada</button>} />
    <section className="panel list-panel">
      <div className="list-toolbar"><span className="eyebrow">REGISTRO RECIENTE</span><span className="result-count">{data?.totalRecords ?? 0} registros</span></div>
      <div className="table-wrap">
        <table>
          <thead><tr><th>Empleado</th><th>Fecha</th><th>Entrada</th><th>Salida</th><th>Horas</th><th>Estado</th></tr></thead>
          <tbody>
            {rows.map(row => <tr key={row.asistenciaId}>
              <td><strong>{row.nombreCompleto}</strong><small className="cell-subtitle">{row.area}</small></td>
              <td>{row.fecha}</td><td>{row.fechaHoraEntrada?.slice(11, 16)}</td>
              <td>{row.fechaHoraSalida?.slice(11, 16) || <span className="in-progress">En curso</span>}</td>
              <td>{row.horasTrabajadasReal ?? '—'}</td>
              <td><span className={`status-pill ${row.estadoAsistencia === 'Tardanza' ? 'warning' : 'success'}`}><i /> {row.estadoAsistencia}</span></td>
            </tr>)}
            {!rows.length && <tr><td colSpan="6" className="empty-cell">No hay registros de asistencia.</td></tr>}
          </tbody>
        </table>
      </div>
    </section>
  </>
}

function App() {
  return <AuthProvider>
    <Routes>
      <Route path="/login" element={<Login />} />
      <Route element={<RequireAuth />}>
        <Route element={<AppShell />}>
          <Route index element={<Dashboard />} />
          <Route path="empleados" element={<Employees />} />
          <Route path="horarios" element={<Schedules />} />
          <Route path="asistencia" element={<Attendance />} />
        </Route>
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  </AuthProvider>
}

ReactDOM.createRoot(document.getElementById('root')).render(<BrowserRouter><App /></BrowserRouter>)
