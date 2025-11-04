import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { Toaster } from 'react-hot-toast';
import { useEffect } from 'react';
import { useAuth } from 'react-oidc-context';
import { KeycloakProvider } from './providers/KeycloakProvider';
import { ErrorBoundary } from './components/ErrorBoundary';
import { ProtectedRoute } from './components/ProtectedRoute';
import { showToast } from './utils/toast';

import { LandingPage } from './pages/LandingPage';
import { DashboardPage } from './pages/DashboardPage';
import { SettingsPage } from './pages/SettingsPage';
import { PlaylistCreatorPage } from './pages/PlaylistCreatorPage';
import { DiscoverPage } from './pages/DiscoverPage';
import { DuplicateCleanerPage } from './pages/DuplicateCleanerPage';
import { SuggestionsPage } from './pages/SuggestionsPage';
import { AgentControlPage } from './pages/AgentControlPage';
import { AnalyticsPage } from './pages/AnalyticsPage';
import { HistoryPage } from './pages/HistoryPage';

function AppContent() {
  const auth = useAuth();

  useEffect(() => {
    if (auth.isAuthenticated && auth.user) {
      const name = auth.user.profile.name || auth.user.profile.preferred_username || 'User';
      showToast.success(`Welcome, ${name}!`);
    }
  }, [auth.isAuthenticated, auth.user]);

  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<LandingPage />} />
        <Route
          path="/dashboard"
          element={
            <ProtectedRoute>
              <DashboardPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/playlist-creator"
          element={
            <ProtectedRoute>
              <PlaylistCreatorPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/discover"
          element={
            <ProtectedRoute>
              <DiscoverPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/duplicate-cleaner"
          element={
            <ProtectedRoute>
              <DuplicateCleanerPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/suggestions"
          element={
            <ProtectedRoute>
              <SuggestionsPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/agent-control"
          element={
            <ProtectedRoute>
              <AgentControlPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/analytics"
          element={
            <ProtectedRoute>
              <AnalyticsPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/history"
          element={
            <ProtectedRoute>
              <HistoryPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/settings"
          element={
            <ProtectedRoute>
              <SettingsPage />
            </ProtectedRoute>
          }
        />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
      <Toaster position="top-right" />
    </BrowserRouter>
  );
}

function App() {
  return (
    <ErrorBoundary>
      <KeycloakProvider>
        <AppContent />
      </KeycloakProvider>
    </ErrorBoundary>
  );
}

export default App;
