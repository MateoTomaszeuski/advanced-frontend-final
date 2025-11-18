import type { AgentAction } from '../../types/api';

interface RecentActivityProps {
  actions: AgentAction[];
  onViewAll: () => void;
}

export function RecentActivity({ actions, onViewAll }: RecentActivityProps) {
  return (
    <div className="bg-theme-card rounded-lg shadow">
      <div className="p-6 border-b border-theme-border flex items-center justify-between">
        <h2 className="text-xl font-semibold text-theme-text">Recent Activity</h2>
        <button
          onClick={onViewAll}
          className="text-theme-accent hover:text-theme-accent hover:opacity-80 text-sm font-medium transition-opacity"
        >
          View All
        </button>
      </div>
      <div className="p-6">
        {actions.length === 0 ? (
          <div className="text-center py-8">
            <svg
              className="mx-auto h-12 w-12 text-theme-text opacity-40 mb-3"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
              />
            </svg>
            <p className="text-theme-text opacity-70">No recent activity</p>
            <p className="text-sm text-theme-text opacity-60 mt-1">
              Start by creating a playlist or discovering new music
            </p>
          </div>
        ) : (
          <ul className="space-y-4">
            {actions.map((action) => (
              <li
                key={action.id}
                className="flex items-start gap-3 pb-4 border-b border-theme-border last:border-0"
              >
                <div
                  className={`w-2 h-2 rounded-full mt-2 shrink-0 ${
                    action.status === 'Completed'
                      ? 'bg-green-500'
                      : action.status === 'Failed'
                        ? 'bg-red-500'
                        : action.status === 'Processing'
                          ? 'bg-blue-500'
                          : 'bg-yellow-500'
                  }`}
                />
                <div className="flex-1 min-w-0">
                  <p className="font-medium text-theme-text truncate">
                    {action.actionType.replace(/([A-Z])/g, ' $1').trim()}
                  </p>
                  <p className="text-sm text-theme-text opacity-70 mt-0.5">
                    {new Date(action.createdAt).toLocaleString()}
                  </p>
                </div>
                <span
                  className={`text-xs px-2 py-1 rounded-full whitespace-nowrap shrink-0 ${
                    action.status === 'Completed'
                      ? 'bg-green-100 text-green-800'
                      : action.status === 'Failed'
                        ? 'bg-red-100 text-red-800'
                        : action.status === 'Processing'
                          ? 'bg-blue-100 text-blue-800'
                          : 'bg-yellow-100 text-yellow-800'
                  }`}
                >
                  {action.status}
                </span>
              </li>
            ))}
          </ul>
        )}
      </div>
    </div>
  );
}
