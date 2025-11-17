import type { ReactNode } from 'react';
import { Sidebar } from './Sidebar';
import { Header } from './Header';
import { useUIStore } from '../../stores/useUIStore';

interface MainLayoutProps {
  children: ReactNode;
}

export function MainLayout({ children }: MainLayoutProps) {
  const { isSidebarOpen, isTransitioning } = useUIStore();

  return (
    <div className="flex h-screen bg-theme-background overflow-hidden">
      <Sidebar />
      <div 
        className={`
          flex-1 flex flex-col overflow-hidden bg-theme-background
          ${isSidebarOpen ? 'md:ml-64' : 'md:ml-0'}
          ${isTransitioning ? 'transition-all duration-300 ease-in-out' : ''}
        `}
      >
        <Header />
        <main className="flex-1 overflow-y-auto p-6">{children}</main>
      </div>
    </div>
  );
}
