import { MainLayout } from '../components/layout/MainLayout';
import { Button } from '../components/forms/Button';
import { useState, useEffect, useRef } from 'react';
import { agentApi } from '../services/api';
import type { AppAnalytics } from '../types/api';
import { showToast } from '../utils/toast';
import { useAgentStore } from '../stores/useAgentStore';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  BarElement,
  ArcElement,
  Title,
  Tooltip,
  Legend,
  Filler,
} from 'chart.js';
import { Line, Bar, Doughnut } from 'react-chartjs-2';

ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  BarElement,
  ArcElement,
  Title,
  Tooltip,
  Legend,
  Filler
);

export function AnalyticsPage() {
  const [analytics, setAnalytics] = useState<AppAnalytics | null>(null);
  const [loading, setLoading] = useState(true);
  const analyticsLoaded = useRef(false);
  const zustandConversations = useAgentStore((state) => state.conversations);

  useEffect(() => {
    if (!analyticsLoaded.current) {
      analyticsLoaded.current = true;
      loadAnalytics();
    }
  }, []);

  const loadAnalytics = async () => {
    setLoading(true);
    try {
      const data = await agentApi.getAnalytics();
      setAnalytics(data as AppAnalytics);
      showToast.success('Analytics loaded successfully');
    } catch (error) {
      console.error('Failed to load analytics:', error);
      showToast.error('Failed to load analytics');
    } finally {
      setLoading(false);
    }
  };

  // Activity over time line chart
  const activityChartData = analytics
    ? {
        labels: Object.keys(analytics.actionsOverTime),
        datasets: [
          {
            label: 'Actions Per Day',
            data: Object.values(analytics.actionsOverTime),
            borderColor: 'rgba(5, 150, 105, 1)',
            backgroundColor: 'rgba(5, 150, 105, 0.1)',
            fill: true,
            tension: 0.4,
          },
        ],
      }
    : null;

  // Action types bar chart
  const actionTypesChartData = analytics
    ? {
        labels: ['Smart Playlists', 'Music Discovery', 'Duplicate Scans', 'Duplicate Removals', 'Music Suggestions'],
        datasets: [
          {
            label: 'Actions by Type',
            data: [
              analytics.actionTypes.smartPlaylists,
              analytics.actionTypes.musicDiscovery,
              analytics.actionTypes.duplicateScans,
              analytics.actionTypes.duplicateRemovals,
              analytics.actionTypes.musicSuggestions,
            ],
            backgroundColor: [
              'rgba(5, 150, 105, 0.8)',
              'rgba(16, 185, 129, 0.8)',
              'rgba(52, 211, 153, 0.8)',
              'rgba(110, 231, 183, 0.8)',
              'rgba(167, 243, 208, 0.8)',
            ],
            borderColor: [
              'rgba(5, 150, 105, 1)',
              'rgba(16, 185, 129, 1)',
              'rgba(52, 211, 153, 1)',
              'rgba(110, 231, 183, 1)',
              'rgba(167, 243, 208, 1)',
            ],
            borderWidth: 1,
          },
        ],
      }
    : null;

  // Duplicate stats doughnut chart
  const duplicateStatsChartData = analytics
    ? {
        labels: ['Duplicates Removed', 'Duplicates Still Present'],
        datasets: [
          {
            data: [
              analytics.duplicates.totalDuplicatesRemoved,
              Math.max(0, analytics.duplicates.totalDuplicatesFound - analytics.duplicates.totalDuplicatesRemoved),
            ],
            backgroundColor: ['rgba(5, 150, 105, 0.8)', 'rgba(239, 68, 68, 0.8)'],
            borderColor: ['rgba(5, 150, 105, 1)', 'rgba(239, 68, 68, 1)'],
            borderWidth: 1,
          },
        ],
      }
    : null;

  return (
    <MainLayout>
      <div className="max-w-7xl mx-auto">
        <div className="flex justify-between items-center mb-8">
          <h1 className="text-3xl font-bold text-theme-text">App Usage Analytics</h1>
          <Button onClick={loadAnalytics} disabled={loading}>
            {loading ? 'Loading...' : 'Refresh'}
          </Button>
        </div>

        {loading && !analytics && (
          <div className="flex items-center justify-center h-64">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-green-600"></div>
          </div>
        )}

        {analytics && (
          <>
            {/* Key Metrics Cards */}
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
              <div className="bg-theme-card rounded-lg shadow p-6 border border-theme-border">
                <h3 className="text-sm font-medium text-theme-text opacity-70 mb-2">Total Actions</h3>
                <p className="text-3xl font-bold text-theme-accent">{analytics.userActivity.totalActions}</p>
                <p className="text-xs text-theme-text opacity-60 mt-1">Completed operations</p>
              </div>
              <div className="bg-theme-card rounded-lg shadow p-6 border border-theme-border">
                <h3 className="text-sm font-medium text-theme-text opacity-70 mb-2">Playlists Created</h3>
                <p className="text-3xl font-bold text-theme-accent">{analytics.userActivity.totalPlaylistsCreated}</p>
                <p className="text-xs text-theme-text opacity-60 mt-1">AI-generated playlists</p>
              </div>
              <div className="bg-theme-card rounded-lg shadow p-6 border border-theme-border">
                <h3 className="text-sm font-medium text-theme-text opacity-70 mb-2">Conversations</h3>
                <p className="text-3xl font-bold text-theme-accent">{zustandConversations.length}</p>
                <p className="text-xs text-theme-text opacity-60 mt-1">Active sessions</p>
              </div>
              <div className="bg-theme-card rounded-lg shadow p-6 border border-theme-border">
                <h3 className="text-sm font-medium text-theme-text opacity-70 mb-2">Duplicates Found</h3>
                <p className="text-3xl font-bold text-theme-accent">{analytics.duplicates.totalDuplicatesFound}</p>
                <p className="text-xs text-theme-text opacity-60 mt-1">Across all playlists</p>
              </div>
            </div>

            {/* Charts Grid */}
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
              {/* Activity Over Time */}
              <div className="bg-theme-card rounded-lg shadow p-6 border border-theme-border">
                <h2 className="text-xl font-semibold text-theme-text mb-4">Activity Over Time (Last 30 Days)</h2>
                {activityChartData && Object.keys(analytics.actionsOverTime).length > 0 ? (
                  <div className="h-80">
                    <Line
                      data={activityChartData}
                      options={{
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: {
                          legend: {
                            display: false,
                          },
                        },
                        scales: {
                          y: {
                            beginAtZero: true,
                            ticks: {
                              stepSize: 1,
                            },
                          },
                        },
                      }}
                    />
                  </div>
                ) : (
                  <div className="h-80 flex items-center justify-center text-theme-text opacity-70">
                    <div className="text-center">
                      <p className="text-4xl mb-2">📊</p>
                      <p>No activity data yet</p>
                      <p className="text-sm">Start using the app to see your activity</p>
                    </div>
                  </div>
                )}
              </div>

              {/* Action Types Distribution */}
              <div className="bg-theme-card rounded-lg shadow p-6 border border-theme-border">
                <h2 className="text-xl font-semibold text-theme-text mb-4">Actions by Type</h2>
                {actionTypesChartData && analytics.userActivity.totalActions > 0 ? (
                  <div className="h-80">
                    <Bar
                      data={actionTypesChartData}
                      options={{
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: {
                          legend: {
                            display: false,
                          },
                        },
                        scales: {
                          y: {
                            beginAtZero: true,
                            ticks: {
                              stepSize: 1,
                            },
                          },
                        },
                      }}
                    />
                  </div>
                ) : (
                  <div className="h-80 flex items-center justify-center text-theme-text opacity-70">
                    <div className="text-center">
                      <p className="text-4xl mb-2">📈</p>
                      <p>No actions yet</p>
                      <p className="text-sm">Use features to see breakdown</p>
                    </div>
                  </div>
                )}
              </div>
            </div>

            {/* Detailed Stats */}
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
              {/* Feature Usage Stats */}
              <div className="bg-theme-card rounded-lg shadow p-6 border border-theme-border">
                <h2 className="text-xl font-semibold text-theme-text mb-4">Feature Usage</h2>
                <div className="space-y-4">
                  <div>
                    <div className="flex justify-between text-sm mb-1">
                      <span className="text-theme-text opacity-70">Smart Playlists</span>
                      <span className="font-medium text-theme-text">{analytics.actionTypes.smartPlaylists} created</span>
                    </div>
                    <div className="w-full bg-gray-200 rounded-full h-2">
                      <div
                        className="bg-green-600 h-2 rounded-full"
                        style={{
                          width: `${
                            analytics.userActivity.totalActions > 0
                              ? (analytics.actionTypes.smartPlaylists / analytics.userActivity.totalActions) * 100
                              : 0
                          }%`,
                        }}
                      />
                    </div>
                  </div>
                  <div>
                    <div className="flex justify-between text-sm mb-1">
                      <span className="text-theme-text opacity-70">Music Discovery</span>
                      <span className="font-medium text-theme-text">{analytics.actionTypes.musicDiscovery} sessions</span>
                    </div>
                    <div className="w-full bg-gray-200 rounded-full h-2">
                      <div
                        className="bg-green-600 h-2 rounded-full"
                        style={{
                          width: `${
                            analytics.userActivity.totalActions > 0
                              ? (analytics.actionTypes.musicDiscovery / analytics.userActivity.totalActions) * 100
                              : 0
                          }%`,
                        }}
                      />
                    </div>
                  </div>
                  <div>
                    <div className="flex justify-between text-sm mb-1">
                      <span className="text-theme-text opacity-70">Duplicate Scans</span>
                      <span className="font-medium text-theme-text">{analytics.actionTypes.duplicateScans} scans</span>
                    </div>
                    <div className="w-full bg-gray-200 rounded-full h-2">
                      <div
                        className="bg-green-600 h-2 rounded-full"
                        style={{
                          width: `${
                            analytics.userActivity.totalActions > 0
                              ? (analytics.actionTypes.duplicateScans / analytics.userActivity.totalActions) * 100
                              : 0
                          }%`,
                        }}
                      />
                    </div>
                  </div>
                  <div>
                    <div className="flex justify-between text-sm mb-1">
                      <span className="text-theme-text opacity-70">Music Suggestions</span>
                      <span className="font-medium text-theme-text">{analytics.actionTypes.musicSuggestions} requests</span>
                    </div>
                    <div className="w-full bg-gray-200 rounded-full h-2">
                      <div
                        className="bg-green-600 h-2 rounded-full"
                        style={{
                          width: `${
                            analytics.userActivity.totalActions > 0
                              ? (analytics.actionTypes.musicSuggestions / analytics.userActivity.totalActions) * 100
                              : 0
                          }%`,
                        }}
                      />
                    </div>
                  </div>
                </div>
              </div>

              {/* Duplicate Management Stats */}
              <div className="bg-theme-card rounded-lg shadow p-6 border border-theme-border">
                <h2 className="text-xl font-semibold text-theme-text mb-4">Duplicate Management</h2>
                {duplicateStatsChartData && analytics.duplicates.totalDuplicatesFound > 0 ? (
                  <div className="h-64 flex items-center justify-center mb-4">
                    <Doughnut
                      data={duplicateStatsChartData}
                      options={{
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: {
                          legend: {
                            position: 'bottom',
                          },
                        },
                      }}
                    />
                  </div>
                ) : null}
                <div className="space-y-3">
                  <div className="flex justify-between items-center">
                    <span className="text-sm text-theme-text opacity-70">Total Scans</span>
                    <span className="font-semibold text-theme-text">{analytics.duplicates.totalScans}</span>
                  </div>
                  <div className="flex justify-between items-center">
                    <span className="text-sm text-theme-text opacity-70">Duplicates Found</span>
                    <span className="font-semibold text-theme-text">{analytics.duplicates.totalDuplicatesFound}</span>
                  </div>
                  <div className="flex justify-between items-center">
                    <span className="text-sm text-theme-text opacity-70">Duplicates Removed</span>
                    <span className="font-semibold text-green-700">{analytics.duplicates.totalDuplicatesRemoved}</span>
                  </div>
                  <div className="flex justify-between items-center pt-3 border-t">
                    <span className="text-sm text-theme-text opacity-70">Avg per Playlist</span>
                    <span className="font-semibold text-theme-text">
                      {analytics.duplicates.averageDuplicatesPerPlaylist.toFixed(1)}
                    </span>
                  </div>
                </div>
              </div>
            </div>
          </>
        )}
      </div>
    </MainLayout>
  );
}
