import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideRouter } from '@angular/router';
import { providePrimeNG } from 'primeng/config';
import { MessageService } from 'primeng/api';
import Aura from '@primeuix/themes/aura';

import { routes } from './app.routes';
import { authInterceptor } from './core/auth/auth.interceptor';
import { errorInterceptor } from './core/http/error.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideAnimationsAsync(),
    // authInterceptor first: it needs to see the raw response to catch a
    // 401 and retry before errorInterceptor's catch-all toast would fire.
    provideHttpClient(withInterceptors([authInterceptor, errorInterceptor])),
    // PrimeNG's CSS-variable theme is scoped under a `.p-` prefix / data
    // attribute, so it coexists with Angular Material's `--mat-sys-*`
    // tokens rather than overriding them.
    providePrimeNG({ theme: { preset: Aura, options: { darkModeSelector: false } } }),
    MessageService,
  ]
};
