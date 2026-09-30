import 'package:flutter/material.dart';

import '../core/api_exception.dart';
import '../core/validadores.dart';
import '../services/auth_service.dart';
import 'home_screen.dart';
import 'register_screen.dart';

/// Pantalla de inicio de sesión.
///
/// Si la API valida las credenciales, navega a la pantalla de sesión. Si responde
/// 401, muestra el mensaje que envió el backend.
class LoginScreen extends StatefulWidget {
  const LoginScreen({super.key});

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final _formulario = GlobalKey<FormState>();
  final _correo = TextEditingController();
  final _contrasena = TextEditingController();
  final _auth = AuthService();

  bool _enviando = false;

  @override
  void dispose() {
    _correo.dispose();
    _contrasena.dispose();
    _auth.dispose();
    super.dispose();
  }

  Future<void> _ingresar() async {
    if (!_formulario.currentState!.validate()) {
      return;
    }

    setState(() => _enviando = true);

    try {
      final sesion = await _auth.iniciarSesion(_correo.text.trim(), _contrasena.text);

      if (!mounted) return;
      // El token viaja a la pantalla de sesión, que es quien lo mantiene en memoria.
      Navigator.of(context).pushReplacement(
        MaterialPageRoute(builder: (_) => HomeScreen(sesion: sesion)),
      );
    } on ApiException catch (error) {
      if (!mounted) return;
      _mostrarMensaje(error.mensaje);
    } finally {
      if (mounted) {
        setState(() => _enviando = false);
      }
    }
  }

  /// Abre el registro y, si la cuenta se creó, deja el correo listo para ingresar.
  Future<void> _irARegistro() async {
    final correoRegistrado = await Navigator.of(context).push<String>(
      MaterialPageRoute(builder: (_) => const RegisterScreen()),
    );

    if (!mounted || correoRegistrado == null) {
      return;
    }

    _correo.text = correoRegistrado;
    _mostrarMensaje('Cuenta creada para $correoRegistrado. Ya puedes iniciar sesión.');
  }

  void _mostrarMensaje(String mensaje) {
    ScaffoldMessenger.of(context)
      ..hideCurrentSnackBar()
      ..showSnackBar(SnackBar(content: Text(mensaje)));
  }

  @override
  Widget build(BuildContext context) {
    final estilos = Theme.of(context);

    return Scaffold(
      appBar: AppBar(title: const Text('Iniciar sesión')),
      body: SafeArea(
        child: Center(
          child: SingleChildScrollView(
            padding: const EdgeInsets.all(24),
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: 420),
              child: Form(
                key: _formulario,
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    Text(
                      'Prueba Técnica Full-Stack',
                      style: estilos.textTheme.headlineSmall,
                      textAlign: TextAlign.center,
                    ),
                    const SizedBox(height: 8),
                    Text(
                      'Ingresa con tu cuenta para obtener el token de acceso.',
                      style: estilos.textTheme.bodyMedium,
                      textAlign: TextAlign.center,
                    ),
                    const SizedBox(height: 28),
                    TextFormField(
                      controller: _correo,
                      keyboardType: TextInputType.emailAddress,
                      autocorrect: false,
                      textInputAction: TextInputAction.next,
                      decoration: const InputDecoration(
                        labelText: 'Correo electrónico',
                        border: OutlineInputBorder(),
                      ),
                      validator: Validadores.correo,
                    ),
                    const SizedBox(height: 16),
                    TextFormField(
                      controller: _contrasena,
                      obscureText: true,
                      decoration: const InputDecoration(
                        labelText: 'Contraseña',
                        border: OutlineInputBorder(),
                      ),
                      validator: Validadores.contrasena,
                      onFieldSubmitted: (_) => _enviando ? null : _ingresar(),
                    ),
                    const SizedBox(height: 24),
                    FilledButton(
                      onPressed: _enviando ? null : _ingresar,
                      child: _enviando
                          ? const SizedBox(
                              height: 20,
                              width: 20,
                              child: CircularProgressIndicator(strokeWidth: 2),
                            )
                          : const Text('Ingresar'),
                    ),
                    const SizedBox(height: 8),
                    TextButton(
                      onPressed: _enviando ? null : _irARegistro,
                      child: const Text('¿No tienes cuenta? Regístrate'),
                    ),
                  ],
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }
}
