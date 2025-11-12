import { create } from 'zustand';
import { devtools, persist } from 'zustand/middleware';

export interface User {
  id: string;
  email: string;
  name: string;
  spotifyConnected: boolean;
}

export interface UserPreferences {
  notificationsEnabled: boolean;
}

interface UserState {
  user: User | null;
  preferences: UserPreferences;
  setUser: (user: User | null) => void;
  updateSpotifyConnection: (connected: boolean) => void;
  updatePreferences: (preferences: Partial<UserPreferences>) => void;
  clearUser: () => void;
}

const defaultPreferences: UserPreferences = {
  notificationsEnabled: true,
};

export const useUserStore = create<UserState>()(
  devtools(
    persist(
      (set) => ({
        user: null,
        preferences: defaultPreferences,
        setUser: (user) => set({ user }),
        updateSpotifyConnection: (connected) =>
          set((state) => ({
            user: state.user ? { ...state.user, spotifyConnected: connected } : null,
          })),
        updatePreferences: (newPreferences) =>
          set((state) => ({
            preferences: { ...state.preferences, ...newPreferences },
          })),
        clearUser: () => set({ user: null, preferences: defaultPreferences }),
      }),
      {
        name: 'user-storage',
      }
    )
  )
);
