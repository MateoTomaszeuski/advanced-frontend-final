import { createContext } from 'react';

export interface AgentTimerContextType {
  elapsedTime: number;
  startTime: number | null;
  resetTimer: () => void;
}

export const AgentTimerContext = createContext<AgentTimerContextType | undefined>(undefined);
