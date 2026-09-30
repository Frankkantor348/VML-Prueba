/// Configuración de la API.
///
/// La URL no se escribe en el código: llega en tiempo de compilación con
/// `--dart-define=API_URL=...`. El valor por defecto sirve para el emulador o para
/// un teléfono conectado por cable con `adb reverse tcp:5065 tcp:5065`.
class ApiConfig {
  const ApiConfig._();

  static const String baseUrl = String.fromEnvironment(
    'API_URL',
    defaultValue: 'http://localhost:5065/api',
  );

  /// Tiempo máximo de espera de una petición.
  ///
  /// Los planes gratuitos suspenden el servicio por inactividad, así que la
  /// primera llamada del día puede tardar bastante más que las siguientes.
  static const Duration tiempoDeEspera = Duration(seconds: 30);
}
