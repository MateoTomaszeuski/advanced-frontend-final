interface StatusCardsProps {
  status: string;
  currentTask: string | null;
  elapsedTime: number;
}

export function StatusCards({ status, currentTask, elapsedTime }: StatusCardsProps) {
  const formatElapsedTime = (seconds: number): string => {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'idle':
        return 'text-gray-500 bg-gray-100';
      case 'processing':
        return 'text-blue-600 bg-blue-100';
      case 'awaiting-approval':
        return 'text-yellow-600 bg-yellow-100';
      case 'error':
        return 'text-red-600 bg-red-100';
      default:
        return 'text-gray-500 bg-gray-100';
    }
  };

  return (
    <div className="grid grid-cols-1 lg:grid-cols-3 gap-6 mb-8">
      <div className="bg-theme-card rounded-lg shadow p-6">
        <h3 className="text-sm font-medium text-theme-text opacity-70 mb-2">Agent Status</h3>
        <div className="flex items-center gap-3">
          <span className={`px-3 py-1 rounded-full text-sm font-medium ${getStatusColor(status)}`}>
            {status.charAt(0).toUpperCase() + status.slice(1)}
          </span>
          {status === 'processing' && (
            <div className="animate-spin h-5 w-5 border-2 border-green-600 border-t-transparent rounded-full" />
          )}
        </div>
      </div>

      <div className="bg-theme-card rounded-lg shadow p-6">
        <h3 className="text-sm font-medium text-theme-text opacity-70 mb-2">Current Task</h3>
        <p className="text-lg font-semibold text-theme-text">
          {currentTask || 'No active task'}
        </p>
      </div>

      <div className="bg-theme-card rounded-lg shadow p-6">
        <h3 className="text-sm font-medium text-theme-text opacity-70 mb-2">Time Elapsed</h3>
        <div className="flex items-center gap-3">
          <p className="text-2xl font-bold text-gray-900 font-mono">
            {formatElapsedTime(elapsedTime)}
          </p>
          {status === 'processing' && (
            <svg className="w-5 h-5 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          )}
        </div>
      </div>
    </div>
  );
}
