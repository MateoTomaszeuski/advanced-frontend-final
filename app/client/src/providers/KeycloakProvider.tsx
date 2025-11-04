import { AuthProvider } from 'react-oidc-context';
import type { AuthProviderProps } from 'react-oidc-context';
import { config } from '../config';

const getRedirectUri = () => {
  if (import.meta.env.DEV) {
    return 'https://127.0.0.1:5173';
  }
  return window.location.origin;
};

const oidcConfig: AuthProviderProps = {
  authority: config.keycloak.authority,
  client_id: config.keycloak.clientId,
  redirect_uri: getRedirectUri(),
  post_logout_redirect_uri: getRedirectUri(),
  response_type: 'code',
  scope: 'openid profile email',
  automaticSilentRenew: true,
  loadUserInfo: true,
};

interface KeycloakProviderProps {
  children: React.ReactNode;
}

export function KeycloakProvider({ children }: KeycloakProviderProps) {
  return <AuthProvider {...oidcConfig}>{children}</AuthProvider>;
}
