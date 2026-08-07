import { Component, HostListener, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { MatMenuModule } from '@angular/material/menu';
import { MatSnackBar } from '@angular/material/snack-bar';
import { LucideAngularModule } from 'lucide-angular';
import { CartService } from '../../../features/cart/cart.service';
import { AuthService } from '../../auth/auth.service';

interface NavLink {
  label: string;
  route: string[];
  queryParams?: Record<string, string>;
  fragment?: string;
}

@Component({
  selector: 'app-navbar',
  imports: [RouterLink, MatMenuModule, LucideAngularModule],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.scss',
})
export class NavbarComponent {
  protected readonly auth = inject(AuthService);
  protected readonly cart = inject(CartService);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly scrolled = signal(false);
  protected readonly mobileMenuOpen = signal(false);

  // Real Category-based filtering needs actual seeded Category data (an
  // admin/seller concern) — until then, a plain text search against the
  // product name is a reasonable, honest stand-in for "browse by type"
  // rather than fabricating category IDs that don't exist in the DB.
  protected readonly navLinks: NavLink[] = [
    { label: 'Rings', route: ['/products'], queryParams: { search: 'ring' } },
    { label: 'Necklaces', route: ['/products'], queryParams: { search: 'necklace' } },
    { label: 'Earrings', route: ['/products'], queryParams: { search: 'earring' } },
    { label: 'Bracelets', route: ['/products'], queryParams: { search: 'bracelet' } },
    { label: 'Unions', route: ['/unions'] },
    { label: 'The Atelier', route: ['/'], fragment: 'brand-story' },
  ];

  @HostListener('window:scroll')
  onWindowScroll(): void {
    this.scrolled.set(window.scrollY > 8);
  }

  closeMobileMenu(): void {
    this.mobileMenuOpen.set(false);
  }

  toggleMobileMenu(): void {
    this.mobileMenuOpen.update((open) => !open);
  }

  /** Search/wishlist/cart aren't built yet (see docs/ROADMAP.md Phase 13b) — this gives honest feedback instead of a dead click or a link to a route that doesn't exist. */
  notifyComingSoon(feature: string): void {
    this.snackBar.open(`${feature} is part of the next release.`, 'Dismiss', { duration: 4000 });
  }

  logout(): void {
    this.auth.logout().subscribe(() => this.router.navigateByUrl('/'));
  }
}
