import { useState, useEffect, useCallback } from 'react';
import type { ReactNode } from 'react';
import { useAgentStore } from '../stores/useAgentStore';
import { AgentTimerContext } from '../contexts/AgentTimerContext';

export function AgentTimerProvider({ children }: { children: ReactNode }) {
  const { status } = useAgentStore();
  const [elapsedTime, setElapsedTime] = useState(0);
  const [startTime, setStartTime] = useState<number | null>(null);

  const resetTimer = useCallback(() => {
    setStartTime(null);
    setElapsedTime(0);
  }, []);

  useEffect(() => {
    let intervalId: ReturnType<typeof setInterval> | null = null;

    if (status === 'processing') {
      if (!startTime) {
        setStartTime(Date.now());
      }

      intervalId = setInterval(() => {
        if (startTime) {
          const elapsed = Math.floor((Date.now() - startTime) / 1000);
          setElapsedTime(elapsed);
        }
      }, 100);
    } else if (status === 'idle' || status === 'error') {
      if (intervalId) {
        clearInterval(intervalId);
      }
      resetTimer();
    } else if (status === 'awaiting-approval') {
      if (intervalId) {
        clearInterval(intervalId);
      }
    }

    return () => {
      if (intervalId) {
        clearInterval(intervalId);
      }
    };
  }, [status, startTime, resetTimer]);

  return (
    <AgentTimerContext.Provider value={{ elapsedTime, startTime, resetTimer }}>
      {children}
    </AgentTimerContext.Provider>
  );
}
