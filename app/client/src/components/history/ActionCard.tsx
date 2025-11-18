import type { AgentAction } from '../../types/api';

interface ActionCardProps {
  action: AgentAction;
}

export function ActionCard({ action }: ActionCardProps) {
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
        return 'bg-theme-card text-theme-text opacity-80';
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
        return 'bg-yellow-100 text-yellow-800';
      default:
        return 'bg-theme-card text-theme-text opacity-80';
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
    <div className="p-6 hover:bg-gray-50 transition-colors">
      <div className="flex items-start justify-between mb-3">
        <div className="flex-1">
          <div className="flex items-center gap-3 mb-2">
            <span className={`px-3 py-1 rounded-full text-xs font-medium ${getActionTypeColor(action.actionType)}`}>
              {getActionTypeLabel(action.actionType)}
            </span>
            <span className={`px-3 py-1 rounded text-xs font-medium ${getStatusColor(action.status)}`}>
              {action.status}
            </span>
            <span className="text-xs text-theme-text opacity-60">
              Conversation #{action.conversationId}
            </span>
          </div>

          {action.inputPrompt && (
            <p className="text-sm text-theme-text opacity-80 mb-2 font-medium">
              {action.inputPrompt}
            </p>
          )}

          <div className="flex items-center gap-6 text-xs text-theme-text opacity-60">
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
          <summary className="cursor-pointer text-sm text-theme-text opacity-70 hover:opacity-100 font-medium">
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
  );
}
