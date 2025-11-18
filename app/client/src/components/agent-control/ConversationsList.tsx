import type { Conversation } from '../../types/api';

interface ConversationsListProps {
  conversations: Conversation[];
  currentConversationId: number | undefined;
}

export function ConversationsList({ conversations, currentConversationId }: ConversationsListProps) {
  return (
    <div className="bg-theme-card rounded-lg shadow mt-8">
      <div className="p-6 border-b border-theme-border">
        <h2 className="text-xl font-bold text-theme-text">All Conversations</h2>
      </div>
      <div className="divide-y divide-gray-200">
      {conversations.length === 0 ? (
        <div className="p-8 text-center text-theme-text opacity-70 italic">
            No conversations yet
          </div>
        ) : (
          conversations.map((conv) => (
            <div
              key={conv.id}
              className={`p-6 hover:bg-theme-accent transition-colors ${
                currentConversationId === conv.id ? 'bg-theme-card' : ''
              }`}
            >
                <div className="flex items-start justify-between group-hover:text-theme-primary transition-colors">
                <div className="flex-1">
                  <h3 className="font-semibold text-theme-text group-hover:text-theme-primary mb-1 transition-colors">{conv.title}</h3>
                  <div className="flex items-center gap-4 text-sm text-theme-text opacity-70 group-hover:text-theme-primary group-hover:opacity-100 transition-colors">
                  <span>ID: {conv.id}</span>
                  <span>Created: {new Date(conv.createdAt).toLocaleDateString()}</span>
                  {conv.actionCount !== undefined && (
                    <span className="font-medium text-theme-accent group-hover:text-theme-primary transition-colors">
                    {conv.actionCount} {conv.actionCount === 1 ? 'action' : 'actions'}
                    </span>
                  )}
                  </div>
                </div>
                {currentConversationId === conv.id && (
                  <span className="px-3 py-1 bg-theme-card text-theme-accent text-xs font-medium rounded-full">
                    Active
                  </span>
                )}
              </div>
            </div>
          ))
        )}
      </div>
    </div>
  );
}
