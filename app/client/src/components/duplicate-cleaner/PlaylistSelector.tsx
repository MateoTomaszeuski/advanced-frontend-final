import { SelectDropdown } from '../forms/SelectDropdown';
import { Button } from '../forms/Button';
import type { SpotifyPlaylist } from '../../types/api';

interface PlaylistSelectorProps {
  playlists: SpotifyPlaylist[];
  selectedPlaylist: string;
  setSelectedPlaylist: (id: string) => void;
  onScan: () => void;
  onSync: () => void;
  isLoading: boolean;
  conversationId: number | null;
}

export function PlaylistSelector({
  playlists,
  selectedPlaylist,
  setSelectedPlaylist,
  onScan,
  onSync,
  isLoading,
  conversationId,
}: PlaylistSelectorProps) {
  const playlistOptions = playlists.map((p) => ({
    value: p.id,
    label: `${p.name} (${p.totalTracks} tracks)`,
  }));

  return (
    <div className="bg-theme-card rounded-lg shadow-sm p-6 mb-6">
      <div className="flex items-center justify-between mb-4">
        <h2 className="text-xl font-semibold text-gray-900">Select Playlist</h2>
        <Button
          variant="ghost"
          size="sm"
          onClick={onSync}
          leftIcon={
            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
            </svg>
          }
        >
          Sync with Spotify
        </Button>
      </div>

      <div className="space-y-4">
        <SelectDropdown
          label="Playlist"
          value={selectedPlaylist}
          onChange={(e) => setSelectedPlaylist(e.target.value)}
          options={playlistOptions}
          placeholder="Choose a playlist to scan"
        />

        <Button
          onClick={onScan}
          disabled={!selectedPlaylist || !conversationId || isLoading}
          isLoading={isLoading}
          className="w-full"
        >
          {isLoading ? 'Scanning...' : 'Scan for Duplicates'}
        </Button>
      </div>
    </div>
  );
}
