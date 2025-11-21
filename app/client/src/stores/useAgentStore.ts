import { create } from 'zustand';
import { devtools, persist } from 'zustand/middleware';
import type { Conversation, AgentAction as ApiAgentAction } from '../types/api';

export type AgentStatus = 'idle' | 'processing' | 'awaiting-approval' | 'error';

interface AgentState {
  status: AgentStatus;
  currentTask: string | null;
  progress: number;
  currentConversation: Conversation | null;
  conversations: Conversation[];
  recentActions: ApiAgentAction[];
  setStatus: (status: AgentStatus) => void;
  setCurrentTask: (task: string | null) => void;
  setProgress: (progress: number) => void;
  setCurrentConversation: (conversation: Conversation | null) => void;
  setConversations: (conversations: Conversation[]) => void;
  addConversation: (conversation: Conversation) => void;
  addRecentAction: (action: ApiAgentAction) => void;
  clearRecentActions: () => void;
  reset: () => void;
}

export const useAgentStore = create<AgentState>()(
  devtools(
    persist(
      (set, _get, api) => ({
        status: 'idle',
        currentTask: null,
        progress: 0,
        currentConversation: null,
        conversations: [],
        recentActions: [],
        setStatus: (status) => set({ status }),
        setCurrentTask: (currentTask) => set({ currentTask }),
        setProgress: (progress) => set({ progress }),
        setCurrentConversation: (currentConversation) => set({ currentConversation }),
        setConversations: (conversations) => set({ conversations }),
        addConversation: (conversation) =>
          set((state) => ({
            conversations: [conversation, ...state.conversations],
          })),
        addRecentAction: (action) =>
          set((state) => ({
            recentActions: [action, ...state.recentActions].slice(0, 10),
          })),
        clearRecentActions: () => set({ recentActions: [] }),
        reset: () => {
          set({
            status: 'idle',
            currentTask: null,
            progress: 0,
            currentConversation: null,
            conversations: [],
            recentActions: [],
          });
          // Clear persisted localStorage data using persist API
          api.persist.clearStorage();
        },
      }),
      {
        name: 'agent-storage',
        partialize: (state) => ({
          conversations: state.conversations,
          recentActions: state.recentActions,
        }),
      }
    )
  )
);
