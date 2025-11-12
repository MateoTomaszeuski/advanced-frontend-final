import { useState, useEffect, useRef } from 'react';
import { MainLayout } from '../components/layout/MainLayout';
import { SpotifyConnectionAlert } from '../components/SpotifyConnectionAlert';
import { InfoBox } from '../components/InfoBox';
import { Button } from '../components/forms/Button';
import { SelectDropdown } from '../components/forms/SelectDropdown';
import { useAgent } from '../hooks/useAgent';
import { useAgentStore } from '../stores/useAgentStore';
import { spotifyApi } from '../services/api';
import { useAuth } from 'react-oidc-context';
import toast from 'react-hot-toast';
import type { AgentActionResult } from '../types/api';

export function DiscoverPage() {
  const [limit, setLimit] = useState('10');
  const [spotifyConnected, setSpotifyConnected] = useState(false);
  const [checkingConnection, setCheckingConnection] = useState(true);
  const conversationCreated = useRef(false);
  const auth = useAuth();
  
  const { discoverNewMusic, createConversation, isLoading } = useAgent();
  const { currentConversation, setCurrentConversation, recentActions } = useAgentStore();
  const agentStatus = useAgentStore((state) => state.status);
  const currentTask = useAgentStore((state) => state.currentTask);

  useEffect(() => {
    if (!auth.isAuthenticated) {
      setCheckingConnection(false);
      return;
    }
    
    const checkSpotifyConnection = async () => {
      try {
        const status = await spotifyApi.getStatus();
        setSpotifyConnected(status.isConnected && status.isTokenValid);
      } catch (error) {
        console.error('Failed to check Spotify connection:', error);
        setSpotifyConnected(false);
      } finally {
        setCheckingConnection(false);
      }
    };
    checkSpotifyConnection();
  }, [auth.isAuthenticated]);

  useEffect(() => {
    const initConversation = async () => {
      if (!currentConversation && spotifyConnected && !conversationCreated.current) {
        conversationCreated.current = true;
        try {
          const conv = await createConversation('Music Discovery');
          setCurrentConversation(conv);
        } catch (error) {
          console.error('Failed to create conversation:', error);
          conversationCreated.current = false;
        }
      }
    };
    initConversation();
  }, [currentConversation, spotifyConnected, createConversation, setCurrentConversation]);

  const handleDiscover = async () => {
    if (!currentConversation) {
      toast.error('No conversation found. Please refresh the page.');
      return;
    }

    try {
      await discoverNewMusic({
        conversationId: currentConversation.id,
        limit: parseInt(limit),
      });
    } catch (error) {
      console.error('Failed to discover music:', error);
    }
  };

  const lastDiscovery = recentActions.find(a => a.actionType === 'DiscoverNewMusic');
  const discoveryResult = lastDiscovery?.result as AgentActionResult | undefined;

  return (
    <MainLayout>
      <div className="max-w-4xl mx-auto">
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900 mb-2">New Music Discovery</h1>
          <p className="text-gray-600">
            Discover fresh tracks based on your listening habits
          </p>
        </div>

        <div className="mb-6">
          <InfoBox
            type="info"
            items={[
              'Analyzes your saved tracks and listening patterns',
              'Uses Spotify\'s recommendation algorithm',
              'Filters out songs you\'ve already saved',
              'Creates a new playlist with fresh discoveries',
            ]}
          />
        </div>

        {!checkingConnection && !spotifyConnected && <SpotifyConnectionAlert />}

        {agentStatus === 'processing' && currentTask && (
          <div className="mb-6 bg-green-50 border-2 border-green-300 rounded-lg p-5 shadow-sm">
            <div className="flex items-start gap-4">
              <div className="shrink-0 mt-1">
                <div className="relative">
                  <div className="animate-spin h-6 w-6 border-3 border-green-600 border-t-transparent rounded-full"></div>
                  <div className="absolute inset-0 animate-pulse">
                    <div className="h-6 w-6 border-3 border-green-300 border-t-transparent rounded-full"></div>
                  </div>
                </div>
              </div>
              <div className="flex-1 min-w-0">
                <div className="flex items-center gap-2 mb-1">
                  <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-600 text-white">
                    PROCESSING
                  </span>
                  <span className="text-xs text-green-600 font-mono">
                    {new Date().toLocaleTimeString()}
                  </span>
                </div>
                <p className="font-semibold text-green-900 text-lg leading-snug">
                  {currentTask}
                </p>
                <p className="text-sm text-green-700 mt-1">
                  Please wait while the AI agent completes this operation...
                </p>
              </div>
            </div>
            <div className="mt-3 h-1.5 bg-green-200 rounded-full overflow-hidden">
              <div className="h-full bg-green-600 rounded-full animate-pulse" style={{ width: '100%' }}></div>
            </div>
          </div>
        )}

        <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6 mb-6">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">Discovery Settings</h2>
          
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Number of Tracks to Discover
              </label>
              <SelectDropdown
                value={limit}
                onChange={(e) => setLimit(e.target.value)}
                options={[
                  { value: '5', label: '5 tracks' },
                  { value: '10', label: '10 tracks' },
                  { value: '15', label: '15 tracks' },
                  { value: '20', label: '20 tracks' },
                  { value: '30', label: '30 tracks' },
                  { value: '50', label: '50 tracks' },
                  { value: '75', label: '75 tracks' },
                  { value: '100', label: '100 tracks' },
                  { value: '150', label: '150 tracks' },
                  { value: '200', label: '200 tracks' },
                  { value: '250', label: '250 tracks' },
                ]}
                disabled={isLoading}
              />
            </div>

            <Button
              variant="primary"
              onClick={handleDiscover}
              disabled={isLoading}
              isLoading={isLoading}
              className="w-full mt-4"
            >
              Discover New Music
            </Button>
          </div>
        </div>

        {discoveryResult && (
          <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
            <h2 className="text-lg font-semibold text-gray-900 mb-4">Latest Discovery</h2>
            
            <div className="mb-4">
              <div className="flex items-center justify-between mb-2">
                <h3 className="font-medium text-gray-900">
                  {discoveryResult.playlistName}
                </h3>
                <span className="text-sm text-gray-500">
                  {discoveryResult.trackCount} tracks
                </span>
              </div>
              <a
                href={discoveryResult.playlistUri}
                target="_blank"
                rel="noopener noreferrer"
                className="text-sm text-green-600 hover:text-green-700 font-medium"
              >
                Open in Spotify →
              </a>
            </div>

            {discoveryResult.tracks && discoveryResult.tracks.length > 0 && (
              <div className="space-y-2">
                <h4 className="text-sm font-medium text-gray-700">Discovered Tracks:</h4>
                <div className="max-h-64 overflow-y-auto space-y-2">
                  {discoveryResult.tracks.map((track, index) => {
                    const trackData = track as Record<string, unknown>;
                    const trackName = (trackData.name || trackData.Name || 'Unknown Track') as string;
                    const trackArtists = (trackData.artists || trackData.Artists || 'Unknown Artist') as string;
                    const trackId = (trackData.id || trackData.Id || index) as string | number;
                    
                    return (
                      <div
                        key={trackId}
                        className="flex items-center gap-3 p-2 bg-gray-50 rounded-lg"
                      >
                        <span className="text-sm text-gray-500 w-6">{index + 1}</span>
                        <div className="flex-1 min-w-0">
                          <p className="text-sm font-medium text-gray-900 truncate">
                            {trackName}
                          </p>
                          <p className="text-xs text-gray-600 truncate">
                            {trackArtists}
                          </p>
                        </div>
                      </div>
                    );
                  })}
                </div>
              </div>
            )}
          </div>
        )}
      </div>
    </MainLayout>
  );
}
