import { MainLayout } from '../components/layout/MainLayout';
import { SpotifyConnectionAlert } from '../components/SpotifyConnectionAlert';
import { useAgentStore } from '../stores/useAgentStore';
import { Button } from '../components/forms/Button';
import { useNavigate } from 'react-router-dom';
import { useState, useEffect } from 'react';
import { agentApi } from '../services/api';
import type { AppAnalytics, AgentAction } from '../types/api';
import { showToast } from '../utils/toast';

export function DashboardPage() {
  const { status, currentTask } = useAgentStore();
  const navigate = useNavigate();
  const [analytics, setAnalytics] = useState<AppAnalytics | null>(null);
  const [recentActions, setRecentActions] = useState<AgentAction[]>([]);

  useEffect(() => {
    const fetchData = async () => {
      try {
        const [analyticsData, historyData] = await Promise.all([
          agentApi.getAnalytics(),
          agentApi.getHistory({ limit: 5 })
        ]);
        setAnalytics(analyticsData as AppAnalytics);
        setRecentActions(historyData as AgentAction[]);
      } catch (error) {
        console.error('Failed to fetch data:', error);
        showToast.error(
          error instanceof Error ? error.message : 'Failed to load dashboard data'
        );
      }
    };
    fetchData();
  }, []);

  const displayedActions = recentActions;

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
      color: 'bg-green-100 text-green-700',
    },
    {
      title: 'Discover New Music',
      description: 'Find fresh tracks based on your listening history',
      icon: (
        <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
        </svg>
      ),
      path: '/discover',
      color: 'bg-blue-100 text-blue-700',
    },
    {
      title: 'Scan Duplicates',
      description: 'Find and remove duplicate songs from playlists',
      icon: (
        <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7h12m0 0l-4-4m4 4l-4 4m0 6H4m0 0l4 4m-4-4l4-4" />
        </svg>
      ),
      path: '/duplicate-cleaner',
      color: 'bg-yellow-100 text-yellow-700',
    },
    {
      title: 'Get Suggestions',
      description: 'Receive AI-powered music recommendations',
      icon: (
        <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z" />
        </svg>
      ),
      path: '/suggestions',
      color: 'bg-purple-100 text-purple-700',
    },
  ];

  return (
    <MainLayout>
      <div className="max-w-7xl mx-auto">
        <h1 className="text-3xl font-bold text-gray-900 mb-8">Dashboard</h1>

        <SpotifyConnectionAlert />

        <div className="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
          <div className="bg-white rounded-lg shadow p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600">Agent Status</p>
                <p className="text-2xl font-bold text-gray-900 capitalize">{status}</p>
              </div>
              <div className={`w-12 h-12 rounded-full flex items-center justify-center ${
                status === 'idle' ? 'bg-gray-100' :
                status === 'processing' ? 'bg-green-100' :
                status === 'awaiting-approval' ? 'bg-yellow-100' :
                'bg-red-100'
              }`}>
                {status === 'processing' ? (
                  <div className="animate-spin h-6 w-6 border-2 border-green-600 border-t-transparent rounded-full" />
                ) : (
                  <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                )}
              </div>
            </div>
          </div>

          <div className="bg-white rounded-lg shadow p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600">Total Actions</p>
                <p className="text-2xl font-bold text-gray-900">
                  {analytics?.userActivity.totalActions ?? 0}
                </p>
              </div>
              <div className="w-12 h-12 rounded-full bg-green-100 flex items-center justify-center">
                <svg className="w-6 h-6 text-green-700" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
                </svg>
              </div>
            </div>
          </div>

          <div className="bg-white rounded-lg shadow p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600">Completed</p>
                <p className="text-2xl font-bold text-gray-900">
                  {analytics?.userActivity.completedActions ?? 0}
                </p>
              </div>
              <div className="w-12 h-12 rounded-full bg-green-100 flex items-center justify-center">
                <svg className="w-6 h-6 text-green-700" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                </svg>
              </div>
            </div>
          </div>

          <div className="bg-white rounded-lg shadow p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600">Failed</p>
                <p className="text-2xl font-bold text-gray-900">
                  {analytics?.userActivity.failedActions ?? 0}
                </p>
              </div>
              <div className="w-12 h-12 rounded-full bg-red-100 flex items-center justify-center">
                <svg className="w-6 h-6 text-red-700" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
              </div>
            </div>
          </div>
        </div>

        {currentTask && (
          <div className="bg-green-50 border border-green-200 rounded-lg p-6 mb-8">
            <div className="flex items-start gap-3">
              <div className="animate-spin h-5 w-5 border-2 border-green-600 border-t-transparent rounded-full mt-0.5" />
              <div className="flex-1">
                <h2 className="text-lg font-semibold text-green-900 mb-1">Current Task</h2>
                <p className="text-green-700">{currentTask}</p>
              </div>
            </div>
          </div>
        )}

        <div className="bg-white rounded-lg shadow mb-8">
          <div className="p-6 border-b border-gray-200">
            <h2 className="text-xl font-semibold text-gray-900">Quick Actions</h2>
          </div>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 p-6">
            {quickActions.map((action) => (
              <button
                key={action.path}
                onClick={() => navigate(action.path)}
                className="flex flex-col items-start p-4 border-2 border-gray-200 rounded-lg hover:border-green-500 hover:shadow-md transition-all group"
              >
                <div className={`${action.color} p-3 rounded-lg mb-3 group-hover:scale-110 transition-transform`}>
                  {action.icon}
                </div>
                <h3 className="text-sm font-semibold text-gray-900 mb-1 group-hover:text-green-700 transition-colors">
                  {action.title}
                </h3>
                <p className="text-xs text-gray-500">{action.description}</p>
              </button>
            ))}
          </div>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
          <div className="bg-white rounded-lg shadow">
            <div className="p-6 border-b border-gray-200 flex items-center justify-between">
              <h2 className="text-xl font-semibold text-gray-900">Recent Activity</h2>
              <Button variant="ghost" onClick={() => navigate('/history')}>View All</Button>
            </div>
            <div className="p-6">
              {displayedActions.length === 0 ? (
                <div className="text-center py-8">
                  <svg className="mx-auto h-12 w-12 text-gray-400 mb-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                  </svg>
                  <p className="text-gray-500">No recent activity</p>
                  <p className="text-sm text-gray-400 mt-1">Start by creating a playlist or discovering new music</p>
                </div>
              ) : (
                <ul className="space-y-4">
                  {displayedActions.map((action) => (
                    <li key={action.id} className="flex items-start gap-3 pb-4 border-b border-gray-100 last:border-0">
                      <div className={`w-2 h-2 rounded-full mt-2 shrink-0 ${
                        action.status === 'Completed' ? 'bg-green-500' :
                        action.status === 'Failed' ? 'bg-red-500' :
                        action.status === 'Processing' ? 'bg-blue-500' :
                        'bg-yellow-500'
                      }`} />
                      <div className="flex-1 min-w-0">
                        <p className="font-medium text-gray-900 truncate">{action.actionType.replace(/([A-Z])/g, ' $1').trim()}</p>
                        <p className="text-sm text-gray-500 mt-0.5">
                          {new Date(action.createdAt).toLocaleString()}
                        </p>
                      </div>
                      <span className={`text-xs px-2 py-1 rounded-full whitespace-nowrap shrink-0 ${
                        action.status === 'Completed' ? 'bg-green-100 text-green-800' :
                        action.status === 'Failed' ? 'bg-red-100 text-red-800' :
                        action.status === 'Processing' ? 'bg-blue-100 text-blue-800' :
                        'bg-yellow-100 text-yellow-800'
                      }`}>
                        {action.status}
                      </span>
                    </li>
                  ))}
                </ul>
              )}
            </div>
          </div>

          <div className="bg-white rounded-lg shadow">
            <div className="p-6 border-b border-gray-200 flex items-center justify-between">
              <h2 className="text-xl font-semibold text-gray-900">More Tools</h2>
            </div>
            <div className="p-6 space-y-3">
              <button
                onClick={() => navigate('/analytics')}
                className="w-full flex items-center gap-4 p-4 border border-gray-200 rounded-lg hover:border-green-500 hover:shadow-md transition-all group text-left"
              >
                <div className="bg-purple-100 p-3 rounded-lg group-hover:scale-110 transition-transform">
                  <svg className="w-6 h-6 text-purple-700" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
                  </svg>
                </div>
                <div className="flex-1">
                  <h3 className="font-semibold text-gray-900 group-hover:text-green-700 transition-colors">App Usage Analytics</h3>
                  <p className="text-sm text-gray-500">Visualize app usage and insights</p>
                </div>
              </button>

              <button
                onClick={() => navigate('/agent-control')}
                className="w-full flex items-center gap-4 p-4 border border-gray-200 rounded-lg hover:border-green-500 hover:shadow-md transition-all group text-left"
              >
                <div className="bg-indigo-100 p-3 rounded-lg group-hover:scale-110 transition-transform">
                  <svg className="w-6 h-6 text-indigo-700" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                  </svg>
                </div>
                <div className="flex-1">
                  <h3 className="font-semibold text-gray-900 group-hover:text-green-700 transition-colors">Agent Control Center</h3>
                  <p className="text-sm text-gray-500">Monitor and manage agent tasks</p>
                </div>
              </button>

              <button
                onClick={() => navigate('/settings')}
                className="w-full flex items-center gap-4 p-4 border border-gray-200 rounded-lg hover:border-green-500 hover:shadow-md transition-all group text-left"
              >
                <div className="bg-gray-100 p-3 rounded-lg group-hover:scale-110 transition-transform">
                  <svg className="w-6 h-6 text-gray-700" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6V4m0 2a2 2 0 100 4m0-4a2 2 0 110 4m-6 8a2 2 0 100-4m0 4a2 2 0 110-4m0 4v2m0-6V4m6 6v10m6-2a2 2 0 100-4m0 4a2 2 0 110-4m0 4v2m0-6V4" />
                  </svg>
                </div>
                <div className="flex-1">
                  <h3 className="font-semibold text-gray-900 group-hover:text-green-700 transition-colors">Settings</h3>
                  <p className="text-sm text-gray-500">Configure your account and preferences</p>
                </div>
              </button>
            </div>
          </div>
        </div>
      </div>
    </MainLayout>
  );
}
