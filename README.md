# Prueba Técnica Full-Stack — VML Colombia

**Código del documento:** VML-TEC-FS-001 v1.0
Implementación de la prueba técnica: API .NET con arquitectura hexagonal (puertos y
adaptadores), cliente web Angular, app móvil Flutter y despliegue en servicios gratuitos.

> **Estado del repositorio**
> - [x] Backend .NET (Domain, Application, Infrastructure, Api) + pruebas unitarias
> - [x] Frontend Angular
> - [ ] App móvil Flutter (APK)
> - [ ] Despliegue en la nube
>
> Este README se completa a medida que avanza cada módulo.

---

## 1. Stack

| Componente | Tecnología | Versión |
|---|---|---|
| Backend | .NET (ASP.NET Core) | 10.0 (LTS) |
| Persistencia | Entity Framework Core + Npgsql | 10.0 |
| Base de datos | PostgreSQL | 17 (docker) / 16+ en la nube |
| Hashing | BCrypt.Net-Next | 4.x |
| Tokens | JWT firmado con HS256 | — |
| Documentación | Swagger (Swashbuckle) | 10.x |
| Pruebas | xUnit + Moq | 2.9 / 4.21 |

---

## 2. Arquitectura hexagonal (puertos y adaptadores)

El servicio está partido en cuatro capas y la regla de dependencia apunta siempre
hacia adentro:

```
        ┌───────────────────────────────────────────────┐
        │  Api  (adaptador de entrada HTTP)             │
        │  controllers · DTOs · middleware · DI         │
        └───────────────┬───────────────────────────────┘
                        │
        ┌───────────────▼───────────────────────────────┐
        │  Application                                  │
        │  casos de uso · puertos de entrada/salida     │
        └───────────────┬───────────────────────────────┘
                        │
        ┌───────────────▼───────────────────────────────┐
        │  Domain  — sin dependencias de frameworks     │
        │  User · Email · PasswordPolicy · excepciones   │
        └───────────────────────────────────────────────┘

        Infrastructure  →  implementa los puertos de salida
        (EF Core/Npgsql · BCrypt · JWT · health)
```

| Capa | Responsabilidad | Depende de |
|---|---|---|
| `VmlTest.Domain` | Entidades y reglas de negocio. Cero referencias a EF Core, ASP.NET o Npgsql. | nada |
| `VmlTest.Application` | Casos de uso (registro y login), puertos de entrada y de salida. | Domain |
| `VmlTest.Infrastructure` | Implementación de los puertos de salida: repositorio PostgreSQL, hashing, tokens, verificación de la base. | Application |
| `VmlTest.Api` | Adaptador de entrada HTTP: controllers, DTOs, middleware de errores y *composition root*. | Application, Infrastructure |
| `VmlTest.Application.Tests` | Pruebas unitarias de los casos de uso con dobles de prueba. | Application |

**Comprobación de la regla de dependencia:**

```bash
cd backend
dotnet list src/VmlTest.Domain/VmlTest.Domain.csproj reference    # no debe listar nada
dotnet list src/VmlTest.Application/VmlTest.Application.csproj reference
```

### Puertos

| Tipo | Puerto | Implementación |
|---|---|---|
| Entrada | `IRegisterUserUseCase` | `RegisterUserUseCase` |
| Entrada | `ILoginUserUseCase` | `LoginUserUseCase` |
| Salida | `IUserRepository` | `EfCoreUserRepository` (EF Core + Npgsql) |
| Salida | `IPasswordHasher` | `BCryptPasswordHasher` |
| Salida | `ITokenGenerator` | `JwtTokenGenerator` |
| Salida | `IDatabaseHealthChecker` | `DatabaseHealthChecker` |

El **composition root** está en `VmlTest.Api/Program.cs`: es el único archivo donde se
decide qué implementación concreta usa cada puerto.

---

## 3. Estructura del repositorio

```
.
├── README.md
├── docker-compose.yml                  PostgreSQL local
├── .gitignore
└── backend/
    ├── VmlTest.sln                     solución (abrir con Visual Studio)
    ├── VmlTest.slnLaunch               perfiles de arranque para Visual Studio
    ├── Dockerfile                      imagen para el despliegue
    ├── db/schema.sql                   script SQL idempotente (plan B de migraciones)
    ├── src/
    │   ├── VmlTest.Domain/
    │   │   ├── Users/                  User · Email · PasswordPolicy
    │   │   └── Exceptions/             DomainValidation · DuplicateEmail · InvalidCredentials
    │   ├── VmlTest.Application/
    │   │   ├── Abstractions/           puertos de salida
    │   │   ├── Exceptions/             DatabaseUnavailableException
    │   │   └── Users/                  Register/ · Login/
    │   ├── VmlTest.Infrastructure/
    │   │   ├── Persistence/            AppDbContext · configuraciones · repositorio · migraciones
    │   │   ├── Security/               BCrypt · JWT · opciones
    │   │   └── Health/                 verificador de la base
    │   └── VmlTest.Api/
    │       ├── Controllers/            HealthController · AuthController
    │       ├── Contracts/              DTOs de entrada y salida
    │       ├── Extensions/             CORS · Swagger · autenticación
    │       ├── Middleware/             manejo centralizado de errores
    │       └── VmlTest.Api.http        peticiones de ejemplo
    └── tests/
        └── VmlTest.Application.Tests/  xUnit + Moq
```

---

## 4. Requisitos previos

- **.NET SDK 10** (`dotnet --version` → `10.x`)
- **Visual Studio Community 2026** (o superior) con la carga de trabajo *ASP.NET and web development*
- **PostgreSQL**: cualquiera de estas opciones
  - Docker + `docker compose up -d postgres` (recomendado, no requiere instalar nada)
  - Una instancia propia (pgAdmin, Neon, Supabase, Aiven…)
- **Node.js 20+** y **Flutter 3.x** para los módulos de frontend y móvil

---

## 5. Cómo abrir y ejecutar el backend

### 5.1 Visual Studio Community

1. Abrir el archivo de solución **`backend\VmlTest.sln`**.
2. Visual Studio detecta el perfil de arranque y deja **`VmlTest.Api`** como proyecto de inicio.
   Si no lo hace, hacer clic derecho sobre `VmlTest.Api` → *Establecer como proyecto de inicio*.
3. Configurar la base de datos (sección 6).
4. Presionar **F5**. La API abre en `http://localhost:5065` y Swagger queda en
   `http://localhost:5065/swagger`.

### 5.2 Terminal

```bash
# 1. Base de datos local
docker compose up -d postgres

# 2. API
cd backend
dotnet run --project src/VmlTest.Api
```

### 5.3 Visual Studio Code (frontend y móvil)

VS Code se usa para el **frontend Angular** y la **app Flutter**, no para el backend:

```bash
# Frontend Angular
cd frontend
npm install          # o npm ci para una instalación limpia
npm start            # http://localhost:4200
```

```bash
# App Flutter en un teléfono Android conectado por cable
cd mobile

# 1. Puente USB: el teléfono alcanza la API del PC como si fuera local
adb reverse tcp:5065 tcp:5065

# 2. Compilar, instalar y ejecutar
flutter run --dart-define=API_URL=http://localhost:5065/api
```

El puente `adb reverse` se pierde al desconectar el cable o al reiniciar `adb`:
si la app deja de conectar, vuelve a ejecutarlo.

El detalle del módulo web está en `frontend/README.md`.

---

## 6. Base de datos y migraciones

### 6.1 Opción A — PostgreSQL local con Docker

```bash
docker compose up -d postgres
```

Crea la base `vmltest` con usuario `vml` / contraseña `vml`, y ya coincide con la cadena
de conexión de `appsettings.Development.json`.

### 6.2 Opción B — Base propia (pgAdmin, Neon, Supabase…)

Crear una base llamada `vmltest` y guardar la cadena de conexión fuera del repositorio:

```bash
cd backend
dotnet user-secrets init --project src/VmlTest.Api
dotnet user-secrets set "ConnectionStrings:Default" "Host=...;Port=5432;Database=vmltest;Username=...;Password=...;SSL Mode=Require" --project src/VmlTest.Api
```

`user-secrets` guarda los valores en el perfil del usuario, nunca en el repositorio.

### 6.3 Aplicar el esquema

La API aplica las migraciones **automáticamente al arrancar** (con reintentos). Si se
prefiere hacerlo a mano:

```bash
cd backend
dotnet ef database update --project src/VmlTest.Infrastructure --startup-project src/VmlTest.Api
```

Alternativa sin herramientas de EF: ejecutar `backend/db/schema.sql` (idempotente) sobre
la base.

La tabla creada es exactamente la que pide la prueba:

```
Users
├── Id            uuid          PK
├── Email         varchar(254)  UNIQUE (IX_Users_Email)
├── PasswordHash  varchar(200)
└── CreatedAt     timestamptz
```

---

## 7. Endpoints

| Método | Ruta | Éxito | Errores |
|---|---|---|---|
| `GET` | `/api/health` | `200 OK` — `{ status: "Healthy", database: "Connected" }` | `503` si la base no responde |
| `POST` | `/api/auth/register` | `201 Created` — usuario creado | `400` datos inválidos · `409` correo ya registrado · `503` base no disponible |
| `POST` | `/api/auth/login` | `200 OK` — `{ token, expiresAt, email }` | `400` datos inválidos · `401` credenciales incorrectas · `503` base no disponible |

Los errores se devuelven siempre en formato **ProblemDetails (RFC 7807)**:

```json
{
  "title": "Correo ya registrado",
  "status": 409,
  "detail": "Ya existe una cuenta registrada con el correo 'candidato@vml.com'.",
  "instance": "/api/auth/register",
  "traceId": "0HNOV4A3SM6CD:00000001"
}
```

### Ejemplos

```bash
# Health
curl http://localhost:5065/api/health

# Registro -> 201
curl -X POST http://localhost:5065/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"candidato@vml.com","password":"ClaveSegura123"}'

# Registro repetido -> 409
curl -X POST http://localhost:5065/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"candidato@vml.com","password":"ClaveSegura123"}'

# Login -> 200 con token
curl -X POST http://localhost:5065/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"candidato@vml.com","password":"ClaveSegura123"}'

# Login con clave incorrecta -> 401
curl -X POST http://localhost:5065/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"candidato@vml.com","password":"OtraClave123"}'
```

También hay peticiones listas para ejecutar en `backend/src/VmlTest.Api/VmlTest.Api.http`
(se pueden lanzar desde Visual Studio, VS Code o Rider).

---

## 8. Pruebas unitarias

```bash
cd backend
dotnet test
```

Resultado actual:

```
Passed!  - Failed: 0, Passed: 19, Skipped: 0, Total: 19
```

Las pruebas cubren los dos casos de uso de `Application` con dobles de prueba (Moq) para
los puertos de salida: **no tocan PostgreSQL, BCrypt ni la red**.

| Caso de uso | Casos cubiertos |
|---|---|
| `RegisterUserUseCase` | registro correcto (se guarda el hash, nunca la clave) · correo duplicado (no persiste) · 7 correos inválidos · 4 contraseñas que no cumplen la política · normalización a minúsculas |
| `LoginUserUseCase` | login correcto (devuelve el token) · contraseña incorrecta (no emite token) · correo no registrado (mismo error) · contraseña vacía · correo mal formado |

---

## 9. Decisiones de diseño

- **Arquitectura hexagonal real, no decorativa.** `Domain` no tiene ni una referencia de
  paquete. Las reglas de negocio (formato de correo, política de contraseña, unicidad de
  la cuenta) están en el dominio, y las validaciones de los DTOs no las duplican: el
  dominio es la única fuente de verdad.
- **EF Core en lugar de Dapper.** El esquema se versiona con migraciones, que quedan
  versionadas junto al código y las aplica la propia API al arrancar. Dapper habría dado
  consultas algo más rápidas, pero a cambio de mantener el esquema a mano; con una sola
  tabla y un despliegue que debe quedar listo sin intervención, las migraciones pesan más.
- **.NET 10 (LTS).** El documento no fija versión. Se usa la LTS vigente, que además es
  la que tienen instalada el entorno de desarrollo y la imagen base del `Dockerfile`.
- **BCrypt detrás de un puerto.** `IPasswordHasher` permite cambiar a Argon2 escribiendo un
  adaptador nuevo, sin tocar `RegisterUserUseCase` (abierto/cerrado).
- **Errores centralizados.** Un único `ExceptionHandlingMiddleware` traduce excepciones a
  códigos HTTP y a `ProblemDetails`. No hay un solo `try/catch` en los controllers.
- **La base caída no es un error de la petición.** El repositorio traduce los fallos de
  conexión a `DatabaseUnavailableException` y el middleware responde `503`, igual que
  `/api/health`. Así la capa HTTP no necesita conocer Npgsql.
- **El índice único es la última defensa.** El caso de uso verifica el correo antes de
  insertar, y si dos registros simultáneos se cruzan, el `SQLSTATE 23505` de PostgreSQL se
  traduce a `409` en lugar de convertirse en un `500`.
- **DTOs en la frontera.** Las entidades de dominio nunca se serializan: `UserResponse` no
  expone `PasswordHash`.
- **Secretos fuera del repositorio.** `appsettings.json` no tiene valores; en local se usan
  `user-secrets` y en la nube variables de entorno del proveedor. La única excepción es
  `appsettings.Development.json`, que trae valores locales de juguete (base de datos del
  `docker-compose` y una clave JWT identificada como tal) para que `F5` funcione de una vez.
- **Manejo de la suspensión de la base.** Las migraciones se reintentan al arrancar y
  `/api/health` degrada a `503` en lugar de caer, algo necesario con planes gratuitos que
  suspenden la instancia por inactividad.

### Verificación realizada

Todo lo de la tabla se ejecutó realmente; no hay resultados supuestos.

| Prueba | Resultado |
|---|---|
| `dotnet build` de la solución completa | 0 errores, 0 warnings |
| `dotnet test` | 19/19 pruebas en verde |
| `dotnet ef migrations add InitialCreate` | tabla `Users` con las 4 columnas + índice único |

**Casos de error (sin depender de una base disponible):**

| Prueba | Resultado |
|---|---|
| `GET /api/health` con la base inalcanzable | `503` con `{"status":"Unhealthy","database":"Unavailable"}` |
| `POST /api/auth/register` con correo mal formado | `400` "El correo electrónico no tiene un formato válido." |
| `POST /api/auth/register` con contraseña de 5 caracteres | `400` "La contraseña debe tener al menos 8 caracteres." |
| `POST /api/auth/register` con cuerpo `{}` | `400` "El correo electrónico es obligatorio." |
| `POST /api/auth/register` / `login` con la base inalcanzable | `503` "No fue posible comunicarse con la base de datos." |

**Flujo completo contra PostgreSQL 17.11 real** (contenedor del `docker-compose.yml`):

| Prueba | Resultado |
|---|---|
| `GET /api/health` | `200` `{"status":"Healthy","database":"Connected"}` |
| `POST /api/auth/register` | `201` con `id`, `email`, `createdAt` |
| `POST /api/auth/register` repetido con `Candidato@VML.com` | `409` — y el correo quedó guardado en minúsculas, que es lo que prueba la normalización |
| `POST /api/auth/login` | `200` con un JWT firmado (HS256) cuyo payload trae `sub`, `email`, `jti`, `nbf`, `exp`, `iss`, `aud` |
| `POST /api/auth/login` con contraseña incorrecta | `401` "El correo o la contraseña no son correctos." |
| `POST /api/auth/login` con correo no registrado | `401` con el mismo mensaje que el caso anterior |
| Contenido de la base | una fila, `PasswordHash` de 60 caracteres empezando por `$2a$12$` (BCrypt, factor 12) y `CreatedAt` en `timestamptz` |
| SQL generado por EF Core | `WHERE u."Email" = @email` — el objeto de valor `Email` se traduce correctamente a la columna |

**Frontend Angular:**

| Prueba | Resultado |
|---|---|
| `npm ci` limpio (borrando antes `node_modules`) | 472 paquetes instalados sin errores |
| `ng test` | 1/1 prueba en verde (`debería crear el componente raíz`) |
| `ng build` de producción | compilación correcta en 32 s, 0 avisos |
| `ng serve` en `http://localhost:4200` | sirve la aplicación; el bundle incluye la URL de la API y las rutas `/auth/register` y `/auth/login` |
| Preflight CORS desde `http://localhost:4200` | `204` con `Access-Control-Allow-Origin: http://localhost:4200` |
| Recorrido completo en el navegador (Angular → API → PostgreSQL) | verificado: el registro crea la fila en `Users` y el login muestra el JWT en la pantalla de sesión |
| Correspondencia del token con la base | el `sub` del JWT coincide con el `Id` real de la fila en `Users`; `exp − nbf` = 60 min |

---

## 10. Despliegue

### 10.1 URLs públicas

| Componente | Proveedor | URL |
|---|---|---|
| API | Render (Web Service con Docker) | https://vml-prueba-api.onrender.com |
| Health check | Render | https://vml-prueba-api.onrender.com/api/health |
| Base de datos | Render PostgreSQL | red interna |
| Frontend Angular | Vercel | _pendiente_ |
| APK Android | GitHub Releases | _pendiente_ |

### 10.2 Orden de despliegue

La API y el frontend se necesitan mutuamente: el frontend no sabe a qué API llamar y la
API no sabe qué dominio autorizar por CORS. Por eso el orden importa.

1. **API y base de datos a la vez.** El repositorio incluye `render.yaml`, un *blueprint*
   que crea los dos recursos y los conecta. En el panel de Render: `New +` → `Blueprint`
   → elegir el repositorio → `Apply`. Render aprovisiona la base, construye la imagen
   desde `backend/Dockerfile` y genera por su cuenta la clave `Jwt__Secret`.
2. **`Cors__AllowedOrigins` queda pendiente** (`sync: false` en el blueprint) porque el
   dominio del frontend todavía no existe.
3. **Frontend en Vercel.** Importar el repositorio con *Root Directory* = `frontend`.
   Antes, poner la URL de la API en `src/environments/environment.prod.ts`.
4. **Cerrar el círculo.** Copiar la URL de Vercel en `Cors__AllowedOrigins` de Render.
   Guardar una variable reinicia el servicio.
5. **APK de release.** Con la URL HTTPS ya disponible, compilar y adjuntar el APK a un
   *release* de GitHub (10.4).

### 10.3 Variables de entorno del servicio

| Variable | Valor |
|---|---|
| `ConnectionStrings__Default` | la inyecta Render desde la base (`fromDatabase`). Llega en formato URI y la API la normaliza al arrancar |
| `Jwt__Secret` | generada por Render (`generateValue: true`), nunca escrita en el repositorio |
| `Jwt__Issuer` / `Jwt__Audience` | `VmlTest.Api` / `VmlTest.Clients` |
| `Cors__AllowedOrigins` | dominio del frontend; varios separados por comas |
| `ASPNETCORE_ENVIRONMENT` | `Production` |

`ASPNETCORE_URLS` no se configura: la API toma el puerto de la variable `PORT` que asigna
el proveedor y, si no existe (por ejemplo en un `docker run` local), usa el 8080 que trae
el `Dockerfile`.

### 10.4 APK de release

```bash
cd mobile
flutter build apk --release --dart-define=API_URL=https://<api>.onrender.com/api
```

El APK de release **exige HTTPS**: el permiso para tráfico sin cifrar está declarado solo
en `android/app/src/debug/AndroidManifest.xml`, así que la build de producción sigue
bloqueando el HTTP en claro, como corresponde.

### 10.5 Cosas que conviene saber antes de desplegar

- **Arranque en frío.** El plan gratuito de Render suspende el servicio por inactividad: el
  primer request puede tardar medio minuto. Está documentado aquí a propósito, para que el
  evaluador no lo confunda con una caída.
- **El esquema se crea solo.** La API aplica las migraciones de EF Core al arrancar, con
  reintentos. No hace falta ejecutar nada sobre la base desplegada.
- **La base gratuita de Render vence.** El plan gratuito de PostgreSQL caduca a los 30 días;
  el propio documento de la prueba lo advierte. Si se vence, hay que crear otra y volver a
  desplegar; basta con actualizar `ConnectionStrings__Default`.
- **Medir el peso del repositorio.** Ya está verificado: con el `.gitignore` del proyecto se
  suben 133 archivos y 0,5 MB, no los 2,5 GB de `node_modules` y `mobile/build`.

### Pintar el estado del despliegue

Cuando la API esté arriba, esta debería ser la comprobación mínima antes de avisar al
evaluador:

```bash
curl https://<api>.onrender.com/api/health
# {"status":"Healthy","database":"Connected", ...}
```

---

## 11. Qué mejoraría con más tiempo

Manejo de la ventana de expiración del token (token de refresco), pruebas de integración
con Testcontainers sobre PostgreSQL real, límite de intentos en el login para frenar la
fuerza bruta, rotación del secreto JWT y un pipeline de CI/CD que ejecute las pruebas y
publique la API y el APK automáticamente.
