/**
 * Contratos de los endpoints de autenticación.
 * Se mantienen en un archivo aparte para que el servicio y los componentes
 * compartan los mismos tipos.
 */

/** Cuerpo de `POST /api/auth/register`. */
export interface RegisterRequest {
  email: string;
  password: string;
}

/** Cuerpo de `POST /api/auth/login`. */
export interface LoginRequest {
  email: string;
  password: string;
}

/** Respuesta 201 de `POST /api/auth/register`. */
export interface RegisteredUser {
  id: string;
  email: string;
  createdAt: string;
}

/** Respuesta 200 de `POST /api/auth/login`. */
export interface LoginResponse {
  token: string;
  expiresAt: string;
  email: string;
}
