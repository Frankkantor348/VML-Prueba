# Frontend Angular — Prueba Técnica Full-Stack

Cliente web que consume los endpoints de registro y login de la API .NET.

## Requisitos

- Node.js 20+ (probado con 22.20) y npm 10+
- La API corriendo (por defecto en `http://localhost:5065`)

## Puesta en marcha

```bash
npm install      # o npm ci para una instalación limpia
npm start        # ng serve -> http://localhost:4200
```

Abre `http://localhost:4200`. La raíz redirige a `/login`.

> El proyecto trae un `.npmrc` con `legacy-peer-deps=true` porque npm 10.9.x falla
> al resolver las *peer dependencies* de vitest. Sin esa opción `npm install`
> termina con `Cannot read properties of null (reading 'edgesOut')`.

## Configuración de la API

La URL no está escrita en el código: sale de los archivos de entorno.

| Archivo | Cuándo se usa | Valor |
|---|---|---|
| `src/environments/environment.ts` | `ng serve` y `ng build --configuration development` | `http://localhost:5065/api` |
| `src/environments/environment.prod.ts` | `ng build` (producción) | URL de la API desplegada (hay que reemplazarla) |

El reemplazo lo hace Angular mediante `fileReplacements` en `angular.json`, así que
ningún servicio consulta la URL a mano.

Recuerda que el dominio donde se publique este frontend debe estar autorizado en el
CORS de la API (`Cors__AllowedOrigins`).

## Estructura

```
src/
├── environments/                 URLs por ambiente
└── app/
    ├── core/
    │   ├── auth/
    │   │   ├── auth.service.ts   único punto de acceso a la API
    │   │   └── auth.models.ts    contratos de entrada y salida
    │   └── http/api-error.ts     traduce los ProblemDetails a mensajes
    ├── features/
    │   ├── login/                formulario de inicio de sesión
    │   ├── register/             formulario de registro
    │   └── session/              confirma la sesión y muestra el token
    ├── app.config.ts             provideRouter + provideHttpClient
    └── app.routes.ts             /login, /register y /session
```

Los componentes no inyectan `HttpClient`: todo el tráfico pasa por `AuthService`.
La aplicación es *zoneless*, así que el estado se maneja con señales (`signal`).

## Comandos

| Comando | Qué hace |
|---|---|
| `npm start` | Servidor de desarrollo en `http://localhost:4200` |
| `npm run build` | Compila para producción en `dist/` |
| `npm test` | Pruebas unitarias con vitest |
