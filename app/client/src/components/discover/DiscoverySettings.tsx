import { SelectDropdown } from '../forms/SelectDropdown';
import { Button } from '../forms/Button';

interface DiscoverySettingsProps {
  limit: string;
  setLimit: (limit: string) => void;
  isLoading: boolean;
  onDiscover: () => void;
}

export function DiscoverySettings({
  limit,
  setLimit,
  isLoading,
  onDiscover,
}: DiscoverySettingsProps) {
  return (
    <div className="bg-theme-card rounded-lg shadow-sm border border-theme-border p-6 mb-6">
      <h3 className="text-lg font-semibold text-gray-900 mb-4">Discovery Settings</h3>
      
      <div className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Number of Tracks to Discover
          </label>
          <SelectDropdown
            value={limit}
            onChange={(e) => setLimit(e.target.value)}
            options={[
              { value: '5', label: '5 tracks' },
              { value: '10', label: '10 tracks' },
              { value: '15', label: '15 tracks' },
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

        <Button
          variant="primary"
          onClick={onDiscover}
          disabled={isLoading}
          isLoading={isLoading}
          className="w-full mt-4"
        >
          Discover New Music
        </Button>
      </div>
    </div>
  );
}
