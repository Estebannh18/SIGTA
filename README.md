# WorkForce Manager Pro

Sistema empresarial de gestión de personal, turnos y horas trabajadas. El proyecto permite administrar empleados, planificar jornadas, registrar asistencia, calcular cumplimiento y generar reportes operativos.

Está construido como proyecto de portafolio full-stack, con separación entre API, reglas de negocio, persistencia y frontend.

## Funcionalidades

- Autenticación con JWT.
- Roles de Administrador, Supervisor y Empleado.
- Permisos por rol en backend y frontend.
- CRUD de empleados.
- CRUD administrativo de áreas, cargos y tipos de turno.
- Asignación individual y masiva de horarios.
- Calendario de horarios en vista lista, semana y mes.
- Registro de entrada y salida.
- Detección de tardanzas y salidas tempranas.
- Dashboard con KPIs, tendencia de horas y rendimiento por área.
- Dashboard personal para empleados.
- Reportes de horas y asistencia.
- Exportación de reportes a Excel y PDF.
- Paginación real en las tablas principales.
- Tests unitarios y de autorización.

## Stack

### Backend

- .NET 8 Web API.
- Entity Framework Core 8.
- SQL Server.
- JWT Bearer Authentication.
- Arquitectura por capas: Controllers, Services, Repositories y Data.
- ClosedXML para Excel.
- QuestPDF para PDF.

### Frontend

- React.
- Vite.
- React Router.
- Axios.
- Lucide React.
- Diseño responsive con sistema visual Enterprise Editorial.

### Testing

- xUnit.
- Moq.
- Microsoft.NET.Test.Sdk.

## Arquitectura

```text
WorkForce-Manager-Pro/
├── Controllers/              Endpoints HTTP
├── Data/                     DbContext y seed de desarrollo
├── Extensions/               Registro de servicios y JWT
├── Helpers/                  Utilidades compartidas
├── Middleware/               Manejo global de errores
├── Migrations/               Migraciones de Entity Framework
├── Models/
│   ├── Entities/             Entidades persistentes
│   └── DTOs/                 Requests y Responses
├── Repositories/             Acceso a datos
├── Services/                 Lógica de negocio y reportes
├── frontend/                 Aplicación React + Vite
└── tests/                    Tests automatizados
```

## Requisitos

- .NET SDK 8 o superior.
- Node.js 18 o superior.
- SQL Server Express o Developer.
- SQL Server Management Studio opcional, recomendado.

La configuración local actual usa:

```text
Servidor: localhost\SQLEXPRESS
Base de datos: WorkForceManagerDB
Autenticación: Windows
API: http://localhost:5000
Frontend: http://localhost:5173
```

## Ejecución local

### 1. Base de datos

Crea la base de datos vacía en SQL Server:

```sql
CREATE DATABASE WorkForceManagerDB;
```

La API aplica las migraciones automáticamente cuando se ejecuta en entorno `Development`.

### 2. Backend

Desde la raíz del proyecto:

Para desarrollo local se recomienda usar User Secrets. Inicializa el almacenamiento una sola vez:

```powershell
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost\\SQLEXPRESS;Database=WorkForceManagerDB;Trusted_Connection=True;TrustServerCertificate=True;"
dotnet user-secrets set "JwtSettings:SecretKey" "una-clave-local-larga-y-aleatoria-de-32-o-mas-caracteres"
```

Para producción, usa variables de entorno o un secret manager tomando como referencia `.env.example`.

```powershell
$env:DOTNET_ROLL_FORWARD="Major"
dotnet run
```

La API quedará disponible en:

```text
http://localhost:5000
```

Swagger estará disponible en:

```text
http://localhost:5000/swagger
```

### 3. Frontend

En otra terminal:

```powershell
cd frontend
npm install
npm run dev
```

Para configurar la URL de la API, copia `frontend/.env.example` a `frontend/.env` y ajusta `VITE_API_URL` si es necesario.

La aplicación quedará disponible en:

```text
http://localhost:5173
```

El frontend utiliza `http://localhost:5000/api` por defecto. Puede cambiarse mediante `VITE_API_URL`.

## Usuarios de desarrollo

El seeder de desarrollo crea usuarios demo cuando corresponde:

| Rol | Email | Contraseña |
|---|---|---|
| Administrador | `admin@workforce.local` | `Admin123!` |
| Supervisor | `supervisor@workforce.local` | `Super123!` |
| Empleado | `empleado@workforce.local` | `Empleado123!` |

Estas credenciales son únicamente para desarrollo local y no deben utilizarse en producción.

## Endpoints principales

### Autenticación

```text
POST /api/Auth/login
POST /api/Auth/register
GET  /api/Auth/perfil
GET  /api/Auth/usuarios                  Administrador
GET  /api/Auth/usuarios/opciones         Administrador
```

### Empleados

```text
GET    /api/Empleados
GET    /api/Empleados/{id}
POST   /api/Empleados
PUT    /api/Empleados/{id}
PATCH  /api/Empleados/{id}/activar
PATCH  /api/Empleados/{id}/desactivar
```

### Horarios

```text
GET  /api/Horarios
POST /api/Horarios
POST /api/Horarios/asignacion-masiva
DELETE /api/Horarios/{id}
```

### Asistencia

```text
GET  /api/Asistencia
POST /api/Asistencia/entrada
POST /api/Asistencia/salida
GET  /api/Asistencia/estado-hoy/{empleadoId}
GET  /api/Asistencia/resumen/{empleadoId}
```

### Dashboard

```text
GET /api/Dashboard/resumen
GET /api/Dashboard/areas
GET /api/Dashboard/tendencia
```

Los endpoints del Dashboard aceptan rangos mediante `fechaInicio` y `fechaFin`.

### Reportes

```text
GET /api/Reportes/horas
GET /api/Reportes/horas/excel
GET /api/Reportes/horas/pdf
GET /api/Reportes/asistencia
GET /api/Reportes/asistencia/excel
GET /api/Reportes/asistencia/pdf
```

## Tests

Ejecutar la suite completa:

```powershell
$env:DOTNET_ROLL_FORWARD="Major"
dotnet test tests\WorkForceManagerAPI.Tests\WorkForceManagerAPI.Tests.csproj
```

La suite cubre:

- Lectura de claims JWT.
- Cálculo de tardanzas.
- Rechazo de entradas duplicadas.
- Restricciones de acceso por rol.
- Protección de datos de empleados frente a consultas ajenas.

## Seguridad y configuración

No deben subirse secretos reales al repositorio. Antes de desplegar:

- Mover `JwtSettings:SecretKey` a una variable de entorno o secret manager.
- Mover la cadena de conexión fuera de `appsettings.json`.
- Configurar `VITE_API_URL` para el dominio público de la API.
- Restringir CORS al dominio real del frontend.
- Cambiar las credenciales demo.
- Activar HTTPS.

## Estado del proyecto

El sistema funciona localmente con SQL Server, API .NET 8 y frontend React. El siguiente paso operativo es preparar la configuración segura de entorno y el despliegue final.

## Licencia

Proyecto de portafolio personal.
