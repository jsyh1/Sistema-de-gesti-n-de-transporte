# Sistema de Gestión de Transporte

Sistema propuesto para administrar buses, conductores y sus horarios semanales. La solución contempla una interfaz web para los usuarios, una API REST para las operaciones y una base de datos para guardar la información.

> **Estado del repositorio:** esta copia incluye una API ASP.NET Core y un cliente React. La wiki describe requisitos y un diseño propuesto; no confirma por sí sola que todas las funcionalidades estén implementadas.

## Objetivo y alcance

El sistema busca centralizar la gestión de buses y conductores y organizar sus asignaciones en una programación semanal. La programación relaciona un bus, un conductor, un día y un horario, y debe respetar la disponibilidad de los recursos y los permisos del usuario.

### Funcionalidades previstas

- Registrar, consultar, modificar y eliminar buses.
- Registrar, consultar, modificar y eliminar conductores.
- Crear y mantener asignaciones de buses y conductores por día y horario.
- Consultar la programación semanal.
- Permitir que cada conductor consulte sus propias asignaciones.
- Autenticar usuarios y restringir las operaciones según sus roles.
- Validar datos, disponibilidad y conflictos antes de guardar una programación.

## Perfiles de usuario

| Perfil | Funcionalidades previstas |
| --- | --- |
| **Administrador** | Gestionar buses y conductores; crear, consultar, modificar y eliminar programaciones; consultar la programación semanal; administrar usuarios y roles, si esta opción está habilitada. |
| **Conductor** | Iniciar sesión y consultar sus propias asignaciones y horarios. No debe administrar recursos ni consultar información ajena fuera de los permisos concedidos. |

Los permisos deben comprobarse en el backend. Ocultar opciones en la interfaz no es suficiente para proteger una operación.

## Reglas de negocio principales

- La identificación de cada bus debe ser única y su capacidad debe ser mayor que cero.
- El día de la semana debe pertenecer al rango definido por el sistema (el esquema propuesto usa valores del 1 al 7).
- La hora de salida debe ser posterior a la hora de entrada.
- Solo deben asignarse buses y conductores habilitados y disponibles.
- Un bus no debe recibir más de una programación activa por día cuando se aplique esa restricción.
- Un conductor no debe tener asignaciones que se solapen.
- Al crear o modificar una programación, deben comprobarse tanto los conflictos como las relaciones válidas entre buses, conductores y horarios.
- Las operaciones deben respetar las restricciones de integridad al modificar o eliminar recursos relacionados con programaciones.

## Tecnologías y arquitectura propuestas

| Componente | Tecnología descrita en la wiki |
| --- | --- |
| Backend y API | .NET / ASP.NET Core Web API |
| Frontend | React |
| Comunicación | API REST sobre HTTP/HTTPS, con intercambio JSON |
| Autenticación | JWT (propuesta) |
| Documentación de API | Swagger / OpenAPI |
| Persistencia | Motor relacional; la elección debe confirmarse (ver [Decisiones pendientes](#decisiones-pendientes)) |

La API se describe por capas: controladores para solicitudes y respuestas, servicios para reglas de negocio, interfaces y repositorios para persistencia, y un contexto de datos si se utiliza Entity Framework Core. Las reglas de negocio y autorización deben permanecer en el servidor.

## Datos y API

El modelo conceptual contempla las entidades **Roles**, **Usuarios**, **Buses**, **Conductores** y **Programaciones**. La propuesta de base de datos también incluye `ProgramacionesActivasBusDia` para apoyar la regla de una programación activa por bus y día. Las relaciones y restricciones definitivas deben validarse contra el esquema implementado.

La wiki documenta como contrato propuesto estos endpoints de autenticación:

| Método | Endpoint | Descripción | Acceso |
| --- | --- | --- | --- |
| `POST` | `/api/auth/login` | Iniciar sesión y obtener un token JWT | Público |
| `GET` | `/api/auth/me` | Consultar la identidad y el rol del usuario autenticado | Usuario autenticado |

La URL local de la API depende de la configuración del backend; la wiki usa como referencia `https://localhost:{puerto}/api`. Los endpoints de buses, conductores y programaciones, así como los modelos de solicitud y respuesta, deben confirmarse con la implementación final.

### Enums del modelo

La API recibe y devuelve los enums como valores numéricos JSON. En MySQL, los enums se guardan como texto legible: `Buses.Estado`, `Conductores.Estado` y `DiaSemana` en `Programaciones` y `ProgramacionesActivasBusDia`:

| Enum | Valor | Nombre |
| --- | ---: | --- |
| `EstadoBus` | 0 | Disponible |
| `EstadoBus` | 1 | Asignado |
| `EstadoBus` | 2 | EnMantenimiento |
| `EstadoBus` | 3 | FueraDeServicio |
| `EstadoConductor` | 0 | Disponible |
| `EstadoConductor` | 1 | Asignado |
| `EstadoConductor` | 2 | Descanso |
| `EstadoConductor` | 3 | Inactivo |
| `DiaSemana` | 1–7 | Lunes–Domingo, en ese orden |

## Credenciales 
> Administrador
> 12345678


## Requisitos previos

La guía de la wiki indica que se requiere:

- Git.
- El SDK de .NET compatible con los proyectos.
- Node.js y npm.
- Un motor de base de datos relacional y una herramienta para administrarlo.
- Un editor de código.

La API está dirigida a .NET 10 y usa MySQL para la persistencia. Configura la cadena de conexión con los datos de tu servidor antes de ejecutar las operaciones que acceden a la base de datos.

## Instalación y ejecución

Clona el repositorio:

```bash
git clone https://github.com/jsyh1/Sistema-de-gesti-n-de-transporte.git
cd Sistema-de-gesti-n-de-transporte
```

Configura la conexión MySQL según las instrucciones siguientes antes de iniciar el backend. Los comandos para el cliente dependen de los scripts definidos en `client/package.json`.

### Conexión MySQL del backend

La API lee la cadena de conexión desde `ConnectionStrings:DefaultConnection`. En PowerShell, configúrala antes de iniciar la API (sustituye usuario y contraseña por los de tu servidor):

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=localhost;Port=3307;Database=ProgramacionBuses;User ID=TU_USUARIO;Password=TU_CONTRASENA;"
dotnet run --project .\SistemaTransporte.Api
```

En entorno de desarrollo, Swagger está disponible en `http://localhost:5267/swagger`.
La API ofrece operaciones CRUD para los modelos `Roles`, `Usuarios`, `Buses`,
`Conductores`, `Programaciones` y `ProgramacionesActivasBusDia`, bajo las rutas
`/api/Roles`, `/api/Usuarios`, `/api/Buses`, `/api/Conductores`,
`/api/Programaciones` y `/api/ProgramacionesActivasBusDia`. El modelo de
programación activa usa `BusId` y `DiaSemana` como clave compuesta.

### Configuración JWT

La API utiliza una clave secreta para firmar y validar los tokens JWT. Esta clave no debe almacenarse en el repositorio.

Desde la carpeta `SistemaTransporte.Api`, configura User Secrets:

```powershell
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "REEMPLAZAR_POR_UNA_CLAVE_ALEATORIA_DE_AL_MENOS_32_BYTES"
```

Si el proyecto ya tiene configurado `UserSecretsId`, no es necesario ejecutar nuevamente `dotnet user-secrets init`.

Cada desarrollador debe utilizar su propia clave local. No compartas claves JWT ni las subas a GitHub.

Una vez configurada la clave y la conexión a MySQL, ejecuta la API:

```powershell
dotnet run
```

## Pruebas previstas

El plan de pruebas propone cubrir:

- Pruebas unitarias de validaciones, disponibilidad y conflictos de horarios.
- Pruebas de integración entre controladores, servicios, repositorios y base de datos.
- Pruebas funcionales de los flujos de administrador y conductor.
- Pruebas de autenticación, autorización por rol y protección de datos.
- Pruebas de interfaz, validación de formularios y manejo de errores de conexión.
- Casos de solicitudes simultáneas que intenten asignar el mismo bus o conductor.

La wiki menciona xUnit o NUnit como opciones para pruebas unitarias; la herramienta definitiva debe coincidir con la configuración del proyecto.

## Seguridad

- No guardes contraseñas en texto plano ni publiques secretos, tokens o cadenas de conexión.
- Usa variables de entorno o un gestor de secretos para la configuración sensible.
- Protege las operaciones en el backend y valida la identidad y el rol en cada solicitud.
- Usa consultas parametrizadas y concede a la aplicación solo los permisos de base de datos necesarios.
- Valida los datos de entrada y evita exponer información sensible en errores o registros.
- Aplica las mismas comprobaciones de disponibilidad al crear y modificar programaciones.

Estas medidas son recomendaciones documentadas en la wiki y deben verificarse contra la implementación.

## Documentación

Consulta la [wiki del proyecto](https://github.com/jsyh1/Sistema-de-gesti-n-de-transporte/wiki) para el detalle de los documentos:

- [Guía de instalación y ejecución](https://github.com/jsyh1/Sistema-de-gesti-n-de-transporte/wiki/Gu%C3%ADa-de-instalaci%C3%B3n-y-ejecuci%C3%B3n)
- [Manual de usuario](https://github.com/jsyh1/Sistema-de-gesti-n-de-transporte/wiki/Manual-de-usuario)
- [Requisitos funcionales y no funcionales](https://github.com/jsyh1/Sistema-de-gesti-n-de-transporte/wiki/Requisitos-funcionales-y-no-funcionales)
- [Documentación de la API REST](https://github.com/jsyh1/Sistema-de-gesti-n-de-transporte/wiki/Documentaci%C3%B3n-de-la-API-REST)
- [Configuración y seguridad](https://github.com/jsyh1/Sistema-de-gesti-n-de-transporte/wiki/Configuraci%C3%B3n-y-seguridad)
- [Diccionario de datos](https://github.com/jsyh1/Sistema-de-gesti-n-de-transporte/wiki/Diccionario-de-datos)
- [Diagrama relacional](https://github.com/jsyh1/Sistema-de-gesti-n-de-transporte/wiki/Diagrama-Relaciional)
- [Plan de pruebas](https://github.com/jsyh1/Sistema-de-gesti-n-de-transporte/wiki/Plan-de-pruebas)
- [Matriz de trazabilidad de requisitos y pruebas](https://github.com/jsyh1/Sistema-de-gesti-n-de-transporte/wiki/Matriz-de-trazabilidad-de-requisitos-y-pruebas)
- Diagramas: [actividad](https://github.com/jsyh1/Sistema-de-gesti-n-de-transporte/wiki/Diagrama-de-actividad), [arquitectura](https://github.com/jsyh1/Sistema-de-gesti-n-de-transporte/wiki/Diagrama-de-Arquitectura), [casos de uso](https://github.com/jsyh1/Sistema-de-gesti-n-de-transporte/wiki/Diagrama-de-casos-de-uso), [clases de dominio](https://github.com/jsyh1/Sistema-de-gesti-n-de-transporte/wiki/Diagrama-de-Clases-%E2%80%90-Dominio), [navegación](https://github.com/jsyh1/Sistema-de-gesti-n-de-transporte/wiki/Diagrama-de-Navegacion), [negocio](https://github.com/jsyh1/Sistema-de-gesti-n-de-transporte/wiki/Diagrama-de-Negocio) y [secuencia](https://github.com/jsyh1/Sistema-de-gesti-n-de-transporte/wiki/Diagrama-de-secuencia).

## Decisiones pendientes

- **Motor de base de datos:** la guía de instalación menciona SQL Server; el diagrama relacional y su script propuesto están escritos para MySQL 8; la documentación de la API deja la elección abierta. Debe confirmarse cuál utiliza el código fuente antes de configurar o crear la base de datos.
- **Contrato de la API:** confirmar endpoints, campos JSON, códigos de respuesta y formato de `DiaSemana` y de los estados.
- **Autenticación:** confirmar el flujo JWT, duración y configuración de tokens sin incluir secretos en el repositorio.
- **Eliminaciones:** definir si serán físicas o lógicas y cómo se manejarán los recursos con asignaciones existentes.
- **Ejecución y pruebas:** documentar comandos y herramientas definitivos cuando estén presentes los proyectos y sus archivos de configuración.
