interface AgentStatusBannerProps {
  task: string;
}

export function AgentStatusBanner({ task }: AgentStatusBannerProps) {
  return (
    <div className="mb-6 bg-theme-background border-2 border-theme-border rounded-lg p-5 shadow-sm">
      <div className="flex items-start gap-4">
        <div className="shrink-0 mt-1">
          <div className="relative">
            <div className="animate-spin h-6 w-6 border-3 border-theme-accent border-t-transparent rounded-full"></div>
            <div className="absolute inset-0 animate-pulse">
              <div className="h-6 w-6 border-3 border-theme-border border-t-transparent rounded-full"></div>
            </div>
          </div>
        </div>
        <div className="flex-1 min-w-0">
          <div className="flex items-center gap-2 mb-1">
            <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-theme-background text-">
              PROCESSING
            </span>
            <span className="text-xs text-theme-text font-mono">
              {new Date().toLocaleTimeString()}
            </span>
          </div>
          <p className="font-semibold text-theme-text text-lg leading-snug">{task}</p>
          <p className="text-sm text-theme-text mt-1">
            Please wait while the AI agent completes this operation...
          </p>
        </div>
      </div>
      <div className="mt-3 h-1.5 bg-theme-background rounded-full overflow-hidden">
        <div
          className="h-full bg-theme-accent rounded-full animate-pulse"
          style={{ width: '100%' }}
        ></div>
      </div>
    </div>
  );
}
