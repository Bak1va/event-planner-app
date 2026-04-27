import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { of } from 'rxjs';

import { App } from './app';
import { routes } from './app.routes';
import { EventService } from '../../services/event.service';

describe('App', () => {
  beforeEach(async () => {
    const eventServiceMock = {
      getAllEvents: () => of([])
    };

    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideRouter(routes), { provide: EventService, useValue: eventServiceMock }]
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should render welcome page header', async () => {
    const fixture = TestBed.createComponent(App);
    const router = TestBed.inject(Router);

    await router.navigateByUrl('/');
    fixture.detectChanges();
    await fixture.whenStable();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h2')?.textContent).toContain('Find the events worth showing up for');
  });
});
