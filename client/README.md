# RUTA24 · Gestión de transporte

Panel web para administrar la flota de buses, conductores, programaciones semanales, usuarios y roles de SistemaTransporte.Api.

## Requisitos

- Node.js 20.19+ o 22.12+
- SistemaTransporte.Api iniciado en `http://localhost:5267`

## Iniciar en desarrollo

Inicia la API con una clave JWT secreta de al menos 32 bytes en una variable de entorno. No guardes esa clave en el repositorio:

```powershell
$env:Jwt__Key = [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
dotnet run --project ..\SistemaTransporte.Api\SistemaTransporte.Api.csproj --launch-profile http
```

En otra terminal:

```powershell
npm install
npm run dev
```

Vite publica la aplicación en `http://localhost:5173` y reenvía las solicitudes `/api` al backend configurado. Para cambiar el destino:

```powershell
$env:VITE_API_PROXY_TARGET = "http://localhost:5267"
npm run dev
```

También se puede definir `VITE_API_BASE_URL` para apuntar a una URL de API accesible directamente. En ese caso, el servidor debe permitir solicitudes CORS desde el origen del frontend. El proxy de Vite solo aplica en desarrollo; para producción, publica la API detrás de un proxy inverso bajo el mismo origen o configura CORS en la infraestructura existente.

## Validar

```powershell
npm run build
npm run lint
```

## Funcionalidad

- Resumen con indicadores de flota y horarios.
- Alta, consulta, edición y eliminación de buses y conductores.
- Programaciones semanales con asignación de unidad, conductor, día y horario.
- Administración de buses activos por día.
- Gestión de cuentas de usuario y roles.
- Inicio de sesión real con token JWT y cierre de sesión.
- El rol oficial ID 1 (Administrador) administra los recursos; el ID 2 (Conductor) solo consulta sus horarios activos mediante `GET /api/Programaciones/mias`.
- Los roles personalizados u otras cuentas inactivas no obtienen una sesión autorizada.
- El token expira en 30 minutos. El frontend lo conserva en la sesión del navegador y vuelve al inicio de sesión al caducar.
