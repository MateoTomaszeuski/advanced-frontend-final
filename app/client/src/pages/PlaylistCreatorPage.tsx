import { useState, useEffect, useRef } from 'react';
import { MainLayout } from '../components/layout/MainLayout';
import { SpotifyConnectionAlert } from '../components/SpotifyConnectionAlert';
import { InfoBox } from '../components/InfoBox';
import { Button } from '../components/forms/Button';
import { TextInput } from '../components/forms/TextInput';
import { SelectDropdown } from '../components/forms/SelectDropdown';
import { Checkbox } from '../components/forms/Checkbox';
import { useAgent } from '../hooks/useAgent';
import { useAgentStore } from '../stores/useAgentStore';
import { spotifyApi, agentApi } from '../services/api';
import { useAuth } from 'react-oidc-context';
import toast from 'react-hot-toast';
import type { PlaylistPreferences } from '../types/api';

interface RecentPlaylist {
  id: number;
  actionType: string;
  inputPrompt: string;
  result: {
    playlistId: string;
    playlistName: string;
    playlistUri: string;
    trackCount: number;
  };
  createdAt: string;
}

export function PlaylistCreatorPage() {
  const [prompt, setPrompt] = useState('');
  const [maxTracks, setMaxTracks] = useState('20');
  const [minEnergy, setMinEnergy] = useState('');
  const [maxEnergy, setMaxEnergy] = useState('');
  const [minTempo, setMinTempo] = useState('');
  const [maxTempo, setMaxTempo] = useState('');
  const [useAdvanced, setUseAdvanced] = useState(false);
  const [spotifyConnected, setSpotifyConnected] = useState(false);
  const [checkingConnection, setCheckingConnection] = useState(true);
  const [recentPlaylists, setRecentPlaylists] = useState<RecentPlaylist[]>([]);
  const [loadingRecent, setLoadingRecent] = useState(false);
  const conversationCreated = useRef(false);
  const auth = useAuth();
  
  const { createSmartPlaylist, createConversation, isLoading } = useAgent();
  const { currentConversation, setCurrentConversation } = useAgentStore();
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
          const conv = await createConversation('Smart Playlist Creation');
          setCurrentConversation(conv);
        } catch (error) {
          console.error('Failed to create conversation:', error);
          conversationCreated.current = false;
        }
      }
    };
    initConversation();
  }, [currentConversation, spotifyConnected, createConversation, setCurrentConversation]);

  useEffect(() => {
    const fetchRecent = async () => {
      if (!spotifyConnected || !auth.isAuthenticated) return;
      
      setLoadingRecent(true);
      try {
        const data = await agentApi.getRecentPlaylists(10) as RecentPlaylist[];
        setRecentPlaylists(data);
      } catch (error) {
        console.error('Failed to fetch recent playlists:', error);
      } finally {
        setLoadingRecent(false);
      }
    };
    fetchRecent();
  }, [spotifyConnected, auth.isAuthenticated]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!prompt.trim()) {
      toast.error('Please enter a playlist description');
      return;
    }

    if (!currentConversation) {
      toast.error('No conversation found. Please refresh the page.');
      return;
    }

    const preferences: PlaylistPreferences = {
      maxTracks: parseInt(maxTracks) || 20,
    };

    if (useAdvanced) {
      if (minEnergy) preferences.minEnergy = parseInt(minEnergy);
      if (maxEnergy) preferences.maxEnergy = parseInt(maxEnergy);
      if (minTempo) preferences.minTempo = parseInt(minTempo);
      if (maxTempo) preferences.maxTempo = parseInt(maxTempo);
    }

    try {
      await createSmartPlaylist({
        conversationId: currentConversation.id,
        prompt,
        preferences,
      });
      setPrompt('');
      
      try {
        const data = await agentApi.getRecentPlaylists(10) as RecentPlaylist[];
        setRecentPlaylists(data);
      } catch (error) {
        console.error('Failed to refresh recent playlists:', error);
      }
    } catch (error) {
      console.error('Failed to create playlist:', error);
    }
  };

  return (
    <MainLayout>
      <div className="max-w-4xl mx-auto">
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900 mb-2">Playlist Creator</h1>
          <p className="text-gray-600">
            Describe your perfect playlist and let AI create it for you
          </p>
        </div>

        <div className="mb-6">
          <InfoBox
            type="tips"
            items={[
              'Be specific about the mood or activity (workout, study, party, etc.)',
              'Mention genres if you have a preference',
              'Use advanced options to fine-tune energy and tempo',
              'The AI will automatically name your playlist based on your description',
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

        <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
          <form onSubmit={handleSubmit} className="space-y-6">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Playlist Description *
              </label>
              <TextInput
                value={prompt}
                onChange={(e) => setPrompt(e.target.value)}
                placeholder="e.g., Create a workout playlist with high-energy rock songs"
                disabled={isLoading}
              />
              <p className="mt-2 text-sm text-gray-500">
                Examples: "chill vibes for studying", "upbeat party mix", "focus music for work"
              </p>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Number of Tracks
              </label>
              <SelectDropdown
                value={maxTracks}
                onChange={(e) => setMaxTracks(e.target.value)}
                options={[
                  { value: '10', label: '10 tracks' },
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

            <div>
              <Checkbox
                label="Advanced Options"
                checked={useAdvanced}
                onChange={(e) => setUseAdvanced(e.target.checked)}
                disabled={isLoading}
              />
            </div>

            {useAdvanced && (
              <div className="space-y-4 pl-6 border-l-2 border-green-200">
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Min Energy (0-100)
                    </label>
                    <TextInput
                      type="number"
                      min="0"
                      max="100"
                      value={minEnergy}
                      onChange={(e) => setMinEnergy(e.target.value)}
                      placeholder="0"
                      disabled={isLoading}
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Max Energy (0-100)
                    </label>
                    <TextInput
                      type="number"
                      min="0"
                      max="100"
                      value={maxEnergy}
                      onChange={(e) => setMaxEnergy(e.target.value)}
                      placeholder="100"
                      disabled={isLoading}
                    />
                  </div>
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Min Tempo (BPM)
                    </label>
                    <TextInput
                      type="number"
                      min="0"
                      max="300"
                      value={minTempo}
                      onChange={(e) => setMinTempo(e.target.value)}
                      placeholder="60"
                      disabled={isLoading}
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Max Tempo (BPM)
                    </label>
                    <TextInput
                      type="number"
                      min="0"
                      max="300"
                      value={maxTempo}
                      onChange={(e) => setMaxTempo(e.target.value)}
                      placeholder="180"
                      disabled={isLoading}
                    />
                  </div>
                </div>
              </div>
            )}

            <div className="flex gap-3 pt-4">
              <Button
                type="submit"
                variant="primary"
                disabled={isLoading || !prompt.trim()}
                isLoading={isLoading}
              >
                Create Playlist
              </Button>
              <Button
                type="button"
                variant="secondary"
                onClick={() => {
                  setPrompt('');
                  setUseAdvanced(false);
                  setMinEnergy('');
                  setMaxEnergy('');
                  setMinTempo('');
                  setMaxTempo('');
                }}
                disabled={isLoading}
              >
                Clear
              </Button>
            </div>
          </form>
        </div>

        {spotifyConnected && recentPlaylists.length > 0 && (
          <div className="mt-8">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-2xl font-bold text-gray-900">Recently Created Playlists</h2>
              {loadingRecent && (
                <div className="animate-spin h-5 w-5 border-2 border-green-600 border-t-transparent rounded-full"></div>
              )}
            </div>
            <div className="space-y-3">
              {recentPlaylists.map((playlist) => (
                <div
                  key={playlist.id}
                  className="bg-white rounded-lg shadow-sm border border-gray-200 p-4 hover:border-green-300 transition-colors"
                >
                  <div className="flex items-start justify-between gap-4">
                    <div className="flex-1 min-w-0">
                      <h3 className="font-semibold text-gray-900 truncate">
                        {playlist.result.playlistName}
                      </h3>
                      <p className="text-sm text-gray-600 mt-1 line-clamp-2">
                        "{playlist.inputPrompt}"
                      </p>
                      <div className="flex items-center gap-4 mt-2 text-xs text-gray-500">
                        <span>{playlist.result.trackCount} tracks</span>
                        <span>•</span>
                        <span>{new Date(playlist.createdAt).toLocaleDateString()}</span>
                      </div>
                    </div>
                    <a
                      href={playlist.result.playlistUri}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="shrink-0 inline-flex items-center px-4 py-2 text-sm font-medium text-white bg-green-600 hover:bg-green-700 rounded-md transition-colors"
                    >
                      <svg
                        className="w-4 h-4 mr-2"
                        fill="currentColor"
                        viewBox="0 0 24 24"
                      >
                        <path d="M12 0C5.4 0 0 5.4 0 12s5.4 12 12 12 12-5.4 12-12S18.66 0 12 0zm5.521 17.34c-.24.359-.66.48-1.021.24-2.82-1.74-6.36-2.101-10.561-1.141-.418.122-.779-.179-.899-.539-.12-.421.18-.78.54-.9 4.56-1.021 8.52-.6 11.64 1.32.42.18.479.659.301 1.02zm1.44-3.3c-.301.42-.841.6-1.262.3-3.239-1.98-8.159-2.58-11.939-1.38-.479.12-1.02-.12-1.14-.6-.12-.48.12-1.021.6-1.141C9.6 9.9 15 10.561 18.72 12.84c.361.181.54.78.241 1.2zm.12-3.36C15.24 8.4 8.82 8.16 5.16 9.301c-.6.179-1.2-.181-1.38-.721-.18-.601.18-1.2.72-1.381 4.26-1.26 11.28-1.02 15.721 1.621.539.3.719 1.02.419 1.56-.299.421-1.02.599-1.559.3z" />
                      </svg>
                      Open in Spotify
                    </a>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}
      </div>
    </MainLayout>
  );
}
