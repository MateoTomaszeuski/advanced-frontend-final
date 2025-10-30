import { useAuth } from 'react-oidc-context';
import { useUIStore } from '../../stores/useUIStore';
import { Button } from '../forms/Button';

export function Header() {
  const auth = useAuth();
  const { toggleSidebar } = useUIStore();

  return (
    <header className="bg-white border-b border-gray-200 px-6 py-4">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <button
            onClick={toggleSidebar}
            className="p-2 hover:bg-gray-100 rounded-lg transition-colors"
          >
            <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M4 6h16M4 12h16M4 18h16"
              />
            </svg>
          </button>
        </div>

        <div className="flex items-center gap-4">
          <div className="text-right">
            <p className="text-sm font-medium text-gray-700">
              {auth.user?.profile?.name || 'User'}
            </p>
            <p className="text-xs text-gray-500">{auth.user?.profile?.email}</p>
          </div>
          <Button
            variant="ghost"
            size="sm"
            onClick={() => auth.signoutRedirect()}
          >
            Logout
          </Button>
        </div>
      </div>
    </header>
  );
}
