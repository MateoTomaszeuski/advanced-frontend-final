import type { AgentAction } from '../../types/api';

interface RecentActionsListProps {
  actions: AgentAction[];
  loading: boolean;
}

export function RecentActionsList({ actions, loading }: RecentActionsListProps) {
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
        return 'text-theme-text opacity-70 bg-theme-card';
    }
  };

  return (
    <div className="bg-theme-card rounded-lg shadow">
      <div className="p-6 border-b border-theme-border">
        <h2 className="text-xl font-bold text-theme-text">Recent Actions</h2>
      </div>
      <div className="divide-y divide-theme-accent">
        {loading ? (
          <div className="p-8 text-center text-theme-text">Loading actions...</div>
        ) : actions.length === 0 ? (
          <div className="p-8 text-center text-theme-text italic">
            No recent actions to display
          </div>
        ) : (
          actions.map((action) => (
            <div key={action.id} className="p-6 hover:bg-gray-50 transition-colors">
              <div className="flex items-start justify-between mb-3">
                <div className="flex-1">
                  <div className="flex items-center gap-3 mb-2">
                    <span className="font-semibold text-theme-accent">
                      {getActionTypeLabel(action.actionType)}
                    </span>
                    <span className={`px-2 py-1 rounded text-xs font-medium ${getActionStatusColor(action.status)}`}>
                      {action.status}
                    </span>
                  </div>
                  {action.inputPrompt && (
                    <p className="text-sm text-theme-text opacity-70 mb-2">{action.inputPrompt}</p>
                  )}
                  <div className="flex items-center gap-4 text-xs text-theme-text opacity-60">
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
  );
}
