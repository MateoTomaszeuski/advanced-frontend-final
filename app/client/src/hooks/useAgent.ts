import { useState, useEffect } from 'react';
import { useAgentStore } from '../stores/useAgentStore';
import { conversationApi, agentApi } from '../services/api';
import { websocketService, type AgentStatusUpdate } from '../services/websocket';
import toast from 'react-hot-toast';
import type {
  CreateSmartPlaylistRequest,
  DiscoverNewMusicRequest,
  RemoveDuplicatesResponse,
  SuggestMusicResponse,
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

  useEffect(() => {
    const unsubscribe = websocketService.onStatusUpdate((update: AgentStatusUpdate) => {
      console.log('📨 Agent update received in useAgent:', update);
      
      if (update.status === 'processing' && update.message) {
        setStatus('processing');
        setCurrentTask(update.message);
      } else if (update.status === 'completed') {
        setStatus('idle');
        setCurrentTask(null);
        setProgress(100);
      } else if (update.status === 'error') {
        setStatus('error');
        setCurrentTask(update.message || 'An error occurred');
      }
    });

    return unsubscribe;
  }, [setStatus, setCurrentTask, setProgress]);

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
    setCurrentTask('Initializing playlist creation...');
    setProgress(0);

    try {
      // WebSocket will handle real-time status updates
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
        `Playlist "${response.result?.playlistName}" created with ${response.result?.trackCount} tracks!`,
        { duration: 5000 }
      );

      return response;
    } catch (error) {
      setStatus('error');
      setCurrentTask(null);
      toast.error(
        error instanceof Error ? error.message : 'Failed to create playlist',
        { duration: Infinity }
      );
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  const discoverNewMusic = async (data: DiscoverNewMusicRequest) => {
    setIsLoading(true);
    setStatus('processing');
    setCurrentTask('Initializing music discovery...');
    setProgress(0);

    try {
      // WebSocket will handle real-time status updates
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
        `Discovered ${response.result?.trackCount} new tracks in "${response.result?.playlistName}"!`,
        { duration: 5000 }
      );

      return response;
    } catch (error) {
      setStatus('error');
      setCurrentTask(null);
      toast.error(
        error instanceof Error ? error.message : 'Failed to discover music',
        { duration: Infinity }
      );
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  const scanDuplicates = async (conversationId: number, playlistId: string) => {
    setIsLoading(true);
    setStatus('processing');
    setCurrentTask('Scanning for duplicates...');
    setProgress(0);

    try {
      setProgress(30);
      const response = await agentApi.scanDuplicates({ conversationId, playlistId }) as RemoveDuplicatesResponse;

      setProgress(100);
      setStatus('idle');
      setCurrentTask(null);

      if (response.totalDuplicateGroups > 0) {
        toast.success(
          `Found ${response.totalDuplicateTracks} duplicates in ${response.totalDuplicateGroups} groups`
        );
      } else {
        toast.success('No duplicates found in this playlist');
      }

      return response;
    } catch (error) {
      setStatus('error');
      setCurrentTask(null);
      toast.error(
        error instanceof Error ? error.message : 'Failed to scan for duplicates'
      );
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  const confirmRemoveDuplicates = async (
    conversationId: number,
    playlistId: string,
    trackUrisToRemove: string[]
  ) => {
    setIsLoading(true);
    setStatus('processing');
    setCurrentTask('Removing duplicates...');
    setProgress(0);

    try {
      setProgress(30);
      const response = await agentApi.confirmRemoveDuplicates({
        conversationId,
        playlistId,
        trackUrisToRemove,
      });

      if (response.status === 'Failed') {
        throw new Error(response.errorMessage || 'Failed to remove duplicates');
      }

      setProgress(100);
      setStatus('idle');
      setCurrentTask(null);

      const action = await agentApi.getAction(response.actionId);
      addRecentAction(action);

      toast.success(`Removed ${trackUrisToRemove.length} duplicate tracks!`);

      return response;
    } catch (error) {
      setStatus('error');
      setCurrentTask(null);
      toast.error(
        error instanceof Error ? error.message : 'Failed to remove duplicates'
      );
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  const suggestMusic = async (
    conversationId: number,
    playlistId: string,
    context: string,
    limit: number = 10
  ) => {
    setIsLoading(true);
    setStatus('processing');
    setCurrentTask('Generating music suggestions...');
    setProgress(0);

    try {
      setProgress(30);
      const response = await agentApi.suggestMusic({
        conversationId,
        playlistId,
        context,
        limit,
      }) as SuggestMusicResponse;

      setProgress(100);
      setStatus('idle');
      setCurrentTask(null);

      toast.success(`Generated ${response.suggestionCount} suggestions for "${response.playlistName}"`);

      return response;
    } catch (error) {
      setStatus('error');
      setCurrentTask(null);
      toast.error(
        error instanceof Error ? error.message : 'Failed to generate suggestions'
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
    scanDuplicates,
    confirmRemoveDuplicates,
    suggestMusic,
  };
}
