import { useWebSocket } from '../hooks/useWebSocket';

export function WebSocketProvider({ children }: { children: React.ReactNode }) {
  // This hook establishes the WebSocket connection globally
  useWebSocket();

  return <>{children}</>;
}
