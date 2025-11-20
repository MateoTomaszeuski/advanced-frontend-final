import { Button } from '../forms/Button';
import { SelectDropdown } from '../forms/SelectDropdown';

interface FilterPanelProps {
  filterType: string;
  setFilterType: (type: string) => void;
  filterStatus: string;
  setFilterStatus: (status: string) => void;
  onClearFilters: () => void;
}

export function FilterPanel({
  filterType,
  setFilterType,
  filterStatus,
  setFilterStatus,
  onClearFilters,
}: FilterPanelProps) {
  const actionTypeOptions = [
    { value: '', label: 'All Types' },
    { value: 'CreateSmartPlaylist', label: 'Smart Playlist' },
    { value: 'DiscoverNewMusic', label: 'Music Discovery' },
    { value: 'ScanDuplicates', label: 'Scan Duplicates' },
    { value: 'RemoveDuplicates', label: 'Remove Duplicates' },
    { value: 'SuggestMusicByContext', label: 'Music Suggestions' },
  ];

  const statusOptions = [
    { value: '', label: 'All Statuses' },
    { value: 'Processing', label: 'Processing' },
    { value: 'Completed', label: 'Completed' },
    { value: 'Failed', label: 'Failed' },
    { value: 'AwaitingApproval', label: 'Awaiting Approval' },
  ];

  return (
    <div className="bg-theme-card rounded-lg shadow mb-6 p-6">
      <h2 className="text-xl font-semibold text-theme-text mb-4">Filters</h2>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <SelectDropdown
          label="Action Type"
          value={filterType}
          onChange={(e) => setFilterType(e.target.value)}
          options={actionTypeOptions}
        />

        <SelectDropdown
          label="Status"
          value={filterStatus}
          onChange={(e) => setFilterStatus(e.target.value)}
          options={statusOptions}
        />
      </div>

      {(filterType || filterStatus) && (
        <div className="mt-4">
          <Button onClick={onClearFilters} variant="ghost">
            Clear Filters
          </Button>
        </div>
      )}
    </div>
  );
}
