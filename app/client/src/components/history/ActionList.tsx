import { ActionCard } from './ActionCard';
import type { AgentAction } from '../../types/api';

interface ActionListProps {
  actions: AgentAction[];
  loading: boolean;
}

export function ActionList({ actions, loading }: ActionListProps) {
  return (
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
            <ActionCard key={action.id} action={action} />
          ))}
        </div>
      )}
    </div>
  );
}
