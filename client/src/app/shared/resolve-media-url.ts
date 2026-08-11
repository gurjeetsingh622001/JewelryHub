import { environment } from '../../environments/environment';

// In dev, apiUrl is an absolute cross-origin URL (https://localhost:65334/api/v1)
// since the Angular dev server and the API run on different ports — strip the
// /api/v1 suffix to get just the origin. In production apiUrl is a relative
// path (reverse-proxied under the same origin as the built SPA), so there's
// no separate origin to prefix with.
const API_ORIGIN = environment.apiUrl.startsWith('http') ? new URL(environment.apiUrl).origin : '';

/**
 * Uploaded files (product images, KYC documents) come back from the backend
 * as a relative path like "/uploads/products/xxx.jpg" (see UploadsService) —
 * resolve it against the API's origin so <img>/background-image/href
 * references actually load. Already-absolute URLs (Unsplash, or any
 * externally pasted URL from before the upload feature existed) pass
 * through unchanged.
 */
export function resolveMediaUrl(url: string | null | undefined): string | null {
  if (!url) return null;
  return /^https?:\/\//i.test(url) ? url : `${API_ORIGIN}${url}`;
}
