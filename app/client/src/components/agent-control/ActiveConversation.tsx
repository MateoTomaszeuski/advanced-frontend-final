import type { Conversation } from '../../types/api';

interface ActiveConversationProps {
  conversation: Conversation | null;
}

export function ActiveConversation({ conversation }: ActiveConversationProps) {
  return (
    <div className="bg-theme-card rounded-lg shadow mb-8">
      <div className="px-6 py-4 border-b border-theme-border">
        <h2 className="text-xl font-bold text-theme-text">Active Conversation</h2>
      </div>
      <div className="p-6">
        {conversation ? (
          <div className="space-y-3">
            <div>
              <span className="text-sm text-theme-text opacity-70">Conversation ID:</span>
              <span className="ml-2 text-sm font-medium text-theme-text">
                {conversation.id}
              </span>
            </div>
            <div>
              <span className="text-sm text-theme-text opacity-70">Title:</span>
              <span className="ml-2 text-sm font-medium text-theme-text">
                {conversation.title}
              </span>
            </div>
            <div>
              <span className="text-sm text-theme-text opacity-70">Created:</span>
              <span className="ml-2 text-sm text-theme-text opacity-80">
                {new Date(conversation.createdAt).toLocaleString()}
              </span>
            </div>
            {conversation.actionCount !== undefined && (
              <div>
                <span className="text-sm text-theme-text opacity-70">Total Actions:</span>
                <span className="ml-2 text-sm font-medium text-theme-text">
                  {conversation.actionCount}
                </span>
              </div>
            )}
          </div>
        ) : (
          <p className="text-theme-text opacity-70 italic">No active conversation</p>
        )}
      </div>
    </div>
  );
}
