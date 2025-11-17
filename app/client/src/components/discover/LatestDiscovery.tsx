import type { AgentActionResult } from '../../types/api';

interface LatestDiscoveryProps {
  discoveryResult: AgentActionResult;
}

export function LatestDiscovery({ discoveryResult }: LatestDiscoveryProps) {
  return (
    <div className="bg-theme-card rounded-lg shadow-sm border border-theme-border p-6">
      <h2 className="text-lg font-semibold text-theme-text mb-4">Latest Discovery</h2>
      
      <div className="mb-4">
        <div className="flex items-center justify-between mb-2">
          <h3 className="font-medium text-theme-text">
            {discoveryResult.playlistName}
          </h3>
          <span className="text-sm text-theme-text opacity-70">
            {discoveryResult.trackCount} tracks
          </span>
        </div>
        <a
          href={discoveryResult.playlistUri}
          target="_blank"
          rel="noopener noreferrer"
          className="text-sm text-green-600 hover:text-green-700 font-medium"
        >
          Open in Spotify →
        </a>
      </div>

      {discoveryResult.tracks && discoveryResult.tracks.length > 0 && (
        <div className="space-y-2">
          <h4 className="text-sm font-medium text-gray-700">Discovered Tracks:</h4>
          <div className="max-h-64 overflow-y-auto space-y-2">
            {discoveryResult.tracks.map((track, index) => {
              const trackData = track as Record<string, unknown>;
              const trackName = (trackData.name || trackData.Name || 'Unknown Track') as string;
              const trackArtists = (trackData.artists || trackData.Artists || 'Unknown Artist') as string;
              const trackId = (trackData.id || trackData.Id || index) as string | number;
              
              return (
                <div
                  key={trackId}
                  className="flex items-center gap-3 p-2 bg-gray-50 rounded-lg"
                >
                  <span className="text-sm text-gray-500 w-6">{index + 1}</span>
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-medium text-gray-900 truncate">
                      {trackName}
                    </p>
                    <p className="text-xs text-gray-600 truncate">
                      {trackArtists}
                    </p>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      )}
    </div>
  );
}
