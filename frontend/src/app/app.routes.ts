import { Routes } from '@angular/router';
import { Login } from './features/login/login';
import { Register } from './features/register/register';
import { Session } from './features/session/session';

/**
 * Rutas de la aplicación. El alcance de la prueba son registro y login, así que
 * son tres pantallas y la raíz redirige al login.
 */
export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'login' },
  { path: 'login', component: Login },
  { path: 'register', component: Register },
  { path: 'session', component: Session },
  { path: '**', redirectTo: 'login' },
];
