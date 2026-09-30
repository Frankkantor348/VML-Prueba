import 'package:flutter/material.dart';

import 'screens/login_screen.dart';

void main() => runApp(const VmlMobileApp());

/// Aplicación móvil de la prueba técnica.
///
/// Solo tiene tres pantallas: registro, inicio de sesión y confirmación de la
/// sesión. El tema se define aquí para que todas compartan la misma paleta que el
/// cliente web.
class VmlMobileApp extends StatelessWidget {
  const VmlMobileApp({super.key});

  /// Mismo azul de acento que usa el frontend Angular.
  static const Color _acento = Color(0xFF1E4D8C);

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Prueba Técnica Full-Stack',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        colorScheme: ColorScheme.fromSeed(seedColor: _acento),
        useMaterial3: true,
      ),
      home: const LoginScreen(),
    );
  }
}
