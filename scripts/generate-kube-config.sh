#!/bin/bash

# Load environment variables from .env.production
set -a
source app/client/.env.production
set +a

# Generate frontend-config.yml
cat > kube/frontend-config.yml <<EOFCONFIG
apiVersion: v1
kind: ConfigMap
metadata:
  name: mateo-spotify-frontend-config
  namespace: mateo-spotify
data:
  VITE_KEYCLOAK_CLIENT_ID: "$VITE_KEYCLOAK_CLIENT_ID"
  VITE_KEYCLOAK_AUTHORITY: "$VITE_KEYCLOAK_AUTHORITY"
  VITE_KEYCLOAK_JWKS_URI: "$VITE_KEYCLOAK_JWKS_URI"
  VITE_API_URL: "$VITE_API_URL"
  VITE_SPOTIFY_CLIENT_ID: "$VITE_SPOTIFY_CLIENT_ID"
  VITE_SPOTIFY_REDIRECT_URI: "$VITE_SPOTIFY_REDIRECT_URI"
EOFCONFIG

echo "Generated kube/frontend-config.yml from app/client/.env.production"
