import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { of } from 'rxjs';

import { App } from './app';
import { routes } from './app.routes';
import { AuthService } from '../../services/auth.service';
import { EventService } from '../../services/event.service';

describe('App', () => {
  beforeEach(async () => {
    const eventServiceMock = {
      getAllEvents: () => of([])
    };
    const authServiceMock = {
      currentUser: signal(null),
      isAuthenticated: signal(false),
      logout: () => undefined
    };

    await TestBed.configureTestingModule({
      imports: [App],
      providers: [
        provideRouter(routes),
        { provide: EventService, useValue: eventServiceMock },
        { provide: AuthService, useValue: authServiceMock }
      ]
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should render welcome page header', async () => {
    const fixture = TestBed.createComponent(App);
    const router = TestBed.inject(Router);

    await router.navigateByUrl('/');
    fixture.detectChanges();
    await fixture.whenStable();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h2')?.textContent).toContain('Plan, publish, and manage events from one secure workspace');
  });
});
