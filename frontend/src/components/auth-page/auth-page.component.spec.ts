import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';

import { AuthService } from '../../services/auth.service';
import { AuthPageComponent } from './auth-page.component';

describe('AuthPageComponent', () => {
  beforeEach(async () => {
    const authServiceMock = {
      currentUser: signal(null),
      isAuthenticated: signal(false),
      login: () => of(),
      signUp: () => of(),
      logout: () => undefined
    };

    await TestBed.configureTestingModule({
      imports: [AuthPageComponent],
      providers: [provideRouter([]), { provide: AuthService, useValue: authServiceMock }]
    }).compileComponents();
  });

  it('should create', () => {
    const fixture = TestBed.createComponent(AuthPageComponent);
    fixture.detectChanges();

    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should switch to the sign up form when the tab is clicked', () => {
    const fixture = TestBed.createComponent(AuthPageComponent);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const signUpTab = compiled.querySelectorAll<HTMLButtonElement>('.auth-tab')[1];

    signUpTab?.click();
    fixture.detectChanges();

    expect(compiled.textContent).toContain('Create your account');
    expect(compiled.textContent).toContain('Phone number');
  });

  it('should keep the login submit button disabled while the form is invalid', () => {
    const fixture = TestBed.createComponent(AuthPageComponent);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const submitButton = compiled.querySelector('button[type="submit"]') as HTMLButtonElement | null;

    expect(submitButton?.disabled).toBe(true);
  });
});
