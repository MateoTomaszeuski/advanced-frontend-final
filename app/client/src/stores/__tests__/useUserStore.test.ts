import { describe, it, expect, beforeEach } from 'vitest';
import { useUserStore } from '../../stores/useUserStore';

describe('useUserStore', () => {
  beforeEach(() => {
    useUserStore.setState({
      user: null,
    });
  });

  it('should initialize with no user', () => {
    const state = useUserStore.getState();
    
    expect(state.user).toBeNull();
  });

  it('should set user', () => {
    const { setUser } = useUserStore.getState();
    const user = {
      id: '1',
      email: 'test@example.com',
      name: 'Test User',
      spotifyConnected: false,
    };
    
    setUser(user);
    
    const state = useUserStore.getState();
    expect(state.user).toEqual(user);
  });

  it('should clear user', () => {
    const { setUser, clearUser } = useUserStore.getState();
    
    setUser({
      id: '1',
      email: 'test@example.com',
      name: 'Test User',
      spotifyConnected: false,
    });
    
    clearUser();
    
    const state = useUserStore.getState();
    expect(state.user).toBeNull();
  });

  it('should update Spotify connection status', () => {
    const { setUser, updateSpotifyConnection } = useUserStore.getState();
    
    setUser({
      id: '1',
      email: 'test@example.com',
      name: 'Test User',
      spotifyConnected: false,
    });
    
    updateSpotifyConnection(true);
    
    const state = useUserStore.getState();
    expect(state.user?.spotifyConnected).toBe(true);
  });

  it('should not update Spotify connection if no user', () => {
    const { updateSpotifyConnection } = useUserStore.getState();
    
    updateSpotifyConnection(true);
    
    const state = useUserStore.getState();
    expect(state.user).toBeNull();
  });
});
