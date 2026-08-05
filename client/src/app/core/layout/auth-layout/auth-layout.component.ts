import { Component, Input } from '@angular/core';
import { RouterLink } from '@angular/router';

/**
 * Shared full-page shell for Login/Register — deliberately outside the
 * storefront Shell (no Navbar/Footer, see app.routes.ts) so nothing
 * distracts from signing in/up. Split-screen: a full-bleed photo + quote
 * on desktop, content column always. Content is projected so Login and
 * Register each own their form/heading markup.
 */
@Component({
  selector: 'app-auth-layout',
  imports: [RouterLink],
  templateUrl: './auth-layout.component.html',
  styleUrl: './auth-layout.component.scss',
})
export class AuthLayoutComponent {
  @Input({ required: true }) image!: string;
  @Input({ required: true }) quote!: string;
  /** Register's Seller form needs more room for its 2–3 column field rows than Login's single-column form does. */
  @Input() maxWidth = '26rem';
}
