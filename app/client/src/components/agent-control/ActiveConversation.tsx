import type { Conversation } from '../../types/api';

interface ActiveConversationProps {
  conversation: Conversation | null;
}

export function ActiveConversation({ conversation }: ActiveConversationProps) {
  return (
    <div className="bg-theme-card rounded-lg shadow mb-8">
      <div className="px-6 py-4 border-b border-theme-border">
        <h2 className="text-xl font-bold text-gray-900">Active Conversation</h2>
      </div>
      <div className="p-6">
        {conversation ? (
          <div className="space-y-3">
            <div>
              <span className="text-sm text-gray-500">Conversation ID:</span>
              <span className="ml-2 text-sm font-medium text-gray-900">
                {conversation.id}
              </span>
            </div>
            <div>
              <span className="text-sm text-gray-500">Title:</span>
              <span className="ml-2 text-sm font-medium text-gray-900">
                {conversation.title}
              </span>
            </div>
            <div>
              <span className="text-sm text-gray-500">Created:</span>
              <span className="ml-2 text-sm text-gray-700">
                {new Date(conversation.createdAt).toLocaleString()}
              </span>
            </div>
            {conversation.actionCount !== undefined && (
              <div>
                <span className="text-sm text-gray-500">Total Actions:</span>
                <span className="ml-2 text-sm font-medium text-gray-900">
                  {conversation.actionCount}
                </span>
              </div>
            )}
          </div>
        ) : (
          <p className="text-gray-500 italic">No active conversation</p>
        )}
      </div>
    </div>
  );
}
