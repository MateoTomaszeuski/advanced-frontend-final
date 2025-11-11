import { MainLayout } from '../components/layout/MainLayout';
import { useAgentStore } from '../stores/useAgentStore';
import { conversationApi } from '../services/api';
import { useState, useEffect, useCallback } from 'react';
import type { Conversation } from '../types/api';
import { useAgentTimer } from '../hooks/useAgentTimer';
import { showToast } from '../utils/toast';

export function AgentControlPage() {
  const {
    status,
    currentTask,
    recentActions,
    currentConversation,
    setConversations,
  } = useAgentStore();

  const { elapsedTime } = useAgentTimer();
  const [loading, setLoading] = useState(true);
  const [allConversations, setAllConversations] = useState<Conversation[]>([]);

  const loadConversations = useCallback(async () => {
    try {
      const convs = await conversationApi.getAll();
      setAllConversations(convs);
      setConversations(convs);
    } catch (error) {
      console.error('Failed to load conversations:', error);
      showToast.error(
        error instanceof Error ? error.message : 'Failed to load conversations'
      );
    } finally {
      setLoading(false);
    }
  }, [setConversations]);

  useEffect(() => {
    loadConversations();
  }, [loadConversations]);

  const formatElapsedTime = (seconds: number): string => {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'idle':
        return 'text-gray-500 bg-gray-100';
      case 'processing':
        return 'text-blue-600 bg-blue-100';
      case 'awaiting-approval':
        return 'text-yellow-600 bg-yellow-100';
      case 'error':
        return 'text-red-600 bg-red-100';
      default:
        return 'text-gray-500 bg-gray-100';
    }
  };

  const getActionTypeLabel = (type: string) => {
    switch (type) {
      case 'CreateSmartPlaylist':
        return 'Smart Playlist';
      case 'DiscoverNewMusic':
        return 'Music Discovery';
      case 'ScanDuplicates':
        return 'Scan Duplicates';
      case 'RemoveDuplicates':
        return 'Remove Duplicates';
      case 'SuggestMusicByContext':
        return 'Music Suggestions';
      default:
        return type;
    }
  };

  const getActionStatusColor = (status: string) => {
    switch (status) {
      case 'Processing':
        return 'text-blue-600 bg-blue-50';
      case 'Completed':
        return 'text-green-600 bg-green-50';
      case 'Failed':
        return 'text-red-600 bg-red-50';
      case 'AwaitingApproval':
        return 'text-yellow-600 bg-yellow-50';
      default:
        return 'text-gray-600 bg-gray-50';
    }
  };

  return (
    <MainLayout>
      <div className="max-w-6xl mx-auto">
        <h1 className="text-3xl font-bold text-gray-900 mb-6">Agent Control Center</h1>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6 mb-8">
          <div className="bg-white rounded-lg shadow p-6">
            <h3 className="text-sm font-medium text-gray-500 mb-2">Agent Status</h3>
            <div className="flex items-center gap-3">
              <span className={`px-3 py-1 rounded-full text-sm font-medium ${getStatusColor(status)}`}>
                {status.charAt(0).toUpperCase() + status.slice(1)}
              </span>
              {status === 'processing' && (
                <div className="animate-spin h-5 w-5 border-2 border-green-600 border-t-transparent rounded-full" />
              )}
            </div>
          </div>

          <div className="bg-white rounded-lg shadow p-6">
            <h3 className="text-sm font-medium text-gray-500 mb-2">Current Task</h3>
            <p className="text-lg font-semibold text-gray-900">
              {currentTask || 'No active task'}
            </p>
          </div>

          <div className="bg-white rounded-lg shadow p-6">
            <h3 className="text-sm font-medium text-gray-500 mb-2">Time Elapsed</h3>
            <div className="flex items-center gap-3">
              <p className="text-2xl font-bold text-gray-900 font-mono">
                {formatElapsedTime(elapsedTime)}
              </p>
              {status === 'processing' && (
                <svg className="w-5 h-5 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              )}
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow mb-8">
          <div className="px-6 py-4 border-b border-gray-200">
            <h2 className="text-xl font-bold text-gray-900">Active Conversation</h2>
          </div>
          <div className="p-6">
            {currentConversation ? (
              <div className="space-y-3">
                <div>
                  <span className="text-sm text-gray-500">Conversation ID:</span>
                  <span className="ml-2 text-sm font-medium text-gray-900">
                    {currentConversation.id}
                  </span>
                </div>
                <div>
                  <span className="text-sm text-gray-500">Title:</span>
                  <span className="ml-2 text-sm font-medium text-gray-900">
                    {currentConversation.title}
                  </span>
                </div>
                <div>
                  <span className="text-sm text-gray-500">Created:</span>
                  <span className="ml-2 text-sm text-gray-700">
                    {new Date(currentConversation.createdAt).toLocaleString()}
                  </span>
                </div>
                {currentConversation.actionCount !== undefined && (
                  <div>
                    <span className="text-sm text-gray-500">Total Actions:</span>
                    <span className="ml-2 text-sm font-medium text-gray-900">
                      {currentConversation.actionCount}
                    </span>
                  </div>
                )}
              </div>
            ) : (
              <p className="text-gray-500 italic">No active conversation</p>
            )}
          </div>
        </div>

        <div className="bg-white rounded-lg shadow">
          <div className="px-6 py-4 border-b border-gray-200">
            <h2 className="text-xl font-bold text-gray-900">Recent Actions</h2>
          </div>
          <div className="divide-y divide-gray-200">
            {loading ? (
              <div className="p-8 text-center text-gray-500">Loading actions...</div>
            ) : recentActions.length === 0 ? (
              <div className="p-8 text-center text-gray-500 italic">
                No recent actions to display
              </div>
            ) : (
              recentActions.map((action) => (
                <div key={action.id} className="p-6 hover:bg-gray-50 transition-colors">
                  <div className="flex items-start justify-between mb-3">
                    <div className="flex-1">
                      <div className="flex items-center gap-3 mb-2">
                        <span className="font-semibold text-gray-900">
                          {getActionTypeLabel(action.actionType)}
                        </span>
                        <span className={`px-2 py-1 rounded text-xs font-medium ${getActionStatusColor(action.status)}`}>
                          {action.status}
                        </span>
                      </div>
                      {action.inputPrompt && (
                        <p className="text-sm text-gray-600 mb-2">{action.inputPrompt}</p>
                      )}
                      <div className="flex items-center gap-4 text-xs text-gray-500">
                        <span>
                          Created: {new Date(action.createdAt).toLocaleString()}
                        </span>
                        {action.completedAt && (
                          <span>
                            Completed: {new Date(action.completedAt).toLocaleString()}
                          </span>
                        )}
                      </div>
                    </div>
                  </div>
                  {action.errorMessage && (
                    <div className="mt-3 p-3 bg-red-50 border border-red-200 rounded-lg">
                      <p className="text-sm text-red-700">
                        <span className="font-medium">Error:</span> {action.errorMessage}
                      </p>
                    </div>
                  )}
                  {action.result && typeof action.result === 'object' ? (
                    <details className="mt-3">
                      <summary className="cursor-pointer text-sm text-green-700 hover:text-green-800 font-medium">
                        View Result
                      </summary>
                      <pre className="mt-2 p-3 bg-gray-50 border border-gray-200 rounded text-xs overflow-x-auto">
                        {JSON.stringify(action.result, null, 2)}
                      </pre>
                    </details>
                  ) : null}
                </div>
              ))
            )}
          </div>
        </div>

        <div className="bg-white rounded-lg shadow mt-8">
          <div className="px-6 py-4 border-b border-gray-200">
            <h2 className="text-xl font-bold text-gray-900">All Conversations</h2>
          </div>
          <div className="divide-y divide-gray-200">
            {allConversations.length === 0 ? (
              <div className="p-8 text-center text-gray-500 italic">
                No conversations yet
              </div>
            ) : (
              allConversations.map((conv) => (
                <div
                  key={conv.id}
                  className={`p-6 hover:bg-gray-50 transition-colors ${
                    currentConversation?.id === conv.id ? 'bg-green-50' : ''
                  }`}
                >
                  <div className="flex items-start justify-between">
                    <div className="flex-1">
                      <h3 className="font-semibold text-gray-900 mb-1">{conv.title}</h3>
                      <div className="flex items-center gap-4 text-sm text-gray-500">
                        <span>ID: {conv.id}</span>
                        <span>Created: {new Date(conv.createdAt).toLocaleDateString()}</span>
                        {conv.actionCount !== undefined && (
                          <span className="font-medium text-green-700">
                            {conv.actionCount} {conv.actionCount === 1 ? 'action' : 'actions'}
                          </span>
                        )}
                      </div>
                    </div>
                    {currentConversation?.id === conv.id && (
                      <span className="px-3 py-1 bg-green-100 text-green-700 text-xs font-medium rounded-full">
                        Active
                      </span>
                    )}
                  </div>
                </div>
              ))
            )}
          </div>
        </div>
      </div>
    </MainLayout>
  );
}
