import { Component, computed, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

/** Cuántos caracteres del token se dejan visibles, al inicio y al final. */
const VISIBLES_AL_INICIO = 22;
const VISIBLES_AL_FINAL = 6;

/**
 * Pantalla que confirma que la sesión está iniciada.
 *
 * El token se muestra recortado a propósito: sirve para comprobar que la API lo
 * entregó, pero volcarlo entero en pantalla expone una credencial que sigue siendo
 * válida hasta que venza. El token completo permanece en `localStorage`, que es
 * donde lo lee el resto de la aplicación.
 */
@Component({
  selector: 'app-session',
  templateUrl: './session.html',
})
export class Session {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly correo = this.auth.correo;

  /** Vista corta del token, solo para confirmar que existe. */
  protected readonly tokenRecortado = computed(() => {
    const token = this.auth.token();
    if (!token) {
      return null;
    }

    return `${token.slice(0, VISIBLES_AL_INICIO)}...${token.slice(-VISIBLES_AL_FINAL)}`;
  });

  protected cerrarSesion(): void {
    this.auth.logout();
    void this.router.navigate(['/login']);
  }
}
