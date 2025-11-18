import { MainLayout } from '../components/layout/MainLayout';
import { SpotifyConnectionAlert } from '../components/SpotifyConnectionAlert';
import { MetricCard } from '../components/dashboard/MetricCard';
import { QuickActionCard } from '../components/dashboard/QuickActionCard';
import { RecentActivity } from '../components/dashboard/RecentActivity';
import { ToolCard } from '../components/dashboard/ToolCard';
import { CurrentTask } from '../components/dashboard/CurrentTask';
import { useAgentStore } from '../stores/useAgentStore';
import { useNavigate } from 'react-router-dom';
import { useState, useEffect } from 'react';
import { agentApi } from '../services/api';
import type { AppAnalytics, AgentAction } from '../types/api';
import { showToast } from '../utils/toast';
import { useTheme } from '../contexts/ThemeContext';

export function DashboardPage() {
  const { status, currentTask } = useAgentStore();
  const { hasCustomTheme } = useTheme();
  const navigate = useNavigate();
  const [analytics, setAnalytics] = useState<AppAnalytics | null>(null);
  const [recentActions, setRecentActions] = useState<AgentAction[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const fetchData = async () => {
      try {
        const [analyticsData, historyData] = await Promise.all([
          agentApi.getAnalytics(),
          agentApi.getHistory({ limit: 5 }),
        ]);
        
        setRecentActions(historyData as AgentAction[]);
        setAnalytics(analyticsData as AppAnalytics);
      } catch (error) {
        console.error('Failed to fetch data:', error);
        showToast.error(
          error instanceof Error ? error.message : 'Failed to load dashboard data'
        );
      } finally {
        setIsLoading(false);
      }
    };
    fetchData();
  }, []);

  const quickActions = [
    {
      title: 'Create Smart Playlist',
      description: 'Generate a playlist using AI based on your description',
      icon: (
        <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
        </svg>
      ),
      path: '/playlist-creator',
      color: hasCustomTheme ? 'bg-theme-accent bg-opacity-20 text-theme-accent' : 'bg-green-100 text-green-700',
    },
    {
      title: 'Discover New Music',
      description: 'Find fresh tracks based on your listening history',
      icon: (
        <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={2}
            d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
          />
        </svg>
      ),
      path: '/discover',
      color: hasCustomTheme ? 'bg-theme-primary bg-opacity-20 text-theme-primary' : 'bg-blue-100 text-blue-700',
    },
    {
      title: 'Scan Duplicates',
      description: 'Find and remove duplicate songs from playlists',
      icon: (
        <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={2}
            d="M8 7h12m0 0l-4-4m4 4l-4 4m0 6H4m0 0l4 4m-4-4l4-4"
          />
        </svg>
      ),
      path: '/duplicate-cleaner',
      color: hasCustomTheme ? 'bg-theme-secondary bg-opacity-20 text-theme-secondary' : 'bg-yellow-100 text-yellow-700',
    },
    {
      title: 'Get Suggestions',
      description: 'Receive AI-powered music recommendations',
      icon: (
        <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={2}
            d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z"
          />
        </svg>
      ),
      path: '/suggestions',
      color: hasCustomTheme ? 'bg-theme-accent bg-opacity-30 text-theme-accent' : 'bg-purple-100 text-purple-700',
    },
  ];

  return (
    <MainLayout>
      <div className="max-w-7xl mx-auto">
        <h1 className="text-3xl font-bold text-theme-text mb-8">Dashboard</h1>

        <SpotifyConnectionAlert />

        <div className="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
          <MetricCard
            label="Agent Status"
            value={status.charAt(0).toUpperCase() + status.slice(1)}
            isLoading={false}
            icon={
              status === 'processing' ? (
                <div className="animate-spin h-6 w-6 border-2 border-green-600 border-t-transparent rounded-full" />
              ) : (
                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"
                  />
                </svg>
              )
            }
            iconBgColor={
              hasCustomTheme
                ? status === 'idle'
                  ? 'bg-theme-card opacity-70'
                  : status === 'processing'
                    ? 'bg-theme-accent bg-opacity-20'
                    : status === 'awaiting-approval'
                      ? 'bg-theme-secondary bg-opacity-20'
                      : 'bg-theme-primary bg-opacity-20'
                : status === 'idle'
                  ? 'bg-gray-100'
                  : status === 'processing'
                    ? 'bg-green-100'
                    : status === 'awaiting-approval'
                      ? 'bg-yellow-100'
                      : 'bg-red-100'
            }
            iconColor={
              hasCustomTheme
                ? status === 'idle'
                  ? 'text-theme-text opacity-60'
                  : status === 'processing'
                    ? 'text-theme-accent'
                    : status === 'awaiting-approval'
                      ? 'text-theme-secondary'
                      : 'text-theme-primary'
                : undefined
            }
          />

          <MetricCard
            label="Total Actions"
            value={analytics?.userActivity.totalActions ?? 0}
            isLoading={isLoading}
            icon={
              <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"
                />
              </svg>
            }
            iconBgColor={hasCustomTheme ? 'bg-theme-accent bg-opacity-20' : 'bg-green-100'}
            iconColor={hasCustomTheme ? 'text-theme-accent' : 'text-green-700'}
          />

          <MetricCard
            label="Completed"
            value={analytics?.userActivity.completedActions ?? 0}
            isLoading={isLoading}
            icon={
              <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
              </svg>
            }
            iconBgColor={hasCustomTheme ? 'bg-theme-accent bg-opacity-20' : 'bg-green-100'}
            iconColor={hasCustomTheme ? 'text-theme-accent' : 'text-green-700'}
          />

          <MetricCard
            label="Failed"
            value={analytics?.userActivity.failedActions ?? 0}
            isLoading={isLoading}
            icon={
              <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            }
            iconBgColor={hasCustomTheme ? 'bg-theme-primary bg-opacity-20' : 'bg-red-100'}
            iconColor={hasCustomTheme ? 'text-theme-primary' : 'text-red-700'}
          />
        </div>

        {currentTask && <CurrentTask task={currentTask} />}

        <div className="bg-theme-card rounded-lg shadow mb-8">
          <div className="p-6 border-b border-theme-border">
            <h2 className="text-xl font-semibold text-theme-text">Quick Actions</h2>
          </div>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 p-6">
            {quickActions.map((action) => (
              <QuickActionCard
                key={action.path}
                title={action.title}
                description={action.description}
                icon={action.icon}
                color={action.color}
                onClick={() => navigate(action.path)}
              />
            ))}
          </div>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
          <RecentActivity actions={recentActions} onViewAll={() => navigate('/history')} />

          <div className="bg-theme-card rounded-lg shadow">
            <div className="p-6 border-b border-theme-border flex items-center justify-between">
              <h2 className="text-xl font-semibold text-theme-text">More Tools</h2>
            </div>
            <div className="p-6 space-y-3">
              <ToolCard
                title="App Usage Analytics"
                description="Visualize app usage and insights"
                icon={
                  <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth={2}
                      d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"
                    />
                  </svg>
                }
                iconBgColor={hasCustomTheme ? 'bg-theme-accent bg-opacity-20' : 'bg-purple-100'}
                iconColor={hasCustomTheme ? 'text-theme-accent' : 'text-purple-700'}
                onClick={() => navigate('/analytics')}
              />

              <ToolCard
                title="Agent Control Center"
                description="Monitor and manage agent tasks"
                icon={
                  <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth={2}
                      d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z"
                    />
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth={2}
                      d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"
                    />
                  </svg>
                }
                iconBgColor={hasCustomTheme ? 'bg-theme-primary bg-opacity-20' : 'bg-indigo-100'}
                iconColor={hasCustomTheme ? 'text-theme-primary' : 'text-indigo-700'}
                onClick={() => navigate('/agent-control')}
              />

              <ToolCard
                title="Settings"
                description="Configure your account and preferences"
                icon={
                  <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth={2}
                      d="M12 6V4m0 2a2 2 0 100 4m0-4a2 2 0 110 4m-6 8a2 2 0 100-4m0 4a2 2 0 110-4m0 4v2m0-6V4m6 6v10m6-2a2 2 0 100-4m0 4a2 2 0 110-4m0 4v2m0-6V4"
                    />
                  </svg>
                }
                iconBgColor={hasCustomTheme ? 'bg-theme-secondary bg-opacity-20' : 'bg-gray-100'}
                iconColor={hasCustomTheme ? 'text-theme-secondary' : 'text-gray-700'}
                onClick={() => navigate('/settings')}
              />
            </div>
          </div>
        </div>
      </div>
    </MainLayout>
  );
}
