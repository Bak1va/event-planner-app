import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { WelcomePageComponent } from './welcome-page.component';
import { EventService } from '../../services/event.service';
import { EventDto } from '../../DTOs/event.dto';

describe('WelcomePageComponent', () => {
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
    const eventServiceMock = {
      getAllEvents: () => of(mockEvents)
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

    fixture.componentInstance.openCreateModal();
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const submitButton = compiled.querySelector<HTMLButtonElement>('button[type="submit"]');

    expect(submitButton?.disabled).toBe(true);
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
});
