import { ApplicationConfig, importProvidersFrom, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideRouter, withInMemoryScrolling } from '@angular/router';
import {
  LucideAngularModule,
  ArrowRight,
  Award,
  ChevronDown,
  ChevronRight,
  Facebook,
  Gem,
  Heart,
  Instagram,
  LogOut,
  Mail,
  MapPin,
  Menu,
  Minus,
  Package,
  Phone,
  Plus,
  Search,
  ShieldCheck,
  ShoppingBag,
  Sparkles,
  Star,
  Trash2,
  Truck,
  Twitter,
  User,
  X,
} from 'lucide-angular';

import { routes } from './app.routes';
import { authInterceptor } from './core/auth/auth.interceptor';
import { errorInterceptor } from './core/http/error.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    // Enables the navbar/footer's fragment links (e.g. #category-rings) to
    // actually scroll to the target section on the Home page.
    provideRouter(routes, withInMemoryScrolling({ anchorScrolling: 'enabled', scrollPositionRestoration: 'enabled' })),
    provideAnimationsAsync(),
    // authInterceptor first: it needs to see the raw response to catch a
    // 401 and retry before errorInterceptor's catch-all toast would fire.
    provideHttpClient(withInterceptors([authInterceptor, errorInterceptor])),
    // Every icon used anywhere in the app is registered once here — see
    // docs/DESIGN_SYSTEM.md § Icons. Add to this list rather than
    // registering icons per-component.
    importProvidersFrom(
      LucideAngularModule.pick({
        ArrowRight,
        Award,
        ChevronDown,
        ChevronRight,
        Facebook,
        Gem,
        Heart,
        Instagram,
        LogOut,
        Mail,
        MapPin,
        Menu,
        Minus,
        Package,
        Phone,
        Plus,
        Search,
        ShieldCheck,
        ShoppingBag,
        Sparkles,
        Star,
        Trash2,
        Truck,
        Twitter,
        User,
        X,
      }),
    ),
  ]
};
