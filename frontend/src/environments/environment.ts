/**
 * Configuración del entorno de desarrollo.
 *
 * `ng serve` usa este archivo. La compilación de producción lo sustituye por
 * `environment.prod.ts` mediante el `fileReplacements` de `angular.json`, así que
 * ningún servicio lleva la URL escrita a mano.
 */
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5065/api',
};
