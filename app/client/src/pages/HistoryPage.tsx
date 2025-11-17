import { MainLayout } from '../components/layout/MainLayout';
import { FilterPanel } from '../components/history/FilterPanel';
import { ActionList } from '../components/history/ActionList';
import { useEffect, useState, useCallback } from 'react';
import { agentApi } from '../services/api';
import type { AgentAction } from '../types/api';
import { Button } from '../components/forms/Button';
import { showToast } from '../utils/toast';

export function HistoryPage() {
  const [actions, setActions] = useState<AgentAction[]>([]);
  const [loading, setLoading] = useState(true);
  const [filterType, setFilterType] = useState<string>('');
  const [filterStatus, setFilterStatus] = useState<string>('');

  const loadHistory = useCallback(async () => {
    setLoading(true);
    try {
      const history = await agentApi.getHistory({
        actionType: filterType || undefined,
        status: filterStatus || undefined,
        limit: 100,
      });
      setActions(history);
    } catch (error) {
      console.error('Failed to load history:', error);
      showToast.error(
        error instanceof Error ? error.message : 'Failed to load activity history'
      );
    } finally {
      setLoading(false);
    }
  }, [filterType, filterStatus]);

  useEffect(() => {
    loadHistory();
  }, [loadHistory]);

  const handleClearFilters = () => {
    setFilterType('');
    setFilterStatus('');
  };

  return (
    <MainLayout>
      <div className="max-w-6xl mx-auto">
        <div className="flex items-center justify-between mb-6">
          <h1 className="text-3xl font-bold text-theme-text">Activity History</h1>
          <Button onClick={loadHistory} variant="secondary">
            Refresh
          </Button>
        </div>

        <FilterPanel
          filterType={filterType}
          setFilterType={setFilterType}
          filterStatus={filterStatus}
          setFilterStatus={setFilterStatus}
          onClearFilters={handleClearFilters}
        />

        <ActionList actions={actions} loading={loading} />
      </div>
    </MainLayout>
  );
}
