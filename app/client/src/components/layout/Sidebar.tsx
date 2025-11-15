import { useAuth } from 'react-oidc-context';
import { Link, useLocation } from 'react-router-dom';
import { useUIStore } from '../../stores/useUIStore';

export function Sidebar() {
  const { isSidebarOpen, toggleSidebar, isTransitioning } = useUIStore();
  const auth = useAuth();
  const location = useLocation();

  const navItems = [
    { path: '/dashboard', label: 'Dashboard', icon: 'M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6' },
    { path: '/playlist-creator', label: 'Playlist Creator', icon: 'M9 19V6l12-3v13M9 19c0 1.105-1.343 2-3 2s-3-.895-3-2 1.343-2 3-2 3 .895 3 2zm12-3c0 1.105-1.343 2-3 2s-3-.895-3-2 1.343-2 3-2 3 .895 3 2zM9 10l12-3' },
    { path: '/discover', label: 'New Music Discovery', icon: 'M5 3v4M3 5h4M6 17v4m-2-2h4m5-16l2.286 6.857L21 12l-5.714 2.143L13 21l-2.286-6.857L5 12l5.714-2.143L13 3z' },
    { path: '/duplicate-cleaner', label: 'Duplicate Cleaner', icon: 'M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z' },
    { path: '/suggestions', label: 'Music Suggestions', icon: 'M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z' },
    { path: '/agent-control', label: 'Agent Control', icon: 'M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z' },
    { path: '/analytics', label: 'Analytics', icon: 'M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z' },
    { path: '/history', label: 'Activity History', icon: 'M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z' },
    { path: '/settings', label: 'Settings', icon: 'M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z M15 12a3 3 0 11-6 0 3 3 0 016 0z' },
  ];

  return (
    <>
      {/* Desktop Sidebar */}
      <aside
        className={`
          hidden md:block
          fixed left-0 top-0 h-screen
          bg-theme-sidebar text-white
          shadow-xl z-30
          ${isSidebarOpen ? 'w-64' : 'w-0'}
          ${isTransitioning ? 'transition-all duration-300 ease-in-out' : ''}
          overflow-hidden
        `}
        style={{ color: 'white' }}
      >
        <div className="p-4 flex flex-col h-full">
          <div className="mb-8 shrink-0">
            <h1 className="text-2xl font-bold whitespace-nowrap">Spotify Agent</h1>
            <p className="text-sm text-green-200 truncate">{auth.user?.profile?.email}</p>
          </div>
          
          <nav className="flex-1 overflow-y-auto">
            <ul className="space-y-2">
              {navItems.map((item) => (
                <li key={item.path}>
                  <Link
                    to={item.path}
                    onClick={() => {
                      if (window.innerWidth < 768) {
                        toggleSidebar();
                      }
                    }}
                    className={`
                      flex items-center gap-3 px-4 py-2 rounded-lg
                      transition-colors whitespace-nowrap
                      ${location.pathname === item.path 
                        ? 'bg-theme-primary font-semibold' 
                        : 'hover:bg-theme-primary hover:bg-opacity-70'
                      }
                    `}
                  >
                    <svg className="w-5 h-5 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d={item.icon} />
                    </svg>
                    <span>{item.label}</span>
                  </Link>
                </li>
              ))}
            </ul>
          </nav>
        </div>
      </aside>

      {/* Mobile Sidebar - Fullscreen Overlay */}
      <div
        className={`
          md:hidden
          fixed inset-0 z-50
          transition-all duration-300 ease-in-out
          ${isSidebarOpen ? 'opacity-100 visible' : 'opacity-0 invisible pointer-events-none'}
        `}
      >
        {/* Backdrop */}
        <div
          className="absolute inset-0 bg-black bg-opacity-50"
          onClick={toggleSidebar}
          aria-hidden="true"
        />
        
        {/* Menu Panel */}
        <div
          className={`
            absolute top-0 left-0 right-0
            bg-theme-sidebar text-white
            shadow-lg
            transform transition-transform duration-300 ease-in-out
            ${isSidebarOpen ? 'translate-y-0' : '-translate-y-full'}
          `}
          style={{ color: 'white' }}
        >
          <div className="p-4">
            <div className="flex items-center justify-between mb-4">
              <div>
                <h1 className="text-xl font-bold">Spotify Agent</h1>
                <p className="text-xs text-green-200 truncate">{auth.user?.profile?.email}</p>
              </div>
              <button
                onClick={toggleSidebar}
                className="p-2 hover:bg-theme-primary hover:bg-opacity-70 rounded-lg transition-colors"
                aria-label="Close menu"
              >
                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>
            
            <nav className="overflow-y-auto max-h-[calc(100vh-200px)]">
              <ul className="space-y-2">
                {navItems.map((item) => (
                  <li key={item.path}>
                    <Link
                      to={item.path}
                      onClick={toggleSidebar}
                      className={`
                        flex items-center gap-3 px-4 py-3 rounded-lg
                        transition-colors
                        ${location.pathname === item.path 
                          ? 'bg-theme-primary font-semibold' 
                          : 'hover:bg-theme-primary hover:bg-opacity-70'
                        }
                      `}
                    >
                      <svg className="w-5 h-5 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d={item.icon} />
                      </svg>
                      <span>{item.label}</span>
                    </Link>
                  </li>
                ))}
              </ul>
            </nav>
          </div>
        </div>
      </div>
    </>
  );
}
