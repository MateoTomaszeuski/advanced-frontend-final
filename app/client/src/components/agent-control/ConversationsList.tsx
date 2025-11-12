import type { Conversation } from '../../types/api';

interface ConversationsListProps {
  conversations: Conversation[];
  currentConversationId: number | undefined;
}

export function ConversationsList({ conversations, currentConversationId }: ConversationsListProps) {
  return (
    <div className="bg-white rounded-lg shadow mt-8">
      <div className="px-6 py-4 border-b border-gray-200">
        <h2 className="text-xl font-bold text-gray-900">All Conversations</h2>
      </div>
      <div className="divide-y divide-gray-200">
        {conversations.length === 0 ? (
          <div className="p-8 text-center text-gray-500 italic">
            No conversations yet
          </div>
        ) : (
          conversations.map((conv) => (
            <div
              key={conv.id}
              className={`p-6 hover:bg-gray-50 transition-colors ${
                currentConversationId === conv.id ? 'bg-green-50' : ''
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
                {currentConversationId === conv.id && (
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
  );
}
