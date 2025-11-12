import { useEffect, useState, useCallback } from 'react';
import { useAuth } from 'react-oidc-context';
import { websocketService, type AgentStatusUpdate } from '../services/websocket';

export function useWebSocket() {
  const auth = useAuth();
  const [isConnected, setIsConnected] = useState(false);
  const [latestStatus, setLatestStatus] = useState<AgentStatusUpdate | null>(null);

  const connect = useCallback(async () => {
    if (!auth.user?.access_token || !auth.user?.profile?.email) {
      console.warn('❌ Cannot connect to WebSocket: no auth token or email');
      console.log('Auth state:', { 
        isAuthenticated: auth.isAuthenticated, 
        hasToken: !!auth.user?.access_token,
        hasEmail: !!auth.user?.profile?.email 
      });
      return;
    }

    console.log('🔌 Attempting WebSocket connection...', {
      email: auth.user.profile.email,
      hasToken: !!auth.user.access_token
    });

    try {
      await websocketService.connect(auth.user.access_token, auth.user.profile.email);
      setIsConnected(true);
      console.log('✅ WebSocket connected successfully');
    } catch (error) {
      console.error('❌ Failed to connect WebSocket:', error);
      setIsConnected(false);
    }
  }, [auth.isAuthenticated, auth.user?.access_token, auth.user?.profile?.email]);

  const disconnect = useCallback(async () => {
    await websocketService.disconnect();
    setIsConnected(false);
  }, []);

  useEffect(() => {
    console.log('🔄 useWebSocket effect triggered', {
      isAuthenticated: auth.isAuthenticated,
      hasToken: !!auth.user?.access_token
    });
    
    if (auth.isAuthenticated && auth.user?.access_token) {
      connect();
    }

    return () => {
      console.log('🔌 Cleaning up WebSocket connection');
      disconnect();
    };
  }, [auth.isAuthenticated, auth.user?.access_token, connect, disconnect]);

  useEffect(() => {
    const unsubscribe = websocketService.onStatusUpdate((update) => {
      setLatestStatus(update);
      setIsConnected(websocketService.isConnected());
    });

    return unsubscribe;
  }, []);

  return {
    isConnected,
    latestStatus,
    connect,
    disconnect,
  };
}
