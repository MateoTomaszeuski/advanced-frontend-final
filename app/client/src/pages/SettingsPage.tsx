import { useEffect, useState } from 'react';
import { MainLayout } from '../components/layout/MainLayout';
import { Button } from '../components/forms/Button';
import { useUserStore } from '../stores/useUserStore';
import { spotifyApi, agentApi } from '../services/api';
import { useAuth } from 'react-oidc-context';
import toast from 'react-hot-toast';
import { showToast } from '../utils/toast';

export function SettingsPage() {
  const { updateSpotifyConnection, preferences, updatePreferences } = useUserStore();
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
        showToast.error(`Spotify authorization failed: ${error}`);
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
            showToast.success('Spotify account connected successfully!');
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
            showToast.error('Failed to connect Spotify account');
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
        showToast.error(`Spotify authorization failed: ${error}`);
        window.history.replaceState(null, '', window.location.pathname);
        return;
      }

      if (code) {
        const exchangeCode = async (authCode: string) => {
          try {
            console.log('Exchanging authorization code...');
            const redirectUri = import.meta.env.VITE_SPOTIFY_REDIRECT_URI || 'https://127.0.0.1:5173/settings';
            await spotifyApi.exchangeCode(authCode, redirectUri);
            showToast.success('Spotify account connected successfully!');
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
            showToast.error('Failed to exchange authorization code');
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
      showToast.error('Spotify Client ID not configured. Please check environment variables.');
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
      showToast.success('Spotify account disconnected');
    } catch (error) {
      showToast.error('Failed to disconnect Spotify account');
      console.error('Spotify disconnection error:', error);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <MainLayout>
      <div className="max-w-5xl mx-auto">
        <h1 className="text-3xl font-bold text-gray-900 mb-8">Settings</h1>

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* Keycloak Account */}
          <div className="bg-white rounded-lg shadow p-6">
            <h2 className="text-xl font-semibold text-gray-900 mb-4 flex items-center gap-2">
              <span className="text-2xl">👤</span>
              Keycloak Account
            </h2>
            <div className="space-y-3">
              <div>
                <label className="text-xs font-medium text-gray-500 uppercase tracking-wide">Name</label>
                <p className="text-gray-900 mt-1">{auth.user?.profile?.name || 'Not set'}</p>
              </div>
              <div>
                <label className="text-xs font-medium text-gray-500 uppercase tracking-wide">Email</label>
                <p className="text-gray-900 mt-1">{auth.user?.profile?.email || 'Not set'}</p>
              </div>
              <div>
                <label className="text-xs font-medium text-gray-500 uppercase tracking-wide">Username</label>
                <p className="text-gray-900 mt-1">{auth.user?.profile?.preferred_username || 'Not set'}</p>
              </div>
            </div>
          </div>

          {/* Spotify Account */}
          <div className="bg-white rounded-lg shadow p-6">
            <h2 className="text-xl font-semibold text-gray-900 mb-4 flex items-center gap-2">
              <span className="text-2xl">🎵</span>
              Spotify Account
            </h2>
            
            {spotifyStatus?.isConnected && spotifyStatus?.isTokenValid && spotifyProfile ? (
              <div className="space-y-4">
                <div className="flex items-center gap-4">
                  {spotifyProfile.imageUrl && (
                    <img 
                      src={spotifyProfile.imageUrl} 
                      alt={spotifyProfile.displayName}
                      className="h-16 w-16 rounded-full ring-2 ring-green-500"
                    />
                  )}
                  <div className="flex-1">
                    <p className="font-semibold text-gray-900">{spotifyProfile.displayName}</p>
                    <p className="text-sm text-gray-600">{spotifyProfile.email}</p>
                    {spotifyProfile.country && (
                      <p className="text-xs text-gray-500 mt-1">📍 {spotifyProfile.country}</p>
                    )}
                  </div>
                </div>
                
                <div className="pt-3 border-t border-gray-200">
                  <div className="flex items-center justify-between mb-3">
                    <span className="text-sm font-medium text-gray-700">Status</span>
                    <span className="inline-flex items-center gap-1 text-sm text-green-600 font-medium">
                      <span className="w-2 h-2 bg-green-500 rounded-full animate-pulse"></span>
                      Connected
                    </span>
                  </div>
                  
                  {spotifyStatus?.tokenExpiry && (
                    <div className="text-xs text-gray-500 mb-3">
                      Token expires: {new Date(spotifyStatus.tokenExpiry).toLocaleString()}
                    </div>
                  )}
                  
                  <Button 
                    variant="danger" 
                    onClick={handleDisconnectSpotify} 
                    disabled={isLoading}
                    className="w-full"
                  >
                    {isLoading ? 'Disconnecting...' : 'Disconnect Spotify'}
                  </Button>
                </div>
              </div>
            ) : (
              <div className="space-y-4">
                <div className="text-center py-6">
                  <div className="text-4xl mb-3">🔌</div>
                  <p className="text-gray-600 mb-1">Not Connected</p>
                  <p className="text-sm text-gray-500">
                    Connect your Spotify account to enable playlist management and music discovery
                  </p>
                </div>
                <Button 
                  variant="primary" 
                  onClick={handleConnectSpotify}
                  className="w-full"
                >
                  Connect Spotify Account
                </Button>
              </div>
            )}
          </div>
        </div>

        {/* Agent Preferences */}
        <div className="bg-white rounded-lg shadow p-6 mt-6">
          <h2 className="text-xl font-semibold text-gray-900 mb-4 flex items-center gap-2">
            <span className="text-2xl">⚙️</span>
            Agent Preferences
          </h2>
          <div className="space-y-4">
            <div className="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
              <div className="flex-1">
                <p className="font-medium text-gray-900 flex items-center gap-2">
                  🔔 Notifications
                </p>
                <p className="text-sm text-gray-600 mt-1">
                  Show toast notifications when agent completes tasks or encounters errors
                </p>
              </div>
              <label className="relative inline-flex items-center cursor-pointer ml-4">
                <input 
                  type="checkbox" 
                  checked={preferences.notificationsEnabled}
                  onChange={(e) => {
                    updatePreferences({ notificationsEnabled: e.target.checked });
                    // Show a native toast to confirm the change (bypass the preference check)
                    if (e.target.checked) {
                      toast.success('Notifications enabled', {
                        duration: 2000,
                        style: {
                          background: '#065f46',
                          color: '#fff',
                          fontWeight: '500',
                        },
                      });
                    } else {
                      toast('Notifications disabled', {
                        duration: 2000,
                        style: {
                          background: '#6b7280',
                          color: '#fff',
                          fontWeight: '500',
                        },
                        icon: '🔕',
                      });
                    }
                  }}
                  className="sr-only peer" 
                />
                <div className="w-11 h-6 bg-gray-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-green-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-0.5 after:left-0.5 after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-green-600"></div>
              </label>
            </div>
          </div>
        </div>

        {/* Danger Zone */}
        <div className="bg-white rounded-lg shadow p-6 mt-6 border-2 border-red-200">
          <h2 className="text-xl font-semibold text-red-600 mb-4 flex items-center gap-2">
            <span className="text-2xl">⚠️</span>
            Danger Zone
          </h2>
          <div className="space-y-4">
            <div className="flex items-center justify-between p-4 bg-red-50 rounded-lg">
              <div className="flex-1">
                <p className="font-medium text-gray-900">Clear agent history</p>
                <p className="text-sm text-gray-600 mt-1">
                  Permanently delete all agent action history from the database. This action cannot be undone.
                </p>
              </div>
              <Button 
                variant="danger" 
                size="sm" 
                className="ml-4 whitespace-nowrap"
                onClick={async () => {
                  if (window.confirm('Are you sure you want to delete all history? This will permanently delete all your agent actions from the database. This action cannot be undone.')) {
                    try {
                      setIsLoading(true);
                      await agentApi.clearHistory();
                      showToast.success('History deleted');
                    } catch (error) {
                      console.error('Failed to clear history:', error);
                      showToast.error('Failed to clear history');
                    } finally {
                      setIsLoading(false);
                    }
                  }
                }}
                disabled={isLoading}
              >
                {isLoading ? 'Clearing...' : 'Clear History'}
              </Button>
            </div>
          </div>
        </div>
      </div>
    </MainLayout>
  );
}
