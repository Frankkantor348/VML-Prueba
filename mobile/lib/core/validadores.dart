/// Reglas de validación de los formularios.
///
/// Viven en un solo sitio para que el registro y el inicio de sesión no se
/// desincronicen, y coinciden a propósito con las que aplica el dominio en el
/// backend: correo con formato válido y contraseña de al menos 8 caracteres.
class Validadores {
  const Validadores._();

  /// Longitud mínima de la contraseña, igual que `PasswordPolicy` en el backend.
  static const int longitudMinimaContrasena = 8;

  // Validación pragmática: texto antes y después de la arroba y un punto en el
  // dominio. No pretende implementar la RFC 5322 completa.
  static final RegExp _patronCorreo = RegExp(r'^[^@\s]+@[^@\s.]+\.[^@\s.]+$');

  static String? correo(String? valor) {
    final texto = valor?.trim() ?? '';

    if (texto.isEmpty) {
      return 'El correo es obligatorio.';
    }

    if (!_patronCorreo.hasMatch(texto)) {
      return 'Escribe un correo con formato válido.';
    }

    return null;
  }

  static String? contrasena(String? valor) {
    if (valor == null || valor.isEmpty) {
      return 'La contraseña es obligatoria.';
    }

    if (valor.length < longitudMinimaContrasena) {
      return 'Debe tener al menos $longitudMinimaContrasena caracteres.';
    }

    return null;
  }
}
