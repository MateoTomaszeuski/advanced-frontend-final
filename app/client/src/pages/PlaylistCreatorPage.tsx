import { useState, useEffect } from 'react';
import { MainLayout } from '../components/layout/MainLayout';
import { SpotifyConnectionAlert } from '../components/SpotifyConnectionAlert';
import { Button } from '../components/forms/Button';
import { TextInput } from '../components/forms/TextInput';
import { SelectDropdown } from '../components/forms/SelectDropdown';
import { Checkbox } from '../components/forms/Checkbox';
import { useAgent } from '../hooks/useAgent';
import { useAgentStore } from '../stores/useAgentStore';
import { spotifyApi } from '../services/api';
import { useAuth } from 'react-oidc-context';
import toast from 'react-hot-toast';
import type { PlaylistPreferences } from '../types/api';

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
      if (!currentConversation && spotifyConnected) {
        try {
          const conv = await createConversation('Smart Playlist Creation');
          setCurrentConversation(conv);
        } catch (error) {
          console.error('Failed to create conversation:', error);
        }
      }
    };
    initConversation();
  }, [currentConversation, createConversation, setCurrentConversation, spotifyConnected]);

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

        {!checkingConnection && !spotifyConnected && <SpotifyConnectionAlert />}

        {agentStatus === 'processing' && (
          <div className="mb-6 bg-green-50 border border-green-200 rounded-lg p-4">
            <div className="flex items-center gap-3">
              <div className="animate-spin h-5 w-5 border-2 border-green-600 border-t-transparent rounded-full"></div>
              <div>
                <p className="font-medium text-green-900">{currentTask}</p>
                <p className="text-sm text-green-700">This may take a moment...</p>
              </div>
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

        <div className="mt-6 bg-green-50 border border-green-200 rounded-lg p-4">
          <h3 className="font-medium text-green-900 mb-2">💡 Tips</h3>
          <ul className="text-sm text-green-800 space-y-1">
            <li>• Be specific about the mood or activity (workout, study, party, etc.)</li>
            <li>• Mention genres if you have a preference</li>
            <li>• Use advanced options to fine-tune energy and tempo</li>
            <li>• The AI will automatically name your playlist based on your description</li>
          </ul>
        </div>
      </div>
    </MainLayout>
  );
}
