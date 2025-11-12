interface AgentStatusBannerProps {
  task: string;
}

export function AgentStatusBanner({ task }: AgentStatusBannerProps) {
  return (
    <div className="mb-6 bg-green-50 border-2 border-green-300 rounded-lg p-5 shadow-sm">
      <div className="flex items-start gap-4">
        <div className="shrink-0 mt-1">
          <div className="relative">
            <div className="animate-spin h-6 w-6 border-3 border-green-600 border-t-transparent rounded-full"></div>
            <div className="absolute inset-0 animate-pulse">
              <div className="h-6 w-6 border-3 border-green-300 border-t-transparent rounded-full"></div>
            </div>
          </div>
        </div>
        <div className="flex-1 min-w-0">
          <div className="flex items-center gap-2 mb-1">
            <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-600 text-white">
              PROCESSING
            </span>
            <span className="text-xs text-green-600 font-mono">
              {new Date().toLocaleTimeString()}
            </span>
          </div>
          <p className="font-semibold text-green-900 text-lg leading-snug">{task}</p>
          <p className="text-sm text-green-700 mt-1">
            Please wait while the AI agent completes this operation...
          </p>
        </div>
      </div>
      <div className="mt-3 h-1.5 bg-green-200 rounded-full overflow-hidden">
        <div
          className="h-full bg-green-600 rounded-full animate-pulse"
          style={{ width: '100%' }}
        ></div>
      </div>
    </div>
  );
}
