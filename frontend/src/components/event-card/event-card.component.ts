import { Component, input } from '@angular/core';

export interface EventCardData {
  title: string;
  date: string;
  location: string;
  description: string;
  imageUrl: string;
}

@Component({
  selector: 'app-event-card',
  standalone: true,
  templateUrl: './event-card.component.html',
  styleUrl: './event-card.component.css'
})
export class EventCardComponent {
  readonly event = input.required<EventCardData>();
}
