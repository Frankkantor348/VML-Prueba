import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from './core/auth/auth.service';

/**
 * Componente raíz: cabecera con la navegación y el contenedor de las rutas.
 * El estado de la sesión se lee del AuthService, que es quien lo mantiene.
 */
@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.html',
})
export class App {
  private readonly auth = inject(AuthService);

  protected readonly sesionIniciada = this.auth.sesionIniciada;
  protected readonly correo = this.auth.correo;

  protected cerrarSesion(): void {
    this.auth.logout();
  }
}
