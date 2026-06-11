/**
 * Production build: the SPA is served by the API from the same origin, so the API is reached
 * at a relative path — no host, no CORS.
 */
export const environment = {
  apiBaseUrl: '/api',
};
