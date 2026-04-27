import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { EventDto } from '../DTOs/event.dto';
import { EventRequest } from '../DTOs/event-request.dto';

@Injectable({
  providedIn: 'root'
})
export class EventService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/events';

  getAllEvents(): Observable<EventDto[]> {
    return this.http.get<EventDto[]>(this.baseUrl);
  }

  getEventById(id: number): Observable<EventDto> {
    return this.http.get<EventDto>(`${this.baseUrl}/${id}`);
  }

  getEventsByUserId(userId: number): Observable<EventDto[]> {
    return this.http.get<EventDto[]>(`${this.baseUrl}/user/${userId}`);
  }

  createEvent(payload: EventRequest): Observable<EventDto> {
    return this.http.post<EventDto>(this.baseUrl, payload);
  }

  updateEvent(id: number, payload: EventRequest): Observable<EventDto> {
    return this.http.put<EventDto>(`${this.baseUrl}/${id}`, payload);
  }

  deleteEvent(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
