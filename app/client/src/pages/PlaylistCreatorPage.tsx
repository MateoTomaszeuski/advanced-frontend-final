import { useState, useEffect, useRef } from 'react';
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
    setUseAdvanced(false);
    setMinEnergy('');
    setMaxEnergy('');
    setMinTempo('');
    setMaxTempo('');
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
          <AgentStatusBanner task={currentTask} />
        )}

        <PlaylistForm
          prompt={prompt}
          setPrompt={setPrompt}
          maxTracks={maxTracks}
          setMaxTracks={setMaxTracks}
          useAdvanced={useAdvanced}
          setUseAdvanced={setUseAdvanced}
          minEnergy={minEnergy}
          setMinEnergy={setMinEnergy}
          maxEnergy={maxEnergy}
          setMaxEnergy={setMaxEnergy}
          minTempo={minTempo}
          setMinTempo={setMinTempo}
          maxTempo={maxTempo}
          setMaxTempo={setMaxTempo}
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
