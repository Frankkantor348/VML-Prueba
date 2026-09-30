import '../models/auth_models.dart';
import 'api_client.dart';

/// Operaciones de autenticación que ofrece la API.
///
/// Es el único punto de la aplicación que habla con el backend: las pantallas
/// piden aquí y no construyen peticiones.
class AuthService {
  AuthService({ApiClient? cliente}) : _cliente = cliente ?? ApiClient();

  final ApiClient _cliente;

  /// Crea la cuenta. La API responde 201, o lanza `ApiException` con 400 o 409.
  Future<UsuarioRegistrado> registrar(String email, String password) async {
    final json = await _cliente.post('/auth/register', {
      'email': email,
      'password': password,
    });

    return UsuarioRegistrado.fromJson(json);
  }

  /// Valida las credenciales y devuelve la sesión con el token JWT.
  ///
  /// El token se mantiene en memoria: la guía de la prueba lo considera
  /// suficiente, y la pantalla de sesión lo recibe por parámetro.
  Future<Sesion> iniciarSesion(String email, String password) async {
    final json = await _cliente.post('/auth/login', {
      'email': email,
      'password': password,
    });

    return Sesion.fromJson(json);
  }

  /// Libera las conexiones del cliente HTTP.
  void dispose() => _cliente.dispose();
}
