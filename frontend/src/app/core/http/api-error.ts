import { HttpErrorResponse } from '@angular/common/http';

/**
 * Formato de error que devuelve la API: ProblemDetails (RFC 7807).
 * Los tres endpoints lo usan, así que un solo lector sirve para todos.
 */
export interface ProblemDetails {
  status?: number;
  title?: string;
  detail?: string;
  traceId?: string;
}

/**
 * Traduce el error de una petición a un mensaje que se le pueda mostrar al
 * usuario. La API ya redacta `detail` en español, así que se aprovecha tal cual
 * en lugar de inventar un mensaje genérico.
 */
export function describeApiError(error: unknown): string {
  if (error instanceof HttpErrorResponse) {
    // status 0 significa que no hubo respuesta: la API está apagada, la URL está
    // mal o no hay red. Es el fallo más común en local.
    if (error.status === 0) {
      return 'No se pudo contactar con la API. Verifica que esté encendida y que la URL sea la correcta.';
    }

    const problema = error.error as ProblemDetails | null;
    if (problema?.detail) {
      return problema.detail;
    }

    return `La API respondió con un error ${error.status}.`;
  }

  return 'Ocurrió un error inesperado. Intenta de nuevo.';
}
