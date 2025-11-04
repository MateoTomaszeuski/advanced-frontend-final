import { useEffect, useState } from 'react';
import { MainLayout } from '../components/layout/MainLayout';
import { Button } from '../components/forms/Button';
import { useUserStore } from '../stores/useUserStore';
import { spotifyApi } from '../services/api';
import { useAuth } from 'react-oidc-context';
import toast from 'react-hot-toast';

export function SettingsPage() {
  const { updateSpotifyConnection } = useUserStore();
  const auth = useAuth();
  const [spotifyStatus, setSpotifyStatus] = useState<{
    isConnected: boolean;
    isTokenValid: boolean;
    tokenExpiry?: string;
  } | null>(null);
  const [spotifyProfile, setSpotifyProfile] = useState<{
    id: string;
    displayName: string;
    email: string;
    country?: string;
    imageUrl?: string;
  } | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    if (!auth.isAuthenticated) return;
    
    const checkSpotifyStatus = async () => {
      try {
        const status = await spotifyApi.getStatus();
        setSpotifyStatus(status);
        updateSpotifyConnection(status.isConnected && status.isTokenValid);
        
        if (status.isConnected && status.isTokenValid) {
          try {
            const profile = await spotifyApi.getProfile() as {
              id: string;
              displayName: string;
              email: string;
              country?: string;
              imageUrl?: string;
            };
            setSpotifyProfile(profile);
          } catch (error) {
            console.error('Failed to fetch Spotify profile:', error);
          }
        } else {
          setSpotifyProfile(null);
        }
      } catch (error) {
        console.error('Failed to check Spotify status:', error);
      }
    };
    checkSpotifyStatus();
  }, [auth.isAuthenticated, updateSpotifyConnection]);

  useEffect(() => {
    const hash = window.location.hash;
    const search = window.location.search;
    console.log('Settings page loaded, hash:', hash, 'search:', search);
    
    // Handle hash-based response (implicit flow)
    if (hash) {
      const params = new URLSearchParams(hash.substring(1));
      const accessToken = params.get('access_token');
      const expiresIn = params.get('expires_in');
      const error = params.get('error');

      console.log('OAuth callback params (hash):', { 
        hasAccessToken: !!accessToken, 
        expiresIn, 
        error 
      });

      if (error) {
        toast.error(`Spotify authorization failed: ${error}`);
        window.history.replaceState(null, '', window.location.pathname);
        return;
      }

      if (accessToken && expiresIn) {
        const connectSpotify = async (token: string, expires: number) => {
          try {
            console.log('Attempting to connect Spotify...');
            await spotifyApi.connect({
              accessToken: token,
              expiresIn: expires,
            });
            toast.success('Spotify account connected successfully!');
            const status = await spotifyApi.getStatus();
            setSpotifyStatus(status);
            updateSpotifyConnection(true);
            
            // Fetch the profile after connecting
            try {
              const profile = await spotifyApi.getProfile() as {
                id: string;
                displayName: string;
                email: string;
                country?: string;
                imageUrl?: string;
              };
              setSpotifyProfile(profile);
            } catch (error) {
              console.error('Failed to fetch Spotify profile:', error);
            }
          } catch (error) {
            toast.error('Failed to connect Spotify account');
            console.error('Spotify connection error:', error);
          }
        };
        connectSpotify(accessToken, parseInt(expiresIn));
        window.history.replaceState(null, '', window.location.pathname);
      }
    }

    // Handle query-based response (authorization code flow)
    if (search) {
      const params = new URLSearchParams(search);
      const code = params.get('code');
      const error = params.get('error');

      console.log('OAuth callback params (query):', { 
        hasCode: !!code, 
        error 
      });

      if (error) {
        toast.error(`Spotify authorization failed: ${error}`);
        window.history.replaceState(null, '', window.location.pathname);
        return;
      }

      if (code) {
        const exchangeCode = async (authCode: string) => {
          try {
            console.log('Exchanging authorization code...');
            const redirectUri = import.meta.env.VITE_SPOTIFY_REDIRECT_URI || 'https://127.0.0.1:5173/settings';
            await spotifyApi.exchangeCode(authCode, redirectUri);
            toast.success('Spotify account connected successfully!');
            const status = await spotifyApi.getStatus();
            setSpotifyStatus(status);
            updateSpotifyConnection(true);
            
            // Fetch the profile after connecting
            try {
              const profile = await spotifyApi.getProfile() as {
                id: string;
                displayName: string;
                email: string;
                country?: string;
                imageUrl?: string;
              };
              setSpotifyProfile(profile);
            } catch (error) {
              console.error('Failed to fetch Spotify profile:', error);
            }
          } catch (error) {
            toast.error('Failed to exchange authorization code');
            console.error('Code exchange error:', error);
          }
        };
        exchangeCode(code);
        window.history.replaceState(null, '', window.location.pathname);
      }
    }
  }, [updateSpotifyConnection]);

  const handleConnectSpotify = () => {
    const clientId = import.meta.env.VITE_SPOTIFY_CLIENT_ID;
    const redirectUri = import.meta.env.VITE_SPOTIFY_REDIRECT_URI || 'https://127.0.0.1:5173/settings';

    if (!clientId || clientId === 'your-spotify-client-id') {
      toast.error('Spotify Client ID not configured. Please check environment variables.');
      return;
    }

    const scopes = [
      'playlist-modify-public',
      'playlist-modify-private',
      'user-library-read',
      'user-top-read',
    ].join(' ');

    // Using authorization code flow instead of implicit grant
    const authUrl = `https://accounts.spotify.com/authorize?client_id=${clientId}&response_type=code&redirect_uri=${encodeURIComponent(redirectUri)}&scope=${encodeURIComponent(scopes)}`;
    window.location.href = authUrl;
  };

  const handleDisconnectSpotify = async () => {
    setIsLoading(true);
    try {
      await spotifyApi.disconnect();
      setSpotifyStatus({ isConnected: false, isTokenValid: false });
      setSpotifyProfile(null);
      updateSpotifyConnection(false);
      toast.success('Spotify account disconnected');
    } catch (error) {
      toast.error('Failed to disconnect Spotify account');
      console.error('Spotify disconnection error:', error);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <MainLayout>
      <div className="max-w-4xl mx-auto">
        <h1 className="text-3xl font-bold text-gray-900 mb-8">Settings</h1>

        <div className="bg-white rounded-lg shadow divide-y divide-gray-200">
          <div className="p-6">
            <h2 className="text-xl font-semibold text-gray-900 mb-4">Keycloak Account</h2>
            <div className="space-y-4">
              <div>
                <label className="text-sm font-medium text-gray-700">Name</label>
                <p className="text-gray-900">{auth.user?.profile?.name || 'Not set'}</p>
              </div>
              <div>
                <label className="text-sm font-medium text-gray-700">Email</label>
                <p className="text-gray-900">{auth.user?.profile?.email || 'Not set'}</p>
              </div>
              <div>
                <label className="text-sm font-medium text-gray-700">Username</label>
                <p className="text-gray-900">{auth.user?.profile?.preferred_username || 'Not set'}</p>
              </div>
            </div>
          </div>

          <div className="p-6">
            <h2 className="text-xl font-semibold text-gray-900 mb-4">Spotify Account</h2>
            <div className="space-y-4">
              <div>
                <label className="text-sm font-medium text-gray-700">Connection Status</label>
                <p className="text-gray-900">
                  {spotifyStatus?.isConnected && spotifyStatus?.isTokenValid ? (
                    <span className="text-green-600 font-medium">✓ Connected</span>
                  ) : (
                    <span className="text-gray-500">Not connected</span>
                  )}
                </p>
              </div>
              {spotifyProfile && (
                <>
                  {spotifyProfile.imageUrl && (
                    <div>
                      <label className="text-sm font-medium text-gray-700">Profile Picture</label>
                      <img 
                        src={spotifyProfile.imageUrl} 
                        alt={spotifyProfile.displayName}
                        className="mt-2 h-16 w-16 rounded-full"
                      />
                    </div>
                  )}
                  <div>
                    <label className="text-sm font-medium text-gray-700">Display Name</label>
                    <p className="text-gray-900">{spotifyProfile.displayName}</p>
                  </div>
                  <div>
                    <label className="text-sm font-medium text-gray-700">Email</label>
                    <p className="text-gray-900">{spotifyProfile.email}</p>
                  </div>
                  {spotifyProfile.country && (
                    <div>
                      <label className="text-sm font-medium text-gray-700">Country</label>
                      <p className="text-gray-900">{spotifyProfile.country}</p>
                    </div>
                  )}
                </>
              )}
              {spotifyStatus?.tokenExpiry && (
                <div>
                  <label className="text-sm font-medium text-gray-700">Token Expires</label>
                  <p className="text-gray-900 text-sm">
                    {new Date(spotifyStatus.tokenExpiry).toLocaleString()}
                  </p>
                </div>
              )}
            </div>
          </div>

          <div className="p-6">
            <h2 className="text-xl font-semibold text-gray-900 mb-4">Spotify Connection</h2>
            <div className="flex items-center justify-between">
              <div>
                <p className="text-gray-900 font-medium">
                  {spotifyStatus?.isConnected && spotifyStatus?.isTokenValid ? 'Connected' : 'Not Connected'}
                </p>
                <p className="text-sm text-gray-600">
                  Connect your Spotify account to enable playlist management
                </p>
              </div>
              {spotifyStatus?.isConnected && spotifyStatus?.isTokenValid ? (
                <Button variant="danger" onClick={handleDisconnectSpotify} disabled={isLoading}>
                  {isLoading ? 'Disconnecting...' : 'Disconnect'}
                </Button>
              ) : (
                <Button variant="primary" onClick={handleConnectSpotify}>
                  Connect Spotify
                </Button>
              )}
            </div>
          </div>

          <div className="p-6">
            <h2 className="text-xl font-semibold text-gray-900 mb-4">Agent Preferences</h2>
            <div className="space-y-4">
              <div className="flex items-center justify-between">
                <div>
                  <p className="font-medium text-gray-900">Auto-approve actions</p>
                  <p className="text-sm text-gray-600">
                    Automatically approve non-destructive agent actions
                  </p>
                </div>
                <label className="relative inline-flex items-center cursor-pointer">
                  <input type="checkbox" className="sr-only peer" />
                  <div className="w-11 h-6 bg-gray-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-green-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-0.5 after:left-0.5 after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-green-600"></div>
                </label>
              </div>

              <div className="flex items-center justify-between">
                <div>
                  <p className="font-medium text-gray-900">Notifications</p>
                  <p className="text-sm text-gray-600">
                    Receive notifications when agent completes tasks
                  </p>
                </div>
                <label className="relative inline-flex items-center cursor-pointer">
                  <input type="checkbox" defaultChecked className="sr-only peer" />
                  <div className="w-11 h-6 bg-gray-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-green-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-0.5 after:left-0.5 after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-green-600"></div>
                </label>
              </div>
            </div>
          </div>

          <div className="p-6">
            <h2 className="text-xl font-semibold text-red-600 mb-4">Danger Zone</h2>
            <div className="space-y-4">
              <div className="flex items-center justify-between">
                <div>
                  <p className="font-medium text-gray-900">Clear agent history</p>
                  <p className="text-sm text-gray-600">
                    Delete all agent action history
                  </p>
                </div>
                <Button variant="danger" size="sm">
                  Clear History
                </Button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </MainLayout>
  );
}
