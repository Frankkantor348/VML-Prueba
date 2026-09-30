import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LoginRequest, LoginResponse, RegisterRequest, RegisteredUser } from './auth.models';

/** Claves con las que se guarda la sesión en el navegador. */
const CLAVE_TOKEN = 'vml.token';
const CLAVE_CORREO = 'vml.correo';

/**
 * Único punto de acceso a los endpoints de autenticación.
 *
 * Los componentes no inyectan `HttpClient`: piden aquí. Así la URL de la API y el
 * manejo de la respuesta viven en un solo sitio.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl;

  // Uso signals para que la cabecera y la pantalla de sesión reaccionen solas al
  // iniciar o cerrar sesión. La aplicación es zoneless, así que el estado reactivo
  // es la forma natural de provocar el refresco de la vista.
  private readonly tokenSignal = signal<string | null>(localStorage.getItem(CLAVE_TOKEN));
  private readonly correoSignal = signal<string | null>(localStorage.getItem(CLAVE_CORREO));

  /** Token actual, o `null` si no hay sesión. */
  readonly token = this.tokenSignal.asReadonly();

  /** Correo de la sesión actual. */
  readonly correo = this.correoSignal.asReadonly();

  /** Indica si hay una sesión guardada. */
  readonly sesionIniciada = computed(() => this.tokenSignal() !== null);

  /** Crea la cuenta. La API responde 201, o 400/409 si algo no cumple. */
  register(datos: RegisterRequest): Observable<RegisteredUser> {
    return this.http.post<RegisteredUser>(`${this.apiUrl}/auth/register`, datos);
  }

  /** Valida las credenciales y, si son correctas, guarda el token JWT. */
  login(datos: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/auth/login`, datos).pipe(
      // El token solo se guarda cuando la petición salió bien; si la API responde
      // 401 el observable pasa por error y aquí no se toca nada.
      tap((respuesta) => this.guardarSesion(respuesta.token, respuesta.email)),
    );
  }

  /** Descarta la sesión. */
  logout(): void {
    this.guardarSesion(null, null);
  }

  private guardarSesion(token: string | null, correo: string | null): void {
    this.tokenSignal.set(token);
    this.correoSignal.set(correo);

    // Guardo también en localStorage para que la sesión sobreviva a un F5.
    if (token) {
      localStorage.setItem(CLAVE_TOKEN, token);
      localStorage.setItem(CLAVE_CORREO, correo ?? '');
      return;
    }

    localStorage.removeItem(CLAVE_TOKEN);
    localStorage.removeItem(CLAVE_CORREO);
  }
}
