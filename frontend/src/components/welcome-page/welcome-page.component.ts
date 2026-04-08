import { Component } from '@angular/core';

import { AppHeaderComponent } from '../app-header/app-header.component';
import { EventCardComponent } from '../event-card/event-card.component';
import { EventCardData } from '../../models/event-card-data.model';

@Component({
  selector: 'app-welcome-page',
  standalone: true,
  imports: [AppHeaderComponent, EventCardComponent],
  templateUrl: './welcome-page.component.html',
  styleUrl: './welcome-page.component.css'
})
export class WelcomePageComponent {
  protected readonly events: EventCardData[] = [
    {
      title: 'Rooftop Sunset Mixer',
      date: 'May 12, 2026 • 6:30 PM',
      location: 'Skyline Terrace',
      description: 'A relaxed networking evening with live acoustic music and city views.',
      imageUrl: 'https://images.unsplash.com/photo-1517457373958-b7bdd4587205?auto=format&fit=crop&w=900&q=80'
    },
    {
      title: 'Spring Food Festival',
      date: 'May 18, 2026 • 11:00 AM',
      location: 'Riverfront Park',
      description: 'Taste signature dishes from local chefs with family-friendly activities all day.',
      imageUrl: 'https://images.unsplash.com/photo-1414235077428-338989a2e8c0?auto=format&fit=crop&w=900&q=80'
    },
    {
      title: 'Tech Innovators Meetup',
      date: 'May 24, 2026 • 4:00 PM',
      location: 'Launch Hub Downtown',
      description: 'Lightning talks and demos from startups building smart city and AI products.',
      imageUrl: 'https://images.unsplash.com/photo-1515169067868-5387ec356754?auto=format&fit=crop&w=900&q=80'
    }
  ];
}
