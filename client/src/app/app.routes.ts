import { Routes } from '@angular/router';
import { authGuard, roleGuard } from './core/auth/auth.guard';
import { ShellComponent } from './core/layout/shell.component';

export const routes: Routes = [
  // Login/Register are deliberately full-page and outside the storefront
  // Shell — no Navbar/Footer inviting someone to wander off mid-signup,
  // same reasoning most premium retailers use for their auth pages.
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register.component').then((m) => m.RegisterComponent),
  },
  {
    // Everything else is the storefront proper — wrapped once in Shell
    // (Navbar + Footer) via its own nested <router-outlet>.
    path: '',
    component: ShellComponent,
    children: [
      {
        path: 'forbidden',
        loadComponent: () => import('./features/forbidden/forbidden.component').then((m) => m.ForbiddenComponent),
      },
      {
        path: 'products',
        loadComponent: () => import('./features/products/product-list/product-list.component').then((m) => m.ProductListComponent),
      },
      {
        path: 'products/:id',
        loadComponent: () =>
          import('./features/products/product-detail/product-detail.component').then((m) => m.ProductDetailComponent),
      },
      {
        // The backend's CartController is [Authorize(Roles = "Customer")] —
        // Sellers/Admins have no cart of their own.
        path: 'cart',
        canActivate: [roleGuard(['Customer'])],
        loadComponent: () => import('./features/cart/cart-page/cart-page.component').then((m) => m.CartPageComponent),
      },
      {
        path: 'checkout',
        canActivate: [roleGuard(['Customer'])],
        loadComponent: () => import('./features/orders/checkout/checkout.component').then((m) => m.CheckoutComponent),
      },
      {
        // The backend's GetMyOrdersQuery is [Authorize(Roles = "Customer")].
        path: 'orders',
        canActivate: [roleGuard(['Customer'])],
        loadComponent: () => import('./features/orders/order-history/order-history.component').then((m) => m.OrderHistoryComponent),
      },
      {
        // Just needs to be logged in, not a specific role — ownership is
        // enforced server-side (GetOrderByIdQuery checks the caller is the
        // order's Customer, an involved Seller, or Admin).
        path: 'orders/:id',
        canActivate: [authGuard],
        loadComponent: () => import('./features/orders/order-detail/order-detail.component').then((m) => m.OrderDetailComponent),
      },
      {
        path: 'seller',
        canActivate: [roleGuard(['Seller'])],
        children: [
          { path: '', loadComponent: () => import('./features/seller/dashboard/seller-dashboard.component').then((m) => m.SellerDashboardComponent) },
          { path: 'kyc', loadComponent: () => import('./features/seller/kyc/seller-kyc.component').then((m) => m.SellerKycComponent) },
          { path: 'products', loadComponent: () => import('./features/seller/products/seller-products.component').then((m) => m.SellerProductsComponent) },
          {
            path: 'products/new',
            loadComponent: () => import('./features/seller/products/product-form/seller-product-form.component').then((m) => m.SellerProductFormComponent),
          },
          {
            path: 'products/:id/edit',
            loadComponent: () => import('./features/seller/products/product-form/seller-product-form.component').then((m) => m.SellerProductFormComponent),
          },
          { path: 'orders', loadComponent: () => import('./features/seller/orders/seller-orders.component').then((m) => m.SellerOrdersComponent) },
        ],
      },
      {
        // Public storefront home — a luxury retail site's landing page is
        // browsable without an account, same as Cartier/Tiffany/Blue Nile.
        // authGuard is reserved for account-specific pages (orders, wishlist).
        path: '',
        loadComponent: () => import('./features/home/home.component').then((m) => m.HomeComponent),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
