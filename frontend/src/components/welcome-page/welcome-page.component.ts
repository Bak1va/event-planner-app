import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { AppHeaderComponent } from '../app-header/app-header.component';
import { EventCardComponent } from '../event-card/event-card.component';
import { EventCardData } from '../../models/event-card-data.model';
import { EventService } from '../../services/event.service';
import { EventDto } from '../../DTOs/event.dto';

@Component({
  selector: 'app-welcome-page',
  standalone: true,
  imports: [AppHeaderComponent, EventCardComponent],
  templateUrl: './welcome-page.component.html',
  styleUrl: './welcome-page.component.css'
})
export class WelcomePageComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly eventService = inject(EventService);
  private readonly fallbackImages = [
    'https://images.unsplash.com/photo-1517457373958-b7bdd4587205?auto=format&fit=crop&w=900&q=80',
    'https://images.unsplash.com/photo-1414235077428-338989a2e8c0?auto=format&fit=crop&w=900&q=80',
    'https://images.unsplash.com/photo-1515169067868-5387ec356754?auto=format&fit=crop&w=900&q=80'
  ];

  protected events: EventCardData[] = [];

  ngOnInit(): void {
    this.eventService
      .getAllEvents()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((events) => {
        this.events = events.map((event, index) => this.mapToCardData(event, index));
      });
  }

  private mapToCardData(event: EventDto, index: number): EventCardData {
    const formattedDate = new Date(event.dateAdded).toLocaleString(undefined, {
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
}
