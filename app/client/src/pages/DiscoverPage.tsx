import { useState, useEffect, useRef } from 'react';
import { MainLayout } from '../components/layout/MainLayout';
import { SpotifyConnectionAlert } from '../components/SpotifyConnectionAlert';
import { InfoBox } from '../components/InfoBox';
import { DiscoverySettings } from '../components/discover/DiscoverySettings';
import { LatestDiscovery } from '../components/discover/LatestDiscovery';
import { AgentStatusBanner } from '../components/playlist-creator/AgentStatusBanner';
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
          <AgentStatusBanner task={currentTask} />
        )}

        <DiscoverySettings
          limit={limit}
          setLimit={setLimit}
          isLoading={isLoading}
          onDiscover={handleDiscover}
        />

        {discoveryResult && <LatestDiscovery discoveryResult={discoveryResult} />}
      </div>
    </MainLayout>
  );
}
