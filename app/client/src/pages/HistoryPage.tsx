import { MainLayout } from '../components/layout/MainLayout';
import { useEffect, useState, useCallback } from 'react';
import { agentApi } from '../services/api';
import type { AgentAction } from '../types/api';
import { Button } from '../components/forms/Button';

export function HistoryPage() {
  const [actions, setActions] = useState<AgentAction[]>([]);
  const [loading, setLoading] = useState(true);
  const [filterType, setFilterType] = useState<string>('');
  const [filterStatus, setFilterStatus] = useState<string>('');

  const loadHistory = useCallback(async () => {
    setLoading(true);
    try {
      const history = await agentApi.getHistory({
        actionType: filterType || undefined,
        status: filterStatus || undefined,
        limit: 100,
      });
      setActions(history);
    } catch (error) {
      console.error('Failed to load history:', error);
    } finally {
      setLoading(false);
    }
  }, [filterType, filterStatus]);

  useEffect(() => {
    loadHistory();
  }, [loadHistory]);

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

  const getActionTypeColor = (type: string) => {
    switch (type) {
      case 'CreateSmartPlaylist':
        return 'bg-purple-100 text-purple-700';
      case 'DiscoverNewMusic':
        return 'bg-blue-100 text-blue-700';
      case 'ScanDuplicates':
        return 'bg-yellow-100 text-yellow-700';
      case 'RemoveDuplicates':
        return 'bg-red-100 text-red-700';
      case 'SuggestMusicByContext':
        return 'bg-green-100 text-green-700';
      default:
        return 'bg-gray-100 text-gray-700';
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Processing':
        return 'bg-blue-50 text-blue-600';
      case 'Completed':
        return 'bg-green-50 text-green-600';
      case 'Failed':
        return 'bg-red-50 text-red-600';
      case 'AwaitingApproval':
        return 'bg-yellow-50 text-yellow-600';
      default:
        return 'bg-gray-50 text-gray-600';
    }
  };

  const getTimeDuration = (start: string, end: string | null) => {
    if (!end) return null;
    const startDate = new Date(start);
    const endDate = new Date(end);
    const durationMs = endDate.getTime() - startDate.getTime();
    const seconds = Math.floor(durationMs / 1000);
    const minutes = Math.floor(seconds / 60);
    if (minutes > 0) {
      return `${minutes}m ${seconds % 60}s`;
    }
    return `${seconds}s`;
  };

  return (
    <MainLayout>
      <div className="max-w-6xl mx-auto">
        <div className="flex items-center justify-between mb-6">
          <h1 className="text-3xl font-bold text-gray-900">Activity History</h1>
          <Button onClick={loadHistory} variant="secondary">
            Refresh
          </Button>
        </div>

        <div className="bg-white rounded-lg shadow mb-6 p-6">
          <h3 className="text-sm font-medium text-gray-700 mb-4">Filters</h3>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Action Type
              </label>
              <select
                value={filterType}
                onChange={(e) => setFilterType(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-green-500 focus:border-green-500"
              >
                <option value="">All Types</option>
                <option value="CreateSmartPlaylist">Smart Playlist</option>
                <option value="DiscoverNewMusic">Music Discovery</option>
                <option value="ScanDuplicates">Scan Duplicates</option>
                <option value="RemoveDuplicates">Remove Duplicates</option>
                <option value="SuggestMusicByContext">Music Suggestions</option>
              </select>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Status
              </label>
              <select
                value={filterStatus}
                onChange={(e) => setFilterStatus(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-green-500 focus:border-green-500"
              >
                <option value="">All Statuses</option>
                <option value="Processing">Processing</option>
                <option value="Completed">Completed</option>
                <option value="Failed">Failed</option>
                <option value="AwaitingApproval">Awaiting Approval</option>
              </select>
            </div>
          </div>

          {(filterType || filterStatus) && (
            <div className="mt-4">
              <Button
                onClick={() => {
                  setFilterType('');
                  setFilterStatus('');
                }}
                variant="ghost"
              >
                Clear Filters
              </Button>
            </div>
          )}
        </div>

        <div className="bg-white rounded-lg shadow">
          <div className="px-6 py-4 border-b border-gray-200">
            <div className="flex items-center justify-between">
              <h2 className="text-xl font-bold text-gray-900">All Actions</h2>
              <span className="text-sm text-gray-500">
                {actions.length} {actions.length === 1 ? 'action' : 'actions'}
              </span>
            </div>
          </div>

          {loading ? (
            <div className="p-12 text-center">
              <div className="inline-block animate-spin h-8 w-8 border-4 border-green-600 border-t-transparent rounded-full" />
              <p className="mt-4 text-gray-600">Loading history...</p>
            </div>
          ) : actions.length === 0 ? (
            <div className="p-12 text-center text-gray-500 italic">
              No actions found matching your filters
            </div>
          ) : (
            <div className="divide-y divide-gray-200">
              {actions.map((action) => (
                <div key={action.id} className="p-6 hover:bg-gray-50 transition-colors">
                  <div className="flex items-start justify-between mb-3">
                    <div className="flex-1">
                      <div className="flex items-center gap-3 mb-2">
                        <span className={`px-3 py-1 rounded-full text-xs font-medium ${getActionTypeColor(action.actionType)}`}>
                          {getActionTypeLabel(action.actionType)}
                        </span>
                        <span className={`px-3 py-1 rounded text-xs font-medium ${getStatusColor(action.status)}`}>
                          {action.status}
                        </span>
                        <span className="text-xs text-gray-500">
                          Conversation #{action.conversationId}
                        </span>
                      </div>

                      {action.inputPrompt && (
                        <p className="text-sm text-gray-700 mb-2 font-medium">
                          {action.inputPrompt}
                        </p>
                      )}

                      <div className="flex items-center gap-6 text-xs text-gray-500">
                        <span>
                          Started: {new Date(action.createdAt).toLocaleString()}
                        </span>
                        {action.completedAt && (
                          <>
                            <span>
                              Completed: {new Date(action.completedAt).toLocaleString()}
                            </span>
                            <span className="font-medium text-green-600">
                              Duration: {getTimeDuration(action.createdAt, action.completedAt)}
                            </span>
                          </>
                        )}
                      </div>
                    </div>
                  </div>

                  {action.errorMessage && (
                    <div className="mt-3 p-3 bg-red-50 border border-red-200 rounded-lg">
                      <p className="text-sm text-red-700">
                        <span className="font-semibold">Error:</span> {action.errorMessage}
                      </p>
                    </div>
                  )}

                  {action.parameters && typeof action.parameters === 'object' ? (
                    <details className="mt-3">
                      <summary className="cursor-pointer text-sm text-gray-600 hover:text-gray-800 font-medium">
                        View Parameters
                      </summary>
                      <pre className="mt-2 p-3 bg-gray-50 border border-gray-200 rounded text-xs overflow-x-auto">
                        {JSON.stringify(action.parameters, null, 2)}
                      </pre>
                    </details>
                  ) : null}

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
              ))}
            </div>
          )}
        </div>
      </div>
    </MainLayout>
  );
}
