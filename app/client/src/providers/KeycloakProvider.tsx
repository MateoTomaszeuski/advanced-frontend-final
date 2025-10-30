import { AuthProvider } from 'react-oidc-context';
import type { AuthProviderProps } from 'react-oidc-context';
import { config } from '../config';

const oidcConfig: AuthProviderProps = {
  authority: config.keycloak.authority,
  client_id: config.keycloak.clientId,
  redirect_uri: window.location.origin,
  post_logout_redirect_uri: window.location.origin,
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
