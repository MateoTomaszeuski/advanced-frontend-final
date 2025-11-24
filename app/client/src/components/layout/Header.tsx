import { useAuth } from 'react-oidc-context';
import { useUIStore } from '../../stores/useUIStore';
import { Button } from '../forms/Button';

export function Header() {
  const auth = useAuth();
  const { toggleSidebar, isSidebarOpen } = useUIStore();

  return (
    <header className="bg-theme-card border-b border-theme-border px-6 py-4 relative z-10">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <button
            onClick={toggleSidebar}
            className="p-2 hover:bg-gray-100 rounded-lg transition-colors focus:outline-none focus:ring-2 focus:ring-theme-accent"
            aria-label={isSidebarOpen ? 'Close menu' : 'Open menu'}
          >
            {isSidebarOpen ? (
              <svg className="w-6 h-6 text-theme-text" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            ) : (
              <svg className="w-6 h-6 text-theme-text" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M4 6h16M4 12h16M4 18h16"
                />
              </svg>
            )}
          </button>
          <h2 className="text-lg font-semibold text-theme-text hidden sm:block">
            Spotify Agent
          </h2>
        </div>

        <div className="flex items-center gap-4">
          <div className="text-right hidden sm:block">
            <p className="text-sm font-medium text-theme-text">
              {auth.user?.profile?.name || 'User'}
            </p>
            <p className="text-xs text-theme-text opacity-70">{auth.user?.profile?.email}</p>
          </div>
          <Button
            variant="primary"
            onClick={async () => {
              try {
                // Remove user from session
                await auth.removeUser();
                // Clear session storage
                sessionStorage.clear();
                // Redirect to home
                window.location.href = '/';
              } catch (error) {
                console.error('Logout error:', error);
                // Force clear and redirect
                sessionStorage.clear();
                window.location.href = '/';
              }
            }}
          >
            Logout
          </Button>
        </div>
      </div>
    </header>
  );
}
