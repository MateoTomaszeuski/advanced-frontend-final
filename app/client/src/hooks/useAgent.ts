import { useState } from 'react';
import { useAgentStore } from '../stores/useAgentStore';
import { conversationApi, agentApi } from '../services/api';
import toast from 'react-hot-toast';
import type {
  CreateSmartPlaylistRequest,
  DiscoverNewMusicRequest,
} from '../types/api';

export function useAgent() {
  const [isLoading, setIsLoading] = useState(false);
  const {
    setStatus,
    setCurrentTask,
    setProgress,
    addRecentAction,
    setCurrentConversation,
    addConversation,
  } = useAgentStore();

  const createConversation = async (title: string) => {
    try {
      const conversation = await conversationApi.create({ title });
      addConversation(conversation);
      setCurrentConversation(conversation);
      toast.success('Conversation created');
      return conversation;
    } catch (error) {
      toast.error(
        error instanceof Error ? error.message : 'Failed to create conversation'
      );
      throw error;
    }
  };

  const createSmartPlaylist = async (data: CreateSmartPlaylistRequest) => {
    setIsLoading(true);
    setStatus('processing');
    setCurrentTask('Creating smart playlist...');
    setProgress(0);

    try {
      setProgress(30);
      const response = await agentApi.createSmartPlaylist(data);

      if (response.status === 'Failed') {
        throw new Error(response.errorMessage || 'Failed to create playlist');
      }

      setProgress(100);
      setStatus('idle');
      setCurrentTask(null);

      const action = await agentApi.getAction(response.actionId);
      addRecentAction(action);

      toast.success(
        `Playlist "${response.result?.playlistName}" created with ${response.result?.trackCount} tracks!`
      );

      return response;
    } catch (error) {
      setStatus('error');
      setCurrentTask(null);
      toast.error(
        error instanceof Error ? error.message : 'Failed to create playlist'
      );
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  const discoverNewMusic = async (data: DiscoverNewMusicRequest) => {
    setIsLoading(true);
    setStatus('processing');
    setCurrentTask('Discovering new music...');
    setProgress(0);

    try {
      setProgress(30);
      const response = await agentApi.discoverNewMusic(data);

      if (response.status === 'Failed') {
        throw new Error(response.errorMessage || 'Failed to discover music');
      }

      setProgress(100);
      setStatus('idle');
      setCurrentTask(null);

      const action = await agentApi.getAction(response.actionId);
      addRecentAction(action);

      toast.success(
        `Discovered ${response.result?.trackCount} new tracks in "${response.result?.playlistName}"!`
      );

      return response;
    } catch (error) {
      setStatus('error');
      setCurrentTask(null);
      toast.error(
        error instanceof Error ? error.message : 'Failed to discover music'
      );
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  return {
    isLoading,
    createConversation,
    createSmartPlaylist,
    discoverNewMusic,
  };
}
