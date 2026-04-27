import { TestBed } from '@angular/core/testing';

import { EventCardComponent } from './event-card.component';
import { EventCardData } from '../../models/event-card-data.model';

describe('EventCardComponent', () => {
  const mockEvent: EventCardData = {
    id: 1,
    title: 'Sunset Networking Meetup',
    date: 'Apr 20, 2026, 6:30 PM',
    location: 'Status: Confirmed',
    description: 'An evening meetup for local professionals.',
    imageUrl: 'https://example.com/event.jpg',
    sourceImageUrl: 'https://example.com/event.jpg',
    status: 'Confirmed',
    eventDate: '2026-04-20T18:30:00Z',
    userId: 1
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EventCardComponent]
    }).compileComponents();
  });

  it('should create', () => {
    const fixture = TestBed.createComponent(EventCardComponent);
    fixture.componentRef.setInput('event', mockEvent);
    fixture.detectChanges();

    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should display the event title and date', () => {
    const fixture = TestBed.createComponent(EventCardComponent);
    fixture.componentRef.setInput('event', mockEvent);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const title = compiled.querySelector('h2')?.textContent;
    const metadata = compiled.querySelector('.event-meta')?.textContent;

    expect(title).toContain(mockEvent.title);
    expect(metadata).toContain(mockEvent.date);
  });
});
