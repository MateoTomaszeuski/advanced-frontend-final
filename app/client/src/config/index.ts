export const config = {
  keycloak: {
    clientId: import.meta.env.VITE_KEYCLOAK_CLIENT_ID,
    authority: import.meta.env.VITE_KEYCLOAK_AUTHORITY,
    jwksUri: import.meta.env.VITE_KEYCLOAK_JWKS_URI,
  },
  api: {
    baseUrl: import.meta.env.VITE_API_URL,
  },
} as const;
