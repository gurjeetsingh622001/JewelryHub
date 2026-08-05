import { Component, HostListener, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { MatMenuModule } from '@angular/material/menu';
import { MatSnackBar } from '@angular/material/snack-bar';
import { LucideAngularModule } from 'lucide-angular';
import { AuthService } from '../../auth/auth.service';

interface NavLink {
  label: string;
  fragment: string;
}

@Component({
  selector: 'app-navbar',
  imports: [RouterLink, MatMenuModule, LucideAngularModule],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.scss',
})
export class NavbarComponent {
  protected readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly scrolled = signal(false);
  protected readonly mobileMenuOpen = signal(false);

  // Category browsing pages don't exist yet (Customer UI Phase 13b) — these
  // scroll to the Home page's own category section in the meantime, so the
  // nav is fully functional today rather than pointing at dead routes.
  protected readonly navLinks: NavLink[] = [
    { label: 'Rings', fragment: 'category-rings' },
    { label: 'Necklaces', fragment: 'category-necklaces' },
    { label: 'Earrings', fragment: 'category-earrings' },
    { label: 'Bracelets', fragment: 'category-bracelets' },
    { label: 'The Atelier', fragment: 'brand-story' },
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
