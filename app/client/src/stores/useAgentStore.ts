import { create } from 'zustand';
import { devtools } from 'zustand/middleware';

export type AgentStatus = 'idle' | 'processing' | 'awaiting-approval' | 'error';

export interface AgentAction {
  id: string;
  type: string;
  timestamp: Date;
  status: 'pending' | 'completed' | 'failed';
  description: string;
  result?: unknown;
  error?: string;
}

interface AgentState {
  status: AgentStatus;
  currentTask: string | null;
  progress: number;
  actions: AgentAction[];
  setStatus: (status: AgentStatus) => void;
  setCurrentTask: (task: string | null) => void;
  setProgress: (progress: number) => void;
  addAction: (action: Omit<AgentAction, 'id' | 'timestamp'>) => void;
  updateAction: (id: string, updates: Partial<AgentAction>) => void;
  clearActions: () => void;
}

export const useAgentStore = create<AgentState>()(
  devtools((set) => ({
    status: 'idle',
    currentTask: null,
    progress: 0,
    actions: [],
    setStatus: (status) => set({ status }),
    setCurrentTask: (currentTask) => set({ currentTask }),
    setProgress: (progress) => set({ progress }),
    addAction: (action) =>
      set((state) => ({
        actions: [
          ...state.actions,
          {
            ...action,
            id: crypto.randomUUID(),
            timestamp: new Date(),
          },
        ],
      })),
    updateAction: (id, updates) =>
      set((state) => ({
        actions: state.actions.map((action) =>
          action.id === id ? { ...action, ...updates } : action
        ),
      })),
    clearActions: () => set({ actions: [] }),
  }))
);
