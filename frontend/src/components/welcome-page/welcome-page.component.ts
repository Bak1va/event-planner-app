import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectorRef, Component, DestroyRef, effect, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  NonNullableFormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { EventDto } from '../../DTOs/event.dto';
import { EventCardData } from '../../models/event-card-data.model';
import { AuthService } from '../../services/auth.service';
import { EventService } from '../../services/event.service';
import { AppHeaderComponent } from '../app-header/app-header.component';
import { EventCardComponent } from '../event-card/event-card.component';

@Component({
  selector: 'app-welcome-page',
  standalone: true,
  imports: [AppHeaderComponent, EventCardComponent, ReactiveFormsModule, RouterLink],
  templateUrl: './welcome-page.component.html',
  styleUrl: './welcome-page.component.css'
})
export class WelcomePageComponent {
  private readonly authService = inject(AuthService);
  private readonly changeDetectorRef = inject(ChangeDetectorRef);
  private readonly destroyRef = inject(DestroyRef);
  private readonly eventService = inject(EventService);
  private readonly formBuilder = inject(NonNullableFormBuilder);
  private readonly router = inject(Router);
  private readonly fallbackImages = [
    'https://images.unsplash.com/photo-1517457373958-b7bdd4587205?auto=format&fit=crop&w=900&q=80',
    'https://images.unsplash.com/photo-1414235077428-338989a2e8c0?auto=format&fit=crop&w=900&q=80',
    'https://images.unsplash.com/photo-1515169067868-5387ec356754?auto=format&fit=crop&w=900&q=80'
  ];

  protected readonly maxNameLength = 150;
  protected readonly maxStatusLength = 100;
  protected readonly maxDescriptionLength = 1000;
  protected readonly maxImageUrlLength = 2048;
  protected readonly statusOptions = [
    'Scheduled',
    'Open',
    'Selling Fast',
    'Registration Required',
    'Cancelled'
  ];
  protected readonly currentUser = this.authService.currentUser;
  protected readonly isAuthenticated = this.authService.isAuthenticated;

  protected readonly eventForm = this.formBuilder.group({
    name: ['', [Validators.required, Validators.maxLength(this.maxNameLength)]],
    status: ['Scheduled', [Validators.required, Validators.maxLength(this.maxStatusLength)]],
    eventDate: ['', [Validators.required, this.futureDateValidator]],
    description: ['', [Validators.maxLength(this.maxDescriptionLength)]],
    imageUrl: ['', [Validators.maxLength(this.maxImageUrlLength)]]
  });

  protected events: EventCardData[] = [];
  protected isLoading = false;
  protected isSaving = false;
  protected isDeleting = false;
  protected isFormModalOpen = false;
  protected isDeleteModalOpen = false;
  protected isProfileModalOpen = false;
  protected formMode: 'create' | 'edit' = 'create';
  protected selectedEventId: number | null = null;
  protected eventPendingDelete: EventCardData | null = null;
  protected loadError = '';
  protected saveError = '';
  protected saveSuccess = '';

  constructor() {
    this.eventForm.controls.eventDate.setValue(this.buildDefaultEventDateValue());

    effect(() => {
      if (this.isAuthenticated()) {
        this.loadEvents();
        return;
      }

      this.events = [];
      this.isLoading = false;
      this.loadError = '';
      this.saveError = '';
      this.saveSuccess = '';
      this.isFormModalOpen = false;
      this.isDeleteModalOpen = false;
      this.isProfileModalOpen = false;
      this.eventPendingDelete = null;
      this.selectedEventId = null;
      this.resetForm();
      this.changeDetectorRef.markForCheck();
    });
  }

  public openCreateModal(): void {
    this.saveSuccess = '';
    this.saveError = '';
    this.formMode = 'create';
    this.selectedEventId = null;
    this.resetForm();
    this.isFormModalOpen = true;
  }

  public openEditModal(event: EventCardData): void {
    this.saveSuccess = '';
    this.saveError = '';
    this.formMode = 'edit';
    this.selectedEventId = event.id;
    this.eventForm.reset({
      name: event.title,
      status: event.status,
      eventDate: this.toDateTimeLocalValue(event.eventDate),
      description: event.description,
      imageUrl: event.sourceImageUrl
    });
    this.isFormModalOpen = true;
  }

  protected closeFormModal(): void {
    this.isFormModalOpen = false;
    this.saveError = '';
    this.selectedEventId = null;
    this.formMode = 'create';
    this.resetForm();
  }

  public openDeleteModal(event: EventCardData): void {
    this.saveError = '';
    this.eventPendingDelete = event;
    this.isDeleteModalOpen = true;
  }

  protected openProfileModal(): void {
    this.isProfileModalOpen = true;
  }

  protected closeProfileModal(): void {
    this.isProfileModalOpen = false;
  }

  protected closeDeleteModal(): void {
    this.isDeleteModalOpen = false;
    this.eventPendingDelete = null;
    this.saveError = '';
  }

  protected onFormBackdropClick(event: MouseEvent): void {
    if (event.target === event.currentTarget) {
      this.closeFormModal();
    }
  }

  protected onFormBackdropKeydown(event: KeyboardEvent): void {
    if (event.target !== event.currentTarget) {
      return;
    }

    if (event.key === 'Enter' || event.key === ' ' || event.key === 'Escape') {
      event.preventDefault();
      this.closeFormModal();
    }
  }

  protected onDeleteBackdropClick(event: MouseEvent): void {
    if (event.target === event.currentTarget) {
      this.closeDeleteModal();
    }
  }

  protected onDeleteBackdropKeydown(event: KeyboardEvent): void {
    if (event.target !== event.currentTarget) {
      return;
    }

    if (event.key === 'Enter' || event.key === ' ' || event.key === 'Escape') {
      event.preventDefault();
      this.closeDeleteModal();
    }
  }

  protected onProfileBackdropClick(event: MouseEvent): void {
    if (event.target === event.currentTarget) {
      this.closeProfileModal();
    }
  }

  protected onProfileBackdropKeydown(event: KeyboardEvent): void {
    if (event.target !== event.currentTarget) {
      return;
    }

    if (event.key === 'Enter' || event.key === ' ' || event.key === 'Escape') {
      event.preventDefault();
      this.closeProfileModal();
    }
  }

  public submitForm(): void {
    this.eventForm.markAllAsTouched();
    this.saveSuccess = '';
    this.saveError = '';

    if (this.eventForm.invalid || this.isSaving) {
      return;
    }

    const formValue = this.eventForm.getRawValue();
    const payload = {
      name: formValue.name.trim(),
      status: formValue.status.trim(),
      eventDate: new Date(formValue.eventDate).toISOString(),
      description: this.normalizeOptionalValue(formValue.description),
      imageUrl: this.normalizeOptionalValue(formValue.imageUrl)
    };
    const request = this.formMode === 'edit' && this.selectedEventId !== null
      ? this.eventService.updateEvent(this.selectedEventId, payload)
      : this.eventService.createEvent(payload);

    this.isSaving = true;
    request
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.isSaving = false;
          this.changeDetectorRef.markForCheck();
        })
      )
      .subscribe({
        next: (savedEvent) => {
          const wasEdit = this.formMode === 'edit';
          this.closeFormModal();
          this.saveSuccess = wasEdit
            ? `"${savedEvent.name}" has been updated.`
            : `"${savedEvent.name}" is now live in your event lineup.`;
          this.loadEvents();
          this.changeDetectorRef.markForCheck();
        },
        error: (error: HttpErrorResponse) => {
          if (this.handleAuthorizationError(error)) {
            return;
          }

          this.saveError = this.extractErrorMessage(
            error,
            this.formMode === 'edit'
              ? 'We could not save your changes. Please review the details and try again.'
              : 'We could not publish this event. Please review the details and try again.'
          );
          this.changeDetectorRef.markForCheck();
        }
      });
  }

  public confirmDelete(): void {
    const eventToDelete = this.eventPendingDelete;
    if (!eventToDelete || this.isDeleting) {
      return;
    }

    this.saveSuccess = '';
    this.saveError = '';
    this.isDeleting = true;

    this.eventService
      .deleteEvent(eventToDelete.id)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.isDeleting = false;
          this.changeDetectorRef.markForCheck();
        })
      )
      .subscribe({
        next: () => {
          this.closeDeleteModal();
          this.saveSuccess = `"${eventToDelete.title}" has been removed from the lineup.`;
          this.loadEvents();
          this.changeDetectorRef.markForCheck();
        },
        error: (error: HttpErrorResponse) => {
          if (this.handleAuthorizationError(error)) {
            return;
          }

          this.saveError = this.extractErrorMessage(
            error,
            'We could not delete this event right now. Please try again in a moment.'
          );
          this.changeDetectorRef.markForCheck();
        }
      });
  }

  protected shouldShowError(controlName: keyof typeof this.eventForm.controls, errorKey: string): boolean {
    const control = this.eventForm.controls[controlName];
    return control.hasError(errorKey) && (control.dirty || control.touched);
  }

  protected currentLength(controlName: keyof typeof this.eventForm.controls): number {
    return this.eventForm.controls[controlName].value.length;
  }

  protected logout(): void {
    this.authService.logout();
    void this.router.navigateByUrl('/');
  }

  private loadEvents(): void {
    if (!this.isAuthenticated()) {
      return;
    }

    this.isLoading = true;
    this.loadError = '';

    this.eventService
      .getAllEvents()
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.isLoading = false;
          this.changeDetectorRef.markForCheck();
        })
      )
      .subscribe({
        next: (events) => {
          this.events = events.map((event, index) => this.mapToCardData(event, index));
          this.changeDetectorRef.markForCheck();
        },
        error: (error: HttpErrorResponse) => {
          if (this.handleAuthorizationError(error)) {
            return;
          }

          this.loadError = this.extractErrorMessage(
            error,
            'We could not load events right now. Please try again in a moment.'
          );
          this.events = [];
          this.changeDetectorRef.markForCheck();
        }
      });
  }

  private mapToCardData(event: EventDto, index: number): EventCardData {
    const formattedDate = new Date(event.eventDate).toLocaleString(undefined, {
      dateStyle: 'medium',
      timeStyle: 'short'
    });

    return {
      id: event.id,
      title: event.name,
      date: formattedDate,
      location: `Status: ${event.status}`,
      description: event.description,
      imageUrl: event.imageUrl || this.fallbackImages[index % this.fallbackImages.length],
      sourceImageUrl: event.imageUrl,
      status: event.status,
      eventDate: event.eventDate,
      userId: event.userId
    };
  }

  private resetForm(): void {
    this.eventForm.reset({
      name: '',
      status: 'Scheduled',
      eventDate: this.buildDefaultEventDateValue(),
      description: '',
      imageUrl: ''
    });
  }

  private normalizeOptionalValue(value: string): string | undefined {
    const trimmedValue = value.trim();
    return trimmedValue.length > 0 ? trimmedValue : undefined;
  }

  private buildDefaultEventDateValue(): string {
    const defaultDate = new Date();
    defaultDate.setDate(defaultDate.getDate() + 7);
    defaultDate.setHours(18, 0, 0, 0);

    return this.toDateTimeLocalValue(defaultDate.toISOString());
  }

  private toDateTimeLocalValue(value: string): string {
    const sourceDate = new Date(value);
    const timezoneOffset = sourceDate.getTimezoneOffset() * 60_000;
    return new Date(sourceDate.getTime() - timezoneOffset).toISOString().slice(0, 16);
  }

  private futureDateValidator(control: AbstractControl<string>): ValidationErrors | null {
    if (!control.value) {
      return null;
    }

    return new Date(control.value).getTime() > Date.now() ? null : { futureDate: true };
  }

  private extractErrorMessage(error: HttpErrorResponse, fallbackMessage: string): string {
    return error.error?.message ?? fallbackMessage;
  }

  private handleAuthorizationError(error: HttpErrorResponse): boolean {
    if (error.status !== 401 && error.status !== 403) {
      return false;
    }

    this.authService.logout();
    void this.router.navigateByUrl('/auth');
    return true;
  }
}
