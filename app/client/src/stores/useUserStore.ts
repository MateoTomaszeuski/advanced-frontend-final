import { create } from 'zustand';
import { devtools, persist } from 'zustand/middleware';

export interface User {
  id: string;
  email: string;
  name: string;
  spotifyConnected: boolean;
}

interface UserState {
  user: User | null;
  setUser: (user: User | null) => void;
  updateSpotifyConnection: (connected: boolean) => void;
  clearUser: () => void;
}

export const useUserStore = create<UserState>()(
  devtools(
    persist(
      (set) => ({
        user: null,
        setUser: (user) => set({ user }),
        updateSpotifyConnection: (connected) =>
          set((state) => ({
            user: state.user ? { ...state.user, spotifyConnected: connected } : null,
          })),
        clearUser: () => set({ user: null }),
      }),
      {
        name: 'user-storage',
      }
    )
  )
);
