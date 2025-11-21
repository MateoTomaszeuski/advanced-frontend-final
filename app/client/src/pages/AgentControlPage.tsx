import { MainLayout } from '../components/layout/MainLayout';
import { StatusCards } from '../components/agent-control/StatusCards';
import { ActiveConversation } from '../components/agent-control/ActiveConversation';
import { RecentActionsList } from '../components/agent-control/RecentActionsList';
import { ConversationsList } from '../components/agent-control/ConversationsList';
import { useAgentStore } from '../stores/useAgentStore';
import { conversationApi, agentApi } from '../services/api';
import { useState, useEffect, useCallback } from 'react';
import type { Conversation, AgentAction } from '../types/api';
import { useAgentTimer } from '../hooks/useAgentTimer';
import { showToast } from '../utils/toast';
import { useWebSocket } from '../hooks/useWebSocket';

export function AgentControlPage() {
  const {
    status,
    currentTask,
    currentConversation,
    conversations,
    setConversations,
    setStatus,
    setCurrentTask,
  } = useAgentStore();

  const { elapsedTime } = useAgentTimer();
  const { isConnected, latestStatus } = useWebSocket();
  const [loading, setLoading] = useState(true);
  const [initialLoadDone, setInitialLoadDone] = useState(false);
  const [recentActions, setRecentActions] = useState<AgentAction[]>([]);
  const [realtimeMessage, setRealtimeMessage] = useState<string | null>(null);

  const loadConversations = useCallback(async () => {
    try {
      const convs = await conversationApi.getAll();
      setConversations(convs);
      // If no conversations, also clear recent actions
      if (convs.length === 0) {
        setRecentActions([]);
      }
    } catch (error) {
      console.error('Failed to load conversations:', error);
      showToast.error(
        error instanceof Error ? error.message : 'Failed to load conversations'
      );
    } finally {
      setLoading(false);
      setInitialLoadDone(true);
    }
  }, [setConversations]);

  const loadRecentActions = useCallback(async () => {
    try {
      const actions = await agentApi.getHistory({ limit: 10 });
      setRecentActions(actions);
    } catch (error) {
      console.error('Failed to load recent actions:', error);
    }
  }, []);

  useEffect(() => {
    if (latestStatus) {
      console.log('Latest WebSocket status:', latestStatus);
      
      if (latestStatus.status === 'processing') {
        setStatus('processing');
      } else if (latestStatus.status === 'error') {
        setStatus('error');
      } else if (latestStatus.status === 'completed') {
        setStatus('idle');
      }
      
      if (latestStatus.message) {
        setCurrentTask(latestStatus.message);
        setRealtimeMessage(latestStatus.message);
        
        setTimeout(() => setRealtimeMessage(null), 5000);
      }

      if (latestStatus.status === 'completed' && latestStatus.message) {
        showToast.success(latestStatus.message);
        loadConversations();
        loadRecentActions();
      } else if (latestStatus.status === 'error' && latestStatus.message) {
        showToast.error(latestStatus.message);
      }
    }
  }, [latestStatus, setStatus, setCurrentTask, loadConversations, loadRecentActions]);

  useEffect(() => {
    loadConversations();
    loadRecentActions();
  }, [loadConversations, loadRecentActions]);

  return (
    <MainLayout>
      <div className="max-w-6xl mx-auto">
        <div className="flex items-center justify-between mb-6">
          <h1 className="text-3xl font-bold text-theme-text">Agent Control Center</h1>
          <div className="flex items-center gap-2">
            <div className={`w-3 h-3 rounded-full ${isConnected ? 'bg-green-500' : 'bg-gray-400'}`} />
            <span className="text-sm text-theme-text opacity-70">
              {isConnected ? 'WebSocket Connected' : 'WebSocket Disconnected'}
            </span>
          </div>
        </div>

        {realtimeMessage && (
          <div className="mb-6 p-4 bg-blue-50 border border-blue-200 rounded-lg flex items-center gap-3">
            <div className="animate-pulse w-2 h-2 bg-blue-600 rounded-full" />
            <p className="text-sm text-blue-800 font-medium">{realtimeMessage}</p>
          </div>
        )}

        <StatusCards 
          status={status} 
          currentTask={currentTask} 
          elapsedTime={elapsedTime} 
        />

        <ActiveConversation conversation={currentConversation} />

        <RecentActionsList actions={recentActions} loading={loading} />

        <ConversationsList 
          conversations={conversations} 
          currentConversationId={currentConversation?.id} 
        />
      </div>
    </MainLayout>
  );
}
