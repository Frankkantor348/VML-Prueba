# Plan de ejecución — Prueba Técnica Full-Stack (VML-TEC-FS-001 v1.0)

**Empresa:** VML Colombia · **Código:** VML-TEC-FS-001 · **Modalidad:** Desarrollador Full-Stack
**Alcance:** Backend .NET (arquitectura hexagonal) + Angular + Flutter + despliegue en servicios gratuitos
**Tiempo estimado oficial:** 4 a 8 h de desarrollo + tiempo de despliegue
**Evaluador / envío final:** Manuel Yivan Rodríguez Carreño — Líder Técnico VML Colombia

---

## 1. Lo que realmente se evalúa (y lo que no)

| Criterio de la rúbrica | Traducción operativa |
|---|---|
| Separación de capas | `Domain` no referencia EF Core, ASP.NET ni Npgsql. Si esa referencia existe, se pierde el criterio. |
| Puertos y adaptadores | Interfaces en `Application` (entrada y salida) + implementaciones en `Infrastructure`/`Api`. No basta con nombrar carpetas. |
| Pruebas unitarias | Casos de uso de `Application` con mocks/fakes de los puertos. ≥ 1 éxito y ≥ 1 error por caso de uso. Sin BD real. |
| Errores y validaciones | Códigos HTTP correctos y consistentes en los 3 endpoints (200/503, 400/409, 200/401). |
| Calidad de código | Legible, consistente, sin duplicación evidente de lógica de negocio. |
| Integración real | Angular y Flutter hablan con la API desplegada. Prohibido mockear respuestas. |
| Despliegue | URLs públicas funcionando end-to-end + APK instalable. |

**Fuera de alcance (no invertir tiempo):** diseño visual, performance, routing complejo en Angular, cobertura exhaustiva de casos límite.
**Extra opcional que suma:** `/api/health` consumido desde Angular y pipeline CI/CD (GitHub Actions).

---

## 2. Decisiones técnicas y justificación

| Componente | Decisión | Por qué |
|---|---|---|
| Backend | .NET 8 (LTS), 4 proyectos + 1 de tests | Cumple la regla de dependencia de forma verificable: `Domain ← Application ← Infrastructure/Api`. |
| ORM | **EF Core + Npgsql** (no Dapper) | Permite `dotnet ef migrations` → el evaluador crea el esquema con un solo comando, y el test de "BD no disponible" sale limpio con `CanConnectAsync`. Queda justificado en el README. |
| Hash | BCrypt.Net-Next tras `IPasswordHasher` | Cambiar el algoritmo no toca los casos de uso (Open/Closed). |
| Token | JWT HS256 con secreto en variable de entorno | Cumple "token (JWT esperado)". |
| Errores | Middleware global de excepciones + `ProblemDetails` (RFC 7807) | Criterio "manejo de errores centralizado"; cero `try/catch` en controllers. |
| BD | **Neon** (PostgreSQL serverless) | Plan gratuito sin tarjeta, región AWS us-east-1 (latencia aceptable desde Colombia) + opción de BD local vía `docker-compose.yml` para el evaluador. |
| Backend hosting | **Render** Web Service desde `Dockerfile` | Despliegue directo desde GitHub; se documenta el arranque en frío (sleep por inactividad). |
| Frontend hosting | **Vercel** | Detecta Angular, HTTPS automático, rewrite SPA, sin configuración de `base-href`. |
| APK | **GitHub Releases** (opción recomendada por el documento) | Enlace estable y descargable, sin expiración. |
| CI/CD (opcional) | GitHub Actions: build + `dotnet test` en PR | Plus mencionado explícitamente en §5.5. |

**Regla de oro:** ningún secreto en el repositorio. `appsettings.json` con placeholders vacíos; valores reales en el panel del proveedor y en `user-secrets` en local.

---

## 3. Estructura del repositorio (monorepo)

```
App_Prueba/
├── README.md                      ← entregable obligatorio (§6)
├── docker-compose.yml             ← PostgreSQL local para el evaluador
├── .gitignore                     ← incluye appsettings.*.local.json, .env, build/
├── .github/workflows/ci.yml       ← opcional: build + tests
├── backend/
│   ├── VmlTest.sln
│   ├── Dockerfile
│   ├── src/
│   │   ├── VmlTest.Domain/              # User (entidad + reglas), DomainException, Email VO
│   │   ├── VmlTest.Application/         # Ports + UseCases + DTOs internos
│   │   ├── VmlTest.Infrastructure/      # EF Core, Npgsql, BCrypt, JwtTokenGenerator
│   │   └── VmlTest.Api/                 # Controllers, DTOs, Middleware, Program.cs (composition root)
│   └── tests/
│       └── VmlTest.Application.Tests/   # xUnit + Moq
├── frontend/                      # Proyecto Angular
└── mobile/                        # Proyecto Flutter
```

Relación de dependencias entre proyectos (a validar con `dotnet list reference`):

- `VmlTest.Domain` → *sin dependencias de proyecto*
- `VmlTest.Application` → `Domain`
- `VmlTest.Infrastructure` → `Application`
- `VmlTest.Api` → `Application` + `Infrastructure`
- `VmlTest.Application.Tests` → `Application` (+ `Domain`)

---

## 4. Cronograma sugerido (≈ 7,5 h)

| Fase | Entregable | Tiempo |
|---|---|---|
| 0 | Repo + solución + proyectos + `.gitignore` + `docker-compose.yml` | 30 min |
| 1 | `Domain` + `Application` (puertos, casos de uso, validaciones) | 1 h 30 |
| 2 | Tests de casos de uso (éxito + error por caso de uso) | 45 min |
| 3 | `Infrastructure` + `Api` + middleware + Swagger | 2 h |
| 4 | Angular (AuthService, register, login, sesión) | 1 h |
| 5 | Flutter (API client, 2 pantallas, sesión) | 1 h 30 |
| 6 | Despliegue: Neon → Render → Vercel → APK → CORS | 1 h 30 – 2 h |
| 7 | README + matriz de rúbrica + checklist + envío | 45 min |

Se puede paralelizar: mientras Render construye la imagen, se avanza en Flutter. La BD (Neon) se crea primero porque el resto depende de ella.

---

## 5. Módulo 1 — Backend .NET (hexagonal)

### 5.1 `Domain`
- `User`: `Id (Guid)`, `Email (string)`, `PasswordHash (string)`, `CreatedAt (DateTimeOffset UTC)`.
- Reglas de negocio aquí, no en los controllers: `Email.Create()` valida formato (regex/`MailAddress`) y longitud de contraseña (≥ 8).
- Excepciones propias: `DomainValidationException`, `DuplicateEmailException`, `InvalidCredentialsException`.
- Sin `using Microsoft.EntityFrameworkCore`, `System.Web`, ni atributos de serialización.

### 5.2 `Application` — puertos
```csharp
// Puertos de salida
public interface IUserRepository {
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct);
    Task AddAsync(User user, CancellationToken ct);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct);
}
public interface IPasswordHasher { string Hash(string plain); bool Verify(string plain, string hash); }
public interface ITokenGenerator { string Create(User user); }
public interface IDatabaseHealthChecker { Task<bool> CanConnectAsync(CancellationToken ct); }

// Puerto de entrada
public interface IRegisterUserUseCase { Task<RegisterResult> ExecuteAsync(string email, string password, CancellationToken ct); }
public interface ILoginUserUseCase    { Task<LoginResult> ExecuteAsync(string email, string password, CancellationToken ct); }
```
- `RegisterUserUseCase`: valida → verifica duplicado → hashea → persiste → devuelve DTO.
- `LoginUserUseCase`: busca → verifica hash → genera token → devuelve DTO.
- **No** devuelven la entidad de dominio: devuelven DTOs (`UserResponse`, `LoginResponse`) para no exponer el `PasswordHash`.

### 5.3 `Infrastructure`
- `EfCoreUserRepository` con `AppDbContext` (Npgsql) + índice único en `Email`.
- `BCryptPasswordHasher`, `JwtTokenGenerator`, `NpgsqlHealthChecker`.
- Migración inicial `InitialCreate`.

### 5.4 `Api` (adaptador de entrada)
| Endpoint | Éxito | Error |
|---|---|---|
| `GET /api/health` | `200 OK` + `{ status, database, timestamp }` | `503 Service Unavailable` si la BD no responde |
| `POST /api/auth/register` | `201 Created` | `400` email inválido/password corta · `409 Conflict` email duplicado |
| `POST /api/auth/login` | `200 OK` + `{ token, expiresAt }` | `400` payload inválido · `401 Unauthorized` credenciales inválidas |

- Controllers **solo orquestan**: reciben DTO → llaman al puerto de entrada → traducen el resultado a HTTP.
- `ExceptionHandlingMiddleware` mapea excepciones de dominio a códigos y responde `ProblemDetails`.
- CORS: origen leído de configuración (`Cors:AllowedOrigins`, separado por comas) para permitir el dominio de Vercel.
- `Program.cs` como composition root: `AddScoped<IUserRepository, EfCoreUserRepository>()`, etc.
- Swagger/OpenAPI habilitado: acelera la verificación del evaluador y sirve de evidencia.

### 5.5 Pruebas unitarias (`xUnit + Moq`)
Mínimo 2 por caso de uso, sin BD:
1. Registro exitoso → `ExistsByEmailAsync` false, `Hash` invocado, `AddAsync` invocado una vez.
2. Registro con email existente → excepción `DuplicateEmailException` y **no** se persiste.
3. Registro con email inválido / password corta → excepción de dominio.
4. Login exitoso → devuelve token no vacío.
5. Login con contraseña incorrecta → `InvalidCredentialsException`.
6. Login con usuario inexistente → mismo error que contraseña incorrecta (no filtrar si el email existe).

### 5.6 Mapeo SOLID → evidencia (para el README)
| Principio | Evidencia concreta en el código |
|---|---|
| SRP | Un caso de uso por clase; controllers sin lógica de negocio. |
| DIP | Casos de uso dependen de `IUserRepository`, `IPasswordHasher`; la inyección se configura en `Program.cs`. |
| OCP | Cambiar BCrypt por Argon2 = nueva clase que implementa `IPasswordHasher`, sin tocar `RegisterUserUseCase`. |
| Errores | Middleware único; sin `try/catch` en controllers. |
| DTOs | Entidades de dominio nunca salen del `Api`; entradas/salidas en DTOs. |
| Config sensible | `ConnectionStrings__Default`, `Jwt__Secret` por variables de entorno; `user-secrets` en local. |

---

## 6. Módulo 2 — Angular

- **AuthService** (`HttpClient` inyectado) — único punto de acceso HTTP:
  `register(email, password)` → `POST ${apiUrl}/api/auth/register`; `login(...)` → `POST /api/auth/login`; `logout()`; `getToken()`.
- **Componentes** (sin HTTP directo):
  - `RegisterComponent`: Reactive Forms con `Validators.required`, `Validators.email`, `minLength(8)`; muestra mensaje de éxito o el error de la API (409 → "El email ya está registrado").
  - `LoginComponent`: al éxito guarda el token en `localStorage` y navega a `SessionComponent`.
  - `SessionComponent`: confirma "sesión iniciada" y ofrece cerrar sesión.
- **Interceptor HTTP** (opcional, +): adjunta `Authorization: Bearer <token>`.
- **Ambientes**: `environment.ts` → `http://localhost:5000`, `environment.prod.ts` → URL de Render. Nunca hardcodear la URL en los servicios.
- Manejo de errores legible: leer `error.status` / `ProblemDetails.detail` y mostrar el mensaje.

---

## 7. Módulo 3 — Flutter

- `lib/services/api_client.dart`: encapsula `http`/`dio`, recibe `baseUrl` por constructor.
- `lib/services/auth_service.dart`: `register()` y `login()`; devuelve resultado tipado (éxito + token / fallo + mensaje).
- Pantallas: `RegisterScreen`, `LoginScreen`, `HomeScreen` (confirma la sesión).
- Token en memoria (suficiente según el documento) + `Navigator.pushReplacement` al `HomeScreen`.
- URL inyectada en compilación:
  ```bash
  flutter run --dart-define=API_URL=https://<api>.onrender.com
  flutter build apk --release --dart-define=API_URL=https://<api>.onrender.com
  ```
- Feedback con `SnackBar` para errores de la API (400/409/401) y éxito.
- HTTPS obligatorio: Android bloquea tráfico en texto plano → la URL de producción siempre `https://`.

---

## 8. Despliegue (obligatorio)

Orden recomendado, porque cada paso depende del anterior:

1. **GitHub** — repo público `vml-fullstack-test` con las tres carpetas y `.gitignore` revisado.
2. **Neon** — crear proyecto PostgreSQL, copiar la connection string (`sslmode=require`) → guardar el secreto en `user-secrets` local y en Render.
3. **Render (API)** — Web Service con `Dockerfile` (`mcr.microsoft.com/dotnet/aspnet:8.0`), `Root Directory = backend`.
   - Variables: `ConnectionStrings__Default`, `Jwt__Secret`, `Jwt__Issuer`, `Jwt__Audience`, `Cors__AllowedOrigins`, `ASPNETCORE_URLS=http://+:8080`, `ASPNETCORE_ENVIRONMENT=Production`.
   - Migraciones: `Database.MigrateAsync()` al arranque (idempotente) **y** script `backend/db/init.sql` documentado como alternativa.
   - Verificar `GET https://<api>.onrender.com/api/health` → `200`.
4. **Vercel (Angular)** — importar el repo, `Root Directory = frontend`, build `npm run build`, output `dist/<app>/browser`.
   - Actualizar `environment.prod.ts` con la URL de Render → redeploy.
   - Añadir el dominio de Vercel a `Cors__AllowedOrigins` en Render → redeploy de la API.
5. **Flutter → APK** — `flutter build apk --release --dart-define=API_URL=https://<api>.onrender.com` → subir `app-release.apk` a un Release de GitHub con tag `v1.0.0`.
6. **Verificación end-to-end** — probar registro y login desde la web desplegada y desde el APK instalado en un dispositivo real (no solo emulador).

Puntos que suelen romper el despliegue:
- **Arranque en frío** de Render: el primer request tarda; documentarlo en el README para que el evaluador espere.
- **Suspensión de Neon**: la primera conexión tras inactividad puede tardar; el `/api/health` debe tolerar el timeout y degradar a 503 sin tumbar el proceso.
- **CORS**: si falta el dominio exacto de Vercel (con `https://` y sin barra final), el navegador bloquea la llamada aunque la API responda 200.
- **`localhost` en el APK**: es el error más común; el APK de release debe apuntar a la URL HTTPS pública.
- **`base-href`**: solo aplica si se elige GitHub Pages; con Vercel no se necesita.

---

## 9. Matriz de rúbrica → evidencia (para el README y para el envío)

| Criterio | Dónde lo ve el evaluador |
|---|---|
| Separación de capas | Diagrama de carpetas + `dotnet list reference` mostrando que `Domain` no referencia nada. |
| Puertos y adaptadores | Interfaces en `Application/Ports` e implementaciones en `Infrastructure`/`Api`; `Program.cs` como composition root. |
| Pruebas unitarias | `dotnet test` con salida adjunta (6 pruebas: 3 registro + 3 login). |
| Errores y validaciones | Swagger + tabla de códigos HTTP en el README + `/api/health` devolviendo 503 con la BD caída. |
| Calidad de código | Un caso de uso por clase, DTOs, middleware único, sin duplicación. |
| Integración real | URLs de Vercel y APK apuntando a la API de Render, sin mocks. |
| Despliegue | Sección "URLs" del README con los 4 enlaces y captura de `/api/health` en 200. |

---

## 10. README.md — esqueleto obligatorio

```markdown
# Prueba Técnica Full-Stack — VML

## URLs en producción
| Componente | URL |
|---|---|
| API | https://... |
| Health | https://.../api/health |
| Frontend Angular | https://... |
| APK Android | https://github.com/.../releases/download/v1.0.0/app-release.apk |

## Stack y decisiones
- .NET 8 + arquitectura hexagonal (Domain / Application / Infrastructure / Api)
- PostgreSQL en Neon · API en Render · Angular en Vercel · APK en GitHub Releases
- ORM: EF Core + Npgsql (justificación)
- Nota: el servicio de Render se suspende por inactividad; el primer request puede tardar ~30-50 s.

## Ejecución local
### Base de datos
docker compose up -d postgres
### Backend
cd backend && dotnet run --project src/VmlTest.Api
### Pruebas
cd backend && dotnet test
### Frontend
cd frontend && npm ci && npm start
### Flutter
cd mobile && flutter run --dart-define=API_URL=https://<api>.onrender.com

## Migraciones / esquema
dotnet ef database update --project src/VmlTest.Infrastructure --startup-project src/VmlTest.Api

## Endpoints
| Método | Ruta | Respuestas |
|---|---|---|
| GET | /api/health | 200, 503 |
| POST | /api/auth/register | 201, 400, 409 |
| POST | /api/auth/login | 200, 400, 401 |

## Decisiones de diseño (3-5 líneas)
...Qué mejoraría con más tiempo: refresh tokens, tests de integración con Testcontainers,
rate limiting en el login, refresh del token en Angular, CI/CD completo.
```

---

## 11. Checklist previo al envío (del documento §8)

- [ ] `/api/health` responde `200` desde la URL pública.
- [ ] Registro y login funcionan desde el frontend Angular desplegado.
- [ ] El APK se instala en un Android real y completa registro + login contra la API desplegada.
- [ ] No hay secretos (connection string, clave JWT) en el repositorio — verificado con `git log -p | grep -i "password\|secret"`.
- [ ] `dotnet test` pasa en verde (adjuntar salida).
- [ ] README completo con las 5 secciones exigidas.
- [ ] CORS restringido al dominio del frontend (sin `AllowAnyOrigin`).
- [ ] Campos del formato de entrega completados: nombre, correo, fecha, repo, API, health, frontend, APK, proveedor BD, proveedor backend/frontend, observaciones.

---

## 12. Riesgos y mitigación

| Riesgo | Mitigación |
|---|---|
| El evaluador entra y la API está dormida → parece caída | Nota explícita en el README + `warm-up` con un cron gratuito (cron-job.org) cada 10 min (opcional). |
| Neon suspende la BD y el primer `/api/health` da 503 | Reintento con timeout de 5 s en `NpgsqlHealthChecker`; documentar. |
| Migraciones fallan al arrancar en Render (concurrencia) | `Database.MigrateAsync()` en un bloque de arranque con reintento y log; script SQL como plan B. |
| El APK apunta a `localhost` | Verificar `--dart-define` en el build de release y probar el APK instalado antes de subirlo. |
| CORS mal configurado | Probar desde el navegador (no desde Postman, que no aplica CORS) antes de enviar. |
| Se acaba el tiempo en diseño visual | Es fuera de alcance: usar el mínimo CSS, priorizar integración y hexagonal. |
