/**
 * Configuración del entorno de producción.
 *
 * `ng build` (sin configuración) sustituye `environment.ts` por este archivo, así que
 * la URL de la API desplegada se declara una sola vez, aquí.
 */
export const environment = {
  production: true,
  // API desplegada en Render, sin barra final.
  apiUrl: 'https://vml-prueba-api.onrender.com/api',
};
