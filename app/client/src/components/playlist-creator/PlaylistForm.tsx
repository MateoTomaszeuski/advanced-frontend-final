import { Button } from '../forms/Button';
import { TextInput } from '../forms/TextInput';
import { SelectDropdown } from '../forms/SelectDropdown';

interface PlaylistFormProps {
  prompt: string;
  setPrompt: (value: string) => void;
  maxTracks: string;
  setMaxTracks: (value: string) => void;
  isLoading: boolean;
  onSubmit: (e: React.FormEvent) => void;
  onClear: () => void;
}

export function PlaylistForm({
  prompt,
  setPrompt,
  maxTracks,
  setMaxTracks,
  isLoading,
  onSubmit,
  onClear,
}: PlaylistFormProps) {
  return (
    <div className="bg-theme-card rounded-lg shadow-sm border border-theme-border p-6">
      <form onSubmit={onSubmit} className="space-y-6">
        <div>
          <label className="block text-sm font-medium text-theme-text mb-2">
            Playlist Description *
          </label>
          <TextInput
            value={prompt}
            onChange={(e) => setPrompt(e.target.value)}
            placeholder="e.g., Create a workout playlist with high-energy rock songs"
            disabled={isLoading}
          />
          <p className="mt-2 text-sm text-theme-text opacity-70">
            Examples: "chill vibes for studying", "upbeat party mix", "focus music for work"
          </p>
        </div>

        <div>
          <label className="block text-sm font-medium text-theme-text mb-2">Number of Tracks</label>
          <SelectDropdown
            value={maxTracks}
            onChange={(e) => setMaxTracks(e.target.value)}
            options={[
              { value: '10', label: '10 tracks' },
              { value: '20', label: '20 tracks' },
              { value: '30', label: '30 tracks' },
              { value: '50', label: '50 tracks' },
              { value: '75', label: '75 tracks' },
              { value: '100', label: '100 tracks' },
              { value: '150', label: '150 tracks' },
              { value: '200', label: '200 tracks' },
              { value: '250', label: '250 tracks' },
            ]}
            disabled={isLoading}
          />
        </div>

        <div className="flex gap-3 pt-4">
          <Button
            type="submit"
            variant="primary"
            disabled={isLoading || !prompt.trim()}
            isLoading={isLoading}
          >
            Create Playlist
          </Button>
          <Button type="button" variant="secondary" onClick={onClear} disabled={isLoading}>
            Clear
          </Button>
        </div>
      </form>
    </div>
  );
}
