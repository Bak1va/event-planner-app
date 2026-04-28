import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectorRef, Component, DestroyRef, effect, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';

import { AuthService } from '../../services/auth.service';
import { AppHeaderComponent } from '../app-header/app-header.component';

@Component({
  selector: 'app-auth-page',
  standalone: true,
  imports: [AppHeaderComponent, ReactiveFormsModule],
  templateUrl: './auth-page.component.html',
  styleUrl: './auth-page.component.css'
})
export class AuthPageComponent {
  private readonly authService = inject(AuthService);
  private readonly changeDetectorRef = inject(ChangeDetectorRef);
  private readonly destroyRef = inject(DestroyRef);
  private readonly formBuilder = inject(NonNullableFormBuilder);
  private readonly router = inject(Router);
  private readonly phonePattern = /^[0-9+\-\s()]{7,20}$/;

  protected readonly isAuthenticated = this.authService.isAuthenticated;
  protected activeTab: 'login' | 'signup' = 'login';
  protected loginError = '';
  protected signUpError = '';
  protected isLoggingIn = false;
  protected isSigningUp = false;

  protected readonly loginForm = this.formBuilder.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]]
  });

  protected readonly signUpForm = this.formBuilder.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    firstName: ['', [Validators.required, Validators.maxLength(80)]],
    lastName: ['', [Validators.required, Validators.maxLength(80)]],
    phoneNumber: ['', [Validators.required, Validators.pattern(this.phonePattern)]]
  });

  constructor() {
    effect(() => {
      if (this.isAuthenticated()) {
        void this.router.navigateByUrl('/');
      }
    });
  }

  protected setActiveTab(tab: 'login' | 'signup'): void {
    this.activeTab = tab;
    this.loginError = '';
    this.signUpError = '';
  }

  protected submitLogin(): void {
    this.loginForm.markAllAsTouched();
    this.loginError = '';

    if (this.loginForm.invalid || this.isLoggingIn) {
      return;
    }

    this.isLoggingIn = true;
    this.authService
      .login(this.loginForm.getRawValue())
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.isLoggingIn = false;
          this.changeDetectorRef.markForCheck();
        })
      )
      .subscribe({
        next: () => {
          void this.router.navigateByUrl('/');
        },
        error: (error: HttpErrorResponse) => {
          this.loginError = this.extractErrorMessage(error, 'We could not sign you in. Please try again.');
          this.changeDetectorRef.markForCheck();
        }
      });
  }

  protected submitSignUp(): void {
    this.signUpForm.markAllAsTouched();
    this.signUpError = '';

    if (this.signUpForm.invalid || this.isSigningUp) {
      return;
    }

    this.isSigningUp = true;
    this.authService
      .signUp(this.signUpForm.getRawValue())
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.isSigningUp = false;
          this.changeDetectorRef.markForCheck();
        })
      )
      .subscribe({
        next: () => {
          void this.router.navigateByUrl('/');
        },
        error: (error: HttpErrorResponse) => {
          this.signUpError = this.extractErrorMessage(
            error,
            'We could not create your account right now. Please review your details and try again.'
          );
          this.changeDetectorRef.markForCheck();
        }
      });
  }

  protected shouldShowLoginError(controlName: keyof typeof this.loginForm.controls, errorKey: string): boolean {
    const control = this.loginForm.controls[controlName];
    return control.hasError(errorKey) && (control.dirty || control.touched);
  }

  protected shouldShowSignUpError(controlName: keyof typeof this.signUpForm.controls, errorKey: string): boolean {
    const control = this.signUpForm.controls[controlName];
    return control.hasError(errorKey) && (control.dirty || control.touched);
  }

  private extractErrorMessage(error: HttpErrorResponse, fallbackMessage: string): string {
    return error.error?.message ?? fallbackMessage;
  }
}
