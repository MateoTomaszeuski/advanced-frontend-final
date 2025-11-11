import { useContext } from 'react';
import { AgentTimerContext } from '../contexts/AgentTimerContext';

export function useAgentTimer() {
  const context = useContext(AgentTimerContext);
  if (context === undefined) {
    throw new Error('useAgentTimer must be used within an AgentTimerProvider');
  }
  return context;
}
