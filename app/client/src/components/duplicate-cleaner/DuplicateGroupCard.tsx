import type { DuplicateGroup } from '../../types/api';

interface DuplicateGroupCardProps {
  group: DuplicateGroup;
  selectedToRemove: Set<string>;
  onToggle: (uri: string) => void;
}

export function DuplicateGroupCard({
  group,
  selectedToRemove,
  onToggle,
}: DuplicateGroupCardProps) {
  return (
    <div className="border border-gray-200 rounded-lg p-4 bg-gray-50">
      <div className="mb-3">
        <div className="flex items-center gap-2 mb-1">
          <span className="text-xs font-medium text-gray-500 uppercase tracking-wide">Song:</span>
          <h3 className="font-semibold text-gray-900">{group.trackName}</h3>
        </div>
        <p className="text-sm text-gray-600">by {group.artists.join(', ')}</p>
        <p className="text-xs text-gray-500 mt-1">Found in {group.duplicates.length} albums:</p>
      </div>

      <div className="space-y-2">
        {group.duplicates.map((track) => (
          <div
            key={track.uri}
            className={`flex items-start gap-3 p-3 rounded border ${
              track.isRecommendedToKeep
                ? 'bg-green-50 border-green-200'
                : 'bg-white border-gray-200'
            }`}
          >
            <input
              type="checkbox"
              checked={selectedToRemove.has(track.uri)}
              onChange={() => onToggle(track.uri)}
              disabled={track.isRecommendedToKeep}
              className="mt-1 h-4 w-4 text-green-600 rounded border-gray-300 focus:ring-green-500"
            />

            <div className="flex-1 min-w-0">
              <div className="flex items-center gap-2">
                <p className="text-sm font-medium text-gray-900">{track.albumName}</p>
                {track.isRecommendedToKeep && (
                  <span className="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-green-100 text-green-800">
                    Recommended
                  </span>
                )}
              </div>
              <div className="flex items-center gap-4 mt-1 text-xs text-gray-500">
                <span>Popularity: {track.popularity}</span>
                {track.releaseDate && (
                  <span>
                    Added: {new Date(track.releaseDate).toLocaleDateString()}
                  </span>
                )}
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
