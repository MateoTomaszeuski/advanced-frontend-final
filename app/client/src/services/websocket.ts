import * as signalR from '@microsoft/signalr';
import { config } from '../config';

export interface AgentStatusUpdate {
  status: 'processing' | 'completed' | 'error' | 'idle';
  message?: string;
  data?: unknown;
  timestamp: string;
}

export type AgentStatusCallback = (update: AgentStatusUpdate) => void;

class WebSocketService {
  private connection: signalR.HubConnection | null = null;
  private reconnectAttempts = 0;
  private maxReconnectAttempts = 5;
  private reconnectDelay = 2000;
  private statusCallbacks: AgentStatusCallback[] = [];
  private userEmail: string | null = null;

  async connect(accessToken: string, userEmail: string): Promise<void> {
    console.log('🔌 WebSocketService.connect() called', { userEmail, hasToken: !!accessToken });
    
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      console.log('⚠️ WebSocket already connected');
      return;
    }

    this.userEmail = userEmail;

    const hubUrl = `${config.api.baseUrl}/hubs/agent`;
    console.log('🔗 Connecting to SignalR hub:', hubUrl);

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => {
          console.log('🔑 Providing access token for WebSocket');
          return accessToken;
        },
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.ServerSentEvents | signalR.HttpTransportType.LongPolling,
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: () => {
          this.reconnectAttempts++;
          if (this.reconnectAttempts > this.maxReconnectAttempts) {
            console.log('❌ Max reconnection attempts reached');
            return null;
          }
          const delay = this.reconnectDelay * this.reconnectAttempts;
          console.log(`🔄 Reconnection attempt ${this.reconnectAttempts}/${this.maxReconnectAttempts}, delay: ${delay}ms`);
          return delay;
        }
      })
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.connection.on('AgentStatusUpdate', (update: AgentStatusUpdate) => {
      console.log('📨 Received agent status update:', update);
      this.statusCallbacks.forEach(callback => callback(update));
    });

    this.connection.onreconnecting((error) => {
      console.warn('⚠️ WebSocket reconnecting...', error);
    });

    this.connection.onreconnected(() => {
      console.log('✅ WebSocket reconnected');
      this.reconnectAttempts = 0;
      if (this.userEmail) {
        this.joinUserGroup(this.userEmail);
      }
    });

    this.connection.onclose((error) => {
      console.error('❌ WebSocket connection closed', error);
    });

    try {
      console.log('🚀 Starting SignalR connection...');
      await this.connection.start();
      console.log('✅ WebSocket connected successfully, state:', this.connection.state);
      this.reconnectAttempts = 0;
      await this.joinUserGroup(userEmail);
    } catch (error) {
      console.error('❌ Failed to connect to WebSocket:', error);
      throw error;
    }
  }

  private async joinUserGroup(userEmail: string): Promise<void> {
    if (!this.connection || this.connection.state !== signalR.HubConnectionState.Connected) {
      console.warn('Cannot join user group: connection not established');
      return;
    }

    try {
      await this.connection.invoke('JoinUserGroup', userEmail);
      console.log(`Joined user group: user-${userEmail}`);
    } catch (error) {
      console.error('Failed to join user group:', error);
    }
  }

  async disconnect(): Promise<void> {
    if (this.connection) {
      try {
        if (this.userEmail) {
          await this.connection.invoke('LeaveUserGroup', this.userEmail);
        }
        await this.connection.stop();
        console.log('WebSocket disconnected');
      } catch (error) {
        console.error('Error disconnecting WebSocket:', error);
      }
      this.connection = null;
      this.userEmail = null;
    }
  }

  onStatusUpdate(callback: AgentStatusCallback): () => void {
    this.statusCallbacks.push(callback);
    return () => {
      this.statusCallbacks = this.statusCallbacks.filter(cb => cb !== callback);
    };
  }

  isConnected(): boolean {
    return this.connection?.state === signalR.HubConnectionState.Connected;
  }

  getConnectionState(): string {
    return this.connection?.state ?? 'Disconnected';
  }
}

export const websocketService = new WebSocketService();
