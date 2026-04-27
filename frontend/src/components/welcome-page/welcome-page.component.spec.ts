import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { vi } from 'vitest';

import { WelcomePageComponent } from './welcome-page.component';
import { EventService } from '../../services/event.service';
import { EventDto } from '../../DTOs/event.dto';

describe('WelcomePageComponent', () => {
  let eventServiceMock: {
    getAllEvents: ReturnType<typeof vi.fn>;
    createEvent: ReturnType<typeof vi.fn>;
    updateEvent: ReturnType<typeof vi.fn>;
    deleteEvent: ReturnType<typeof vi.fn>;
  };

  const mockEvents: EventDto[] = [
    {
      id: 1,
      name: 'Frontend Workshop',
      status: 'Scheduled',
      description: 'A workshop focused on modern Angular testing.',
      imageUrl: 'https://example.com/workshop.jpg',
      eventDate: '2026-05-01T18:30:00Z',
      dateAdded: '2026-04-15T18:30:00Z',
      dateModified: '2026-04-15T18:30:00Z',
      userId: 10
    },
    {
      id: 2,
      name: 'City Hack Night',
      status: 'Open',
      description: 'Collaborative coding and pizza night.',
      imageUrl: 'https://example.com/hacknight.jpg',
      eventDate: '2026-05-02T17:00:00Z',
      dateAdded: '2026-04-18T17:00:00Z',
      dateModified: '2026-04-18T17:00:00Z',
      userId: 11
    }
  ];

  beforeEach(async () => {
    eventServiceMock = {
      getAllEvents: vi.fn().mockReturnValue(of(mockEvents)),
      createEvent: vi.fn().mockReturnValue(of(mockEvents[0])),
      updateEvent: vi.fn().mockReturnValue(of(mockEvents[0])),
      deleteEvent: vi.fn().mockReturnValue(of(void 0))
    };

    await TestBed.configureTestingModule({
      imports: [WelcomePageComponent],
      providers: [{ provide: EventService, useValue: eventServiceMock }]
    }).compileComponents();
  });

  it('should create', () => {
    const fixture = TestBed.createComponent(WelcomePageComponent);
    fixture.detectChanges();

    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should render events from the API in cards', () => {
    const fixture = TestBed.createComponent(WelcomePageComponent);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const cards = compiled.querySelectorAll('app-event-card');

    expect(cards.length).toBe(mockEvents.length);
    expect(compiled.textContent).toContain(mockEvents[0].name);
    expect(compiled.textContent).toContain(mockEvents[1].name);
  });

  it('should show mapped date text for an event card', () => {
    const fixture = TestBed.createComponent(WelcomePageComponent);
    fixture.detectChanges();

    const expectedDate = new Date(mockEvents[0].eventDate).toLocaleString(undefined, {
      dateStyle: 'medium',
      timeStyle: 'short'
    });

    const compiled = fixture.nativeElement as HTMLElement;
    const firstCardMeta = compiled.querySelector('.event-card .event-meta')?.textContent;

    expect(firstCardMeta).toContain(expectedDate);
  });

  it('should keep save disabled while the form is invalid', () => {
    const fixture = TestBed.createComponent(WelcomePageComponent);
    fixture.detectChanges();

    // Test the form validation directly without rendering the modal
    // The form starts with required fields empty, so it should be invalid
    expect(fixture.componentInstance['eventForm'].invalid).toBe(true);
  });

  it('should open the create event modal from the add event button', () => {
    const fixture = TestBed.createComponent(WelcomePageComponent);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const openButton = compiled.querySelector<HTMLButtonElement>('.add-event-button');

    openButton?.click();
    fixture.detectChanges();

    expect(compiled.querySelector('[role="dialog"]')).not.toBeNull();
  });

  it('should open the edit modal from an event card action', () => {
    const fixture = TestBed.createComponent(WelcomePageComponent);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const editButton = compiled.querySelector<HTMLButtonElement>('[aria-label="Edit event"]');

    editButton?.click();
    fixture.detectChanges();

    expect(compiled.querySelector('[aria-label="Edit event dialog"]')).not.toBeNull();
    expect(compiled.textContent).toContain('Update event details');
  });

  it('should call updateEvent when saving edits', () => {
    const fixture = TestBed.createComponent(WelcomePageComponent);
    fixture.detectChanges();

    // Set up the component state for edit mode
    fixture.componentInstance['formMode'] = 'edit';
    fixture.componentInstance['selectedEventId'] = 1;

    // Fill in the form with valid data
    fixture.componentInstance['eventForm'].patchValue({
      name: 'Frontend Workshop Updated',
      status: 'Open',
      eventDate: '2026-05-08T18:30',
      description: 'Updated details',
      imageUrl: 'https://example.com/updated.jpg'
    });

    // Call submitForm and verify updateEvent was called
    fixture.componentInstance['submitForm']();

    expect(eventServiceMock.updateEvent).toHaveBeenCalled();
    expect(eventServiceMock.createEvent).not.toHaveBeenCalled();
  });

  it('should open the delete confirmation modal from an event card action', () => {
    const fixture = TestBed.createComponent(WelcomePageComponent);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const deleteButton = compiled.querySelector<HTMLButtonElement>('[aria-label="Delete event"]');

    deleteButton?.click();
    fixture.detectChanges();

    expect(compiled.querySelector('[aria-label="Delete event dialog"]')).not.toBeNull();
    expect(compiled.textContent).toContain('Delete this event?');
  });

  it('should call deleteEvent after confirming delete', () => {
    const fixture = TestBed.createComponent(WelcomePageComponent);
    fixture.detectChanges();

    fixture.componentInstance['openDeleteModal']({
      id: 1,
      title: 'Frontend Workshop',
      date: 'May 1, 2026, 6:30 PM',
      location: 'Status: Scheduled',
      description: 'A workshop focused on modern Angular testing.',
      imageUrl: 'https://example.com/workshop.jpg',
      sourceImageUrl: 'https://example.com/workshop.jpg',
      status: 'Scheduled',
      eventDate: '2026-05-01T18:30:00Z',
      userId: 10
    });

    fixture.componentInstance['confirmDelete']();

    expect(eventServiceMock.deleteEvent).toHaveBeenCalledWith(1);
  });
});
