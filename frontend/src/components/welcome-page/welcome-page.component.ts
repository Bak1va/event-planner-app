import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectorRef, Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  NonNullableFormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators
} from '@angular/forms';
import { finalize } from 'rxjs';

import { EventDto } from '../../DTOs/event.dto';
import { EventCardData } from '../../models/event-card-data.model';
import { EventService } from '../../services/event.service';
import { AppHeaderComponent } from '../app-header/app-header.component';
import { EventCardComponent } from '../event-card/event-card.component';

@Component({
  selector: 'app-welcome-page',
  standalone: true,
  imports: [AppHeaderComponent, EventCardComponent, ReactiveFormsModule],
  templateUrl: './welcome-page.component.html',
  styleUrl: './welcome-page.component.css'
})
export class WelcomePageComponent implements OnInit {
  private readonly changeDetectorRef = inject(ChangeDetectorRef);
  private readonly destroyRef = inject(DestroyRef);
  private readonly eventService = inject(EventService);
  private readonly formBuilder = inject(NonNullableFormBuilder);
  private readonly fallbackImages = [
    'https://images.unsplash.com/photo-1517457373958-b7bdd4587205?auto=format&fit=crop&w=900&q=80',
    'https://images.unsplash.com/photo-1414235077428-338989a2e8c0?auto=format&fit=crop&w=900&q=80',
    'https://images.unsplash.com/photo-1515169067868-5387ec356754?auto=format&fit=crop&w=900&q=80'
  ];

  protected readonly maxNameLength = 150;
  protected readonly maxStatusLength = 100;
  protected readonly maxDescriptionLength = 1000;
  protected readonly maxImageUrlLength = 2048;
  protected readonly defaultUserId = 1;
  protected readonly statusOptions = [
    'Scheduled',
    'Open',
    'Selling Fast',
    'Registration Required',
    'Cancelled'
  ];

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
  protected isCreateModalOpen = false;
  protected loadError = '';
  protected saveError = '';
  protected saveSuccess = '';

  constructor() {
    this.eventForm.controls.eventDate.setValue(this.buildDefaultEventDateValue());
  }

  ngOnInit(): void {
    this.loadEvents();
  }

  protected openCreateModal(): void {
    this.saveSuccess = '';
    this.isCreateModalOpen = true;
  }

  protected closeCreateModal(): void {
    this.isCreateModalOpen = false;
    this.saveError = '';
    this.saveSuccess = '';
    this.resetForm();
  }

  protected submitForm(): void {
    this.eventForm.markAllAsTouched();
    this.saveSuccess = '';
    this.saveError = '';

    if (this.eventForm.invalid || this.isSaving) {
      return;
    }

    const formValue = this.eventForm.getRawValue();

    this.isSaving = true;
    this.eventService
      .createEvent({
        name: formValue.name.trim(),
        status: formValue.status.trim(),
        eventDate: new Date(formValue.eventDate).toISOString(),
        description: this.normalizeOptionalValue(formValue.description),
        imageUrl: this.normalizeOptionalValue(formValue.imageUrl),
        userId: this.defaultUserId
      })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.isSaving = false;
          this.changeDetectorRef.markForCheck();
        })
      )
      .subscribe({
        next: (createdEvent) => {
          this.closeCreateModal();
          this.saveSuccess = `"${createdEvent.name}" is now live in your event lineup.`;
          this.loadEvents();
          this.changeDetectorRef.markForCheck();
        },
        error: (error: HttpErrorResponse) => {
          this.saveError = this.extractErrorMessage(error, 'We could not publish this event. Please review the details and try again.');
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

  private loadEvents(): void {
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
      imageUrl: event.imageUrl || this.fallbackImages[index % this.fallbackImages.length]
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

    const timezoneOffset = defaultDate.getTimezoneOffset() * 60_000;
    return new Date(defaultDate.getTime() - timezoneOffset).toISOString().slice(0, 16);
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
}
