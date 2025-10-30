import { MainLayout } from '../components/layout/MainLayout';
import { Button } from '../components/forms/Button';
import { useUserStore } from '../stores/useUserStore';
import { useAuth } from 'react-oidc-context';

export function SettingsPage() {
  const { user, updateSpotifyConnection } = useUserStore();
  const auth = useAuth();

  const handleConnectSpotify = () => {
    // TODO: Implement Spotify OAuth connection
    updateSpotifyConnection(true);
  };

  const handleDisconnectSpotify = () => {
    updateSpotifyConnection(false);
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
                <label className="text-sm font-medium text-gray-700">Spotify User</label>
                <p className="text-gray-900">{user?.name || 'Not connected'}</p>
              </div>
              <div>
                <label className="text-sm font-medium text-gray-700">Spotify Email</label>
                <p className="text-gray-900">{user?.email || 'Not connected'}</p>
              </div>
            </div>
          </div>

          <div className="p-6">
            <h2 className="text-xl font-semibold text-gray-900 mb-4">Spotify Connection</h2>
            <div className="flex items-center justify-between">
              <div>
                <p className="text-gray-900 font-medium">
                  {user?.spotifyConnected ? 'Connected' : 'Not Connected'}
                </p>
                <p className="text-sm text-gray-600">
                  Connect your Spotify account to enable playlist management
                </p>
              </div>
              {user?.spotifyConnected ? (
                <Button variant="danger" onClick={handleDisconnectSpotify}>
                  Disconnect
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
                  <div className="w-11 h-6 bg-gray-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-green-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-green-600"></div>
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
                  <div className="w-11 h-6 bg-gray-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-green-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-green-600"></div>
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
