import 'dart:async';
import 'dart:convert';
import 'dart:io';

import 'package:http/http.dart' as http;

import '../core/api_config.dart';
import '../core/api_exception.dart';

/// Cliente HTTP de la aplicación.
///
/// Encapsula `package:http` y la traducción de errores, de modo que las pantallas
/// nunca construyan direcciones ni lean códigos de estado: piden aquí y reciben
/// datos o un `ApiException` con el mensaje ya redactado.
class ApiClient {
  ApiClient({http.Client? cliente, String? baseUrl})
      : _cliente = cliente ?? http.Client(),
        _baseUrl = baseUrl ?? ApiConfig.baseUrl;

  final http.Client _cliente;
  final String _baseUrl;

  /// Envía un `POST` con cuerpo JSON y devuelve la respuesta ya decodificada.
  Future<Map<String, dynamic>> post(String ruta, Map<String, dynamic> cuerpo) async {
    final direccion = Uri.parse('$_baseUrl$ruta');

    try {
      final respuesta = await _cliente
          .post(
            direccion,
            headers: const {'Content-Type': 'application/json'},
            body: jsonEncode(cuerpo),
          )
          .timeout(ApiConfig.tiempoDeEspera);

      return _procesar(respuesta);
    } on ApiException {
      // Ya viene traducido desde _procesar; no lo envuelvo otra vez.
      rethrow;
    } on TimeoutException {
      throw const ApiException(
        'La API tardó demasiado en responder. Verifica que esté encendida.',
      );
    } on SocketException {
      throw const ApiException(
        'No se pudo contactar con la API. Revisa la conexión y la URL configurada.',
      );
    } on FormatException {
      throw const ApiException('La respuesta de la API no tiene el formato esperado.');
    }
  }

  /// Libera las conexiones abiertas.
  void dispose() => _cliente.close();

  Map<String, dynamic> _procesar(http.Response respuesta) {
    final dynamic decodificado = respuesta.body.isEmpty ? null : jsonDecode(respuesta.body);
    final cuerpo = decodificado is Map<String, dynamic> ? decodificado : <String, dynamic>{};

    if (respuesta.statusCode >= 200 && respuesta.statusCode < 300) {
      return cuerpo;
    }

    // La API responde ProblemDetails (RFC 7807) cuando algo falla, y su campo
    // `detail` ya viene redactado en español: se muestra tal cual en lugar de
    // inventar un mensaje genérico.
    final detalle = cuerpo['detail'];
    if (detalle is String && detalle.isNotEmpty) {
      throw ApiException(detalle);
    }

    throw ApiException('La API respondió con un error ${respuesta.statusCode}.');
  }
}
