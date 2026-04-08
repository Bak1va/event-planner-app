import { Component, input } from '@angular/core';
import { EventCardData } from '../../models/event-card-data.model';

@Component({
  selector: 'app-event-card',
  standalone: true,
  templateUrl: './event-card.component.html',
  styleUrl: './event-card.component.css'
})
export class EventCardComponent {
  readonly event = input.required<EventCardData>();
}
