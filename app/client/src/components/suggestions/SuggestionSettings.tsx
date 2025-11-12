import { SelectDropdown } from '../forms/SelectDropdown';
import { TextInput } from '../forms/TextInput';
import { Button } from '../forms/Button';
import type { SpotifyPlaylist } from '../../types/api';

interface SuggestionSettingsProps {
  playlists: SpotifyPlaylist[];
  selectedPlaylist: string;
  setSelectedPlaylist: (id: string) => void;
  context: string;
  setContext: (context: string) => void;
  limit: string;
  setLimit: (limit: string) => void;
  onGenerate: () => void;
  onSync: () => void;
  isLoading: boolean;
  conversationId: number | null;
}

const contextExamples = [
  'more upbeat and energetic',
  'similar but more chill',
  'different artists with same vibe',
  'newer releases in the same genre',
  'deeper cuts and b-sides',
];

export function SuggestionSettings({
  playlists,
  selectedPlaylist,
  setSelectedPlaylist,
  context,
  setContext,
  limit,
  setLimit,
  onGenerate,
  onSync,
  isLoading,
  conversationId,
}: SuggestionSettingsProps) {
  const playlistOptions = playlists.map((p) => ({
    value: p.id,
    label: `${p.name} (${p.totalTracks} tracks)`,
  }));

  return (
    <div className="bg-white rounded-lg shadow-sm p-6 mb-6">
      <div className="flex items-center justify-between mb-4">
        <h2 className="text-xl font-semibold text-gray-900">Configure Suggestions</h2>
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
          placeholder="Choose a playlist"
        />

        <TextInput
          label="Context / Description"
          value={context}
          onChange={(e) => setContext(e.target.value)}
          placeholder="e.g., more upbeat and energetic"
          helperText="Describe what kind of music you're looking for"
        />

        <SelectDropdown
          label="Number of Suggestions"
          value={limit}
          onChange={(e) => setLimit(e.target.value)}
          options={[
            { value: '5', label: '5 suggestions' },
            { value: '10', label: '10 suggestions' },
            { value: '15', label: '15 suggestions' },
            { value: '20', label: '20 suggestions' },
            { value: '30', label: '30 suggestions' },
            { value: '50', label: '50 suggestions' },
          ]}
        />

        <div className="flex flex-wrap gap-2">
          {contextExamples.map((example) => (
            <button
              key={example}
              onClick={() => setContext(example)}
              className="px-3 py-1 text-sm bg-gray-100 hover:bg-gray-200 rounded-full text-gray-700 transition-colors"
            >
              {example}
            </button>
          ))}
        </div>

        <Button
          onClick={onGenerate}
          disabled={!selectedPlaylist || !context || !conversationId || isLoading}
          isLoading={isLoading}
          className="w-full"
        >
          {isLoading ? 'Generating...' : 'Generate Suggestions'}
        </Button>
      </div>
    </div>
  );
}
