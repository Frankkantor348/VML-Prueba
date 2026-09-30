/// Contratos de los endpoints de autenticación.
/// Igual que en el cliente web, se mantienen en un archivo propio para que el
/// servicio y las pantallas compartan los mismos tipos.
library;

/// Respuesta 201 de `POST /api/auth/register`.
class UsuarioRegistrado {
  const UsuarioRegistrado({
    required this.id,
    required this.email,
    required this.createdAt,
  });

  factory UsuarioRegistrado.fromJson(Map<String, dynamic> json) => UsuarioRegistrado(
        id: json['id'] as String,
        email: json['email'] as String,
        createdAt: json['createdAt'] as String,
      );

  final String id;
  final String email;
  final String createdAt;
}

/// Respuesta 200 de `POST /api/auth/login`.
class Sesion {
  const Sesion({
    required this.token,
    required this.expiresAt,
    required this.email,
  });

  factory Sesion.fromJson(Map<String, dynamic> json) => Sesion(
        token: json['token'] as String,
        expiresAt: json['expiresAt'] as String,
        email: json['email'] as String,
      );

  /// Token JWT que devuelve la API.
  final String token;

  /// Momento en el que el token deja de ser válido.
  final String expiresAt;

  final String email;

  /// Vista corta del token, para confirmar que llegó sin exponerlo entero.
  String get tokenRecortado => token.length <= 28
      ? token
      : '${token.substring(0, 22)}...${token.substring(token.length - 6)}';
}
