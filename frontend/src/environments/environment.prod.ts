/**
 * Configuración del entorno de producción.
 *
 * Antes de compilar para desplegar hay que reemplazar `apiUrl` por la dirección
 * HTTPS de la API publicada (por ejemplo la de Render). Es la única línea que
 * cambia entre local y nube.
 */
export const environment = {
  production: true,
  // Sustituir por la URL real de la API desplegada, sin barra final.
  apiUrl: 'https://REEMPLAZAR-URL-DE-LA-API/api',
};
