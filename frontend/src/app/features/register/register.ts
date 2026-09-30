import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { describeApiError } from '../../core/http/api-error';

/**
 * Formulario de registro.
 * Si la API responde 201 muestra la confirmación; si responde 400 o 409 muestra el
 * mensaje que envió el backend (por ejemplo, correo ya registrado).
 */
@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register.html',
})
export class Register {
  private readonly auth = inject(AuthService);
  private readonly fb = inject(FormBuilder);

  protected readonly enviando = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly registrado = signal<string | null>(null);

  protected readonly formulario = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
  });

  protected enviar(): void {
    if (this.formulario.invalid) {
      this.formulario.markAllAsTouched();
      return;
    }

    this.enviando.set(true);
    this.error.set(null);
    this.registrado.set(null);

    this.auth.register(this.formulario.getRawValue()).subscribe({
      next: (usuario) => {
        this.enviando.set(false);
        // El backend normaliza el correo a minúsculas; muestro el que devolvió la API.
        this.registrado.set(usuario.email);
        this.formulario.reset();
      },
      error: (fallo: unknown) => {
        this.enviando.set(false);
        this.error.set(describeApiError(fallo));
      },
    });
  }
}
