import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { describeApiError } from '../../core/http/api-error';

/**
 * Formulario de inicio de sesión.
 * Si la API responde 200, guarda el token y navega a la pantalla de sesión.
 */
@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login.html',
})
export class Login {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  /** Evita que se envíe dos veces mientras la petición está en curso. */
  protected readonly enviando = signal(false);

  /** Mensaje de error que devolvió la API, listo para mostrar. */
  protected readonly error = signal<string | null>(null);

  protected readonly formulario = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    // El mínimo coincide con la política que aplica el dominio en el backend.
    password: ['', [Validators.required, Validators.minLength(8)]],
  });

  protected enviar(): void {
    if (this.formulario.invalid) {
      // Marco los campos para que se vean los mensajes de validación.
      this.formulario.markAllAsTouched();
      return;
    }

    this.enviando.set(true);
    this.error.set(null);

    this.auth.login(this.formulario.getRawValue()).subscribe({
      next: () => {
        this.enviando.set(false);
        void this.router.navigate(['/session']);
      },
      error: (fallo: unknown) => {
        this.enviando.set(false);
        this.error.set(describeApiError(fallo));
      },
    });
  }
}
