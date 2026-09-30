/// Error de la API con un mensaje ya listo para mostrar en pantalla.
///
/// El servicio lo lanza en lugar de dejar salir excepciones técnicas, para que las
/// pantallas no tengan que interpretar códigos de estado ni excepciones de red.
class ApiException implements Exception {
  const ApiException(this.mensaje);

  final String mensaje;

  @override
  String toString() => mensaje;
}
