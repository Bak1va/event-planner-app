import { Component, HostListener, input, output, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './app-header.component.html',
  styleUrl: './app-header.component.css'
})
export class AppHeaderComponent {
  public readonly actionLabel = input<string | null>(null);
  public readonly actionLink = input<string | null>(null);
  public readonly userName = input<string | null>(null);
  public readonly showLogout = input(false);
  public readonly profileRequested = output<void>();
  public readonly logoutRequested = output<void>();
  protected readonly isMenuOpen = signal(false);

  protected toggleMenu(): void {
    this.isMenuOpen.update((value) => !value);
  }

  protected openProfile(): void {
    this.isMenuOpen.set(false);
    this.profileRequested.emit();
  }

  protected logout(): void {
    this.isMenuOpen.set(false);
    this.logoutRequested.emit();
  }

  @HostListener('document:click')
  protected closeMenu(): void {
    this.isMenuOpen.set(false);
  }

  protected onMenuContainerClick(event: MouseEvent): void {
    event.stopPropagation();
  }

  protected onMenuContainerKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter' || event.key === ' ') {
      event.preventDefault();
      event.stopPropagation();
    }
  }
}
