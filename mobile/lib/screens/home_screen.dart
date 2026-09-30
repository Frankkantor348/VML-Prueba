import 'package:flutter/material.dart';

import '../models/auth_models.dart';
import 'login_screen.dart';

/// Pantalla que confirma la sesión iniciada.
///
/// Recibe la sesión por parámetro: el token se mantiene en memoria, que es lo que
/// pide el alcance de la prueba. Se muestra recortado para no dejar una credencial
/// válida a la vista.
class HomeScreen extends StatelessWidget {
  const HomeScreen({super.key, required this.sesion});

  final Sesion sesion;

  /// Fecha de vencimiento en hora local y en formato corto.
  String get _vencimiento {
    final fecha = DateTime.tryParse(sesion.expiresAt);
    if (fecha == null) {
      return sesion.expiresAt;
    }

    final local = fecha.toLocal();
    String dosDigitos(int valor) => valor.toString().padLeft(2, '0');

    return '${dosDigitos(local.day)}/${dosDigitos(local.month)}/${local.year} '
        '${dosDigitos(local.hour)}:${dosDigitos(local.minute)}';
  }

  @override
  Widget build(BuildContext context) {
    final estilos = Theme.of(context);

    return Scaffold(
      appBar: AppBar(
        title: const Text('Sesión iniciada'),
        automaticallyImplyLeading: false,
      ),
      body: SafeArea(
        child: Center(
          child: SingleChildScrollView(
            padding: const EdgeInsets.all(24),
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: 420),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  Icon(
                    Icons.check_circle_outline,
                    size: 56,
                    color: estilos.colorScheme.primary,
                  ),
                  const SizedBox(height: 16),
                  Text(
                    'La API validó las credenciales y entregó un token JWT.',
                    style: estilos.textTheme.bodyLarge,
                    textAlign: TextAlign.center,
                  ),
                  const SizedBox(height: 28),
                  _Dato(etiqueta: 'Correo', valor: sesion.email),
                  _Dato(etiqueta: 'Token JWT', valor: sesion.tokenRecortado, monoespaciado: true),
                  _Dato(etiqueta: 'Vence', valor: _vencimiento),
                  const SizedBox(height: 8),
                  Text(
                    'El token aparece recortado porque sigue siendo válido hasta su '
                    'vencimiento; se conserva completo en memoria.',
                    style: estilos.textTheme.bodySmall,
                  ),
                  const SizedBox(height: 28),
                  OutlinedButton(
                    onPressed: () {
                      // Vuelvo al login descartando esta pantalla: la sesión en
                      // memoria se pierde con ella.
                      Navigator.of(context).pushReplacement(
                        MaterialPageRoute(builder: (_) => const LoginScreen()),
                      );
                    },
                    child: const Text('Cerrar sesión'),
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}

/// Fila de etiqueta y valor, para no repetir el mismo par de textos tres veces.
class _Dato extends StatelessWidget {
  const _Dato({
    required this.etiqueta,
    required this.valor,
    this.monoespaciado = false,
  });

  final String etiqueta;
  final String valor;
  final bool monoespaciado;

  @override
  Widget build(BuildContext context) {
    final estilos = Theme.of(context);

    return Padding(
      padding: const EdgeInsets.only(bottom: 16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            etiqueta,
            style: estilos.textTheme.labelMedium?.copyWith(
              color: estilos.colorScheme.onSurfaceVariant,
            ),
          ),
          const SizedBox(height: 4),
          Container(
            width: double.infinity,
            padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
            decoration: BoxDecoration(
              color: estilos.colorScheme.surfaceContainerHighest,
              borderRadius: BorderRadius.circular(8),
            ),
            child: Text(
              valor,
              style: monoespaciado
                  ? estilos.textTheme.bodySmall?.copyWith(fontFamily: 'monospace')
                  : estilos.textTheme.bodyMedium,
            ),
          ),
        ],
      ),
    );
  }
}
