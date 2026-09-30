import { provideHttpClient } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { App } from './app';
import { routes } from './app.routes';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      // El componente raíz lee la sesión del AuthService, que a su vez necesita
      // HttpClient y el router: hay que dárselos al módulo de pruebas.
      providers: [provideHttpClient(), provideRouter(routes)],
    }).compileComponents();
  });

  it('debería crear el componente raíz', () => {
    const fixture = TestBed.createComponent(App);
    expect(fixture.componentInstance).toBeTruthy();
  });
});
