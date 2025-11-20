import { useState, useEffect } from 'react';
import { MainLayout } from '../components/layout/MainLayout';
import { SpotifyConnectionAlert } from '../components/SpotifyConnectionAlert';
import { InfoBox } from '../components/InfoBox';
import { AgentStatusBanner } from '../components/playlist-creator/AgentStatusBanner';
import { PlaylistForm } from '../components/playlist-creator/PlaylistForm';
import { RecentPlaylists } from '../components/playlist-creator/RecentPlaylists';
import { useAgent } from '../hooks/useAgent';
import { useAgentStore } from '../stores/useAgentStore';
import { spotifyApi, agentApi } from '../services/api';
import { useAuth } from 'react-oidc-context';
import { showToast } from '../utils/toast';
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
  const [spotifyConnected, setSpotifyConnected] = useState(false);
  const [checkingConnection, setCheckingConnection] = useState(true);
  const [recentPlaylists, setRecentPlaylists] = useState<RecentPlaylist[]>([]);
  const [loadingRecent, setLoadingRecent] = useState(false);
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
      showToast.error('Please enter a playlist description');
      return;
    }

    const preferences: PlaylistPreferences = {
      maxTracks: parseInt(maxTracks) || 20,
    };

    try {
      let conversationId = currentConversation?.id;
      
      if (!conversationId) {
        const conv = await createConversation('Smart Playlist Creation');
        setCurrentConversation(conv);
        conversationId = conv.id;
      }

      await createSmartPlaylist({
        conversationId,
        prompt,
        preferences,
      });
      setPrompt('');

      try {
        const data = (await agentApi.getRecentPlaylists(10)) as RecentPlaylist[];
        setRecentPlaylists(data);
      } catch (error) {
        console.error('Failed to refresh recent playlists:', error);
      }
    } catch (error) {
      console.error('Failed to create playlist:', error);
    }
  };

  const handleClear = () => {
    setPrompt('');
  };

  return (
    <MainLayout>
      <div className="max-w-4xl mx-auto">
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-theme-text mb-2">Playlist Creator</h1>
          <p className="text-theme-text opacity-80">
            Describe your perfect playlist and let AI create it for you
          </p>
        </div>

        <div className="mb-6">
          <InfoBox
            type="tips"
            items={[
              'Be specific about the mood or activity (workout, study, party, etc.)',
              'Mention genres if you have a preference',
              'The AI will automatically name your playlist based on your description',
            ]}
          />
        </div>

        {!checkingConnection && !spotifyConnected && <SpotifyConnectionAlert />}

        {agentStatus === 'processing' && currentTask && (
          <AgentStatusBanner task={currentTask} />
        )}

        <PlaylistForm
          prompt={prompt}
          setPrompt={setPrompt}
          maxTracks={maxTracks}
          setMaxTracks={setMaxTracks}
          isLoading={isLoading}
          onSubmit={handleSubmit}
          onClear={handleClear}
        />

        {spotifyConnected && (
          <RecentPlaylists playlists={recentPlaylists} isLoading={loadingRecent} />
        )}
      </div>
    </MainLayout>
  );
}
