import { Button } from '../forms/Button';
import type { SuggestMusicResponse } from '../../types/api';

interface SuggestionResultsProps {
  suggestions: SuggestMusicResponse;
  selectedTracks: Set<string>;
  onToggleTrack: (uri: string) => void;
  onSelectAll: () => void;
  onDeselectAll: () => void;
  onAddToPlaylist: () => void;
  isAdding: boolean;
}

export function SuggestionResults({
  suggestions,
  selectedTracks,
  onToggleTrack,
  onSelectAll,
  onDeselectAll,
  onAddToPlaylist,
  isAdding,
}: SuggestionResultsProps) {
  if (suggestions.suggestionCount === 0) {
    return (
      <div className="bg-yellow-50 border-l-4 border-yellow-400 p-4 rounded">
        <p className="text-yellow-800">
          No suggestions found for this context. Try a different description or playlist.
        </p>
      </div>
    );
  }

  return (
    <div className="bg-theme-card rounded-lg shadow-sm p-6 mb-6">
      <div className="flex items-center justify-between mb-4">
        <h2 className="text-xl font-semibold text-theme-text">
          Suggestions for "{suggestions.playlistName}"
        </h2>
        <div className="flex items-center gap-2">
          {selectedTracks.size > 0 && (
            <Button
              onClick={onAddToPlaylist}
              disabled={isAdding}
              isLoading={isAdding}
              variant="primary"
              size="sm"
            >
              Add {selectedTracks.size} to Playlist
            </Button>
          )}
        </div>
      </div>
      <div className="flex items-center justify-between mb-4">
        <p className="text-sm text-theme-text opacity-70">
          Context: <span className="italic">{suggestions.context}</span>
        </p>
        <div className="flex items-center gap-2">
          {selectedTracks.size === suggestions.suggestions.length ? (
            <Button
              onClick={onDeselectAll}
              variant="ghost"
              size="sm"
            >
              Deselect All
            </Button>
          ) : (
            <Button
              onClick={onSelectAll}
              variant="ghost"
              size="sm"
            >
              Select All
            </Button>
          )}
        </div>
      </div>

      <div className="space-y-3">
        {suggestions.suggestions.map((track) => (
          <div
            key={track.id}
            className="flex items-start gap-4 p-4 bg-theme-card rounded-lg border border-theme-border hover:border-theme-accent transition-colors"
          >
            <input
              type="checkbox"
              checked={selectedTracks.has(track.uri)}
              onChange={() => onToggleTrack(track.uri)}
              className="mt-1 h-5 w-5 text-theme-text border-theme-border rounded focus:ring-theme-accent"
            />
            <div className="flex-1 min-w-0">
              <h3 className="font-semibold text-theme-text truncate">{track.name}</h3>
              <p className="text-sm text-theme-text opacity-70 truncate">
                {track.artists.join(', ')}
              </p>
              <p className="text-xs text-theme-text opacity-60 mt-1 italic">{track.reason}</p>
            </div>

            <div className="flex items-center gap-2 shrink-0">
              <span className="text-xs text-theme-text opacity-60">
                ♪ {track.popularity}
              </span>
              <a
                href={track.uri}
                target="_blank"
                rel="noopener noreferrer"
                className="inline-flex items-center px-3 py-1.5 text-sm font-medium text-theme-text bg-theme-background hover:bg-theme-accent rounded-md transition-colors"
              >
                <svg
                  className="w-4 h-4 mr-1"
                  fill="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path d="M12 0C5.4 0 0 5.4 0 12s5.4 12 12 12 12-5.4 12-12S18.66 0 12 0zm5.521 17.34c-.24.359-.66.48-1.021.24-2.82-1.74-6.36-2.101-10.561-1.141-.418.122-.779-.179-.899-.539-.12-.421.18-.78.54-.9 4.56-1.021 8.52-.6 11.64 1.32.42.18.479.659.301 1.02zm1.44-3.3c-.301.42-.841.6-1.262.3-3.239-1.98-8.159-2.58-11.939-1.38-.479.12-1.02-.12-1.14-.6-.12-.48.12-1.021.6-1.141C9.6 9.9 15 10.561 18.72 12.84c.361.181.54.78.241 1.2zm.12-3.36C15.24 8.4 8.82 8.16 5.16 9.301c-.6.179-1.2-.181-1.38-.721-.18-.601.18-1.2.72-1.381 4.26-1.26 11.28-1.02 15.721 1.621.539.3.719 1.02.419 1.56-.299.421-1.02.599-1.559.3z" />
                </svg>
                Play
              </a>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
