import { createContext, useContext, useEffect, useState } from 'react';
import type { ReactNode } from 'react';
import type { ThemeData } from '../types/theme';
import { themeApi } from '../services/api';
import { useAuth } from 'react-oidc-context';

interface ThemeContextType {
  theme: ThemeData | null;
  isLoading: boolean;
  hasCustomTheme: boolean;
  applyTheme: (theme: ThemeData) => void;
  resetTheme: () => void;
  reloadTheme: () => Promise<void>;
}

const defaultTheme: ThemeData = {
  primaryColor: '#10B981',
  secondaryColor: '#059669',
  accentColor: '#34D399',
  backgroundColor: '#F9FAFB',
  textColor: '#111827',
  sidebarColor: '#14532d',
  cardBackground: '#FFFFFF',
  borderColor: '#E5E7EB',
};

const ThemeContext = createContext<ThemeContextType | undefined>(undefined);

export function ThemeProvider({ children }: { children: ReactNode }) {
  const [theme, setTheme] = useState<ThemeData | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [hasCustomTheme, setHasCustomTheme] = useState(false);
  const auth = useAuth();

  useEffect(() => {
    const loadTheme = async () => {
      if (!auth.isAuthenticated) {
        setTheme(null);
        setIsLoading(false);
        return;
      }

      try {
        const response = await themeApi.getCurrent();
        setTheme(response.themeData);
        setHasCustomTheme(true);
        applyThemeToDOM(response.themeData);
      } catch {
        console.log('No saved theme found, using default');
        setTheme(null);
        setHasCustomTheme(false);
        applyThemeToDOM(defaultTheme);
      } finally {
        setIsLoading(false);
      }
    };

    loadTheme();
  }, [auth.isAuthenticated]);

  const applyThemeToDOM = (themeData: ThemeData) => {
    const root = document.documentElement;
    root.style.setProperty('--color-theme-primary', themeData.primaryColor);
    root.style.setProperty('--color-theme-secondary', themeData.secondaryColor);
    root.style.setProperty('--color-theme-accent', themeData.accentColor);
    root.style.setProperty('--color-theme-background', themeData.backgroundColor);
    root.style.setProperty('--color-theme-text', themeData.textColor);
    root.style.setProperty('--color-theme-sidebar', themeData.sidebarColor);
    root.style.setProperty('--color-theme-card', themeData.cardBackground);
    root.style.setProperty('--color-theme-border', themeData.borderColor);
  };

  const applyTheme = (newTheme: ThemeData) => {
    setTheme(newTheme);
    applyThemeToDOM(newTheme);
  };

  const resetTheme = () => {
    setTheme(null);
    setHasCustomTheme(false);
    applyThemeToDOM(defaultTheme);
  };

  const reloadTheme = async () => {
    try {
      const response = await themeApi.getCurrent();
      setTheme(response.themeData);
      setHasCustomTheme(true);
      applyThemeToDOM(response.themeData);
    } catch {
      setTheme(null);
      setHasCustomTheme(false);
      applyThemeToDOM(defaultTheme);
    }
  };

  return (
    <ThemeContext.Provider value={{ theme, isLoading, hasCustomTheme, applyTheme, resetTheme, reloadTheme }}>
      {children}
    </ThemeContext.Provider>
  );
}

// eslint-disable-next-line react-refresh/only-export-components
export function useTheme() {
  const context = useContext(ThemeContext);
  if (context === undefined) {
    throw new Error('useTheme must be used within a ThemeProvider');
  }
  return context;
}
