import 'package:flutter/material.dart';

import '../core/api_exception.dart';
import '../core/validadores.dart';
import '../services/auth_service.dart';

/// Pantalla de registro.
///
/// Si la API crea la cuenta (201) vuelve al inicio de sesión devolviendo el correo
/// ya normalizado por el backend. Si responde 400 o 409, muestra su mensaje, que es
/// el caso del correo repetido.
class RegisterScreen extends StatefulWidget {
  const RegisterScreen({super.key});

  @override
  State<RegisterScreen> createState() => _RegisterScreenState();
}

class _RegisterScreenState extends State<RegisterScreen> {
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

  Future<void> _registrar() async {
    if (!_formulario.currentState!.validate()) {
      return;
    }

    setState(() => _enviando = true);

    try {
      final usuario = await _auth.registrar(_correo.text.trim(), _contrasena.text);

      if (!mounted) return;
      // Devuelvo el correo tal como lo guardó la API (en minúsculas) para que el
      // login lo reutilice.
      Navigator.of(context).pop(usuario.email);
    } on ApiException catch (error) {
      if (!mounted) return;
      ScaffoldMessenger.of(context)
        ..hideCurrentSnackBar()
        ..showSnackBar(SnackBar(content: Text(error.mensaje)));
    } finally {
      if (mounted) {
        setState(() => _enviando = false);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final estilos = Theme.of(context);

    return Scaffold(
      appBar: AppBar(title: const Text('Crear cuenta')),
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
                      'El correo debe ser único: si ya está registrado, la API responderá '
                      'con un conflicto.',
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
                        helperText: 'Mínimo 8 caracteres',
                        border: OutlineInputBorder(),
                      ),
                      validator: Validadores.contrasena,
                      onFieldSubmitted: (_) => _enviando ? null : _registrar(),
                    ),
                    const SizedBox(height: 24),
                    FilledButton(
                      onPressed: _enviando ? null : _registrar,
                      child: _enviando
                          ? const SizedBox(
                              height: 20,
                              width: 20,
                              child: CircularProgressIndicator(strokeWidth: 2),
                            )
                          : const Text('Registrarme'),
                    ),
                    const SizedBox(height: 8),
                    TextButton(
                      onPressed: _enviando ? null : () => Navigator.of(context).pop(),
                      child: const Text('Ya tengo cuenta'),
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
