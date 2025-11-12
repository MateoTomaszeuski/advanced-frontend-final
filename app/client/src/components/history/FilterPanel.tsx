import { Button } from '../forms/Button';

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
  return (
    <div className="bg-white rounded-lg shadow mb-6 p-6">
      <h3 className="text-sm font-medium text-gray-700 mb-4">Filters</h3>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Action Type
          </label>
          <select
            value={filterType}
            onChange={(e) => setFilterType(e.target.value)}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-green-500 focus:border-green-500"
          >
            <option value="">All Types</option>
            <option value="CreateSmartPlaylist">Smart Playlist</option>
            <option value="DiscoverNewMusic">Music Discovery</option>
            <option value="ScanDuplicates">Scan Duplicates</option>
            <option value="RemoveDuplicates">Remove Duplicates</option>
            <option value="SuggestMusicByContext">Music Suggestions</option>
          </select>
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Status
          </label>
          <select
            value={filterStatus}
            onChange={(e) => setFilterStatus(e.target.value)}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-green-500 focus:border-green-500"
          >
            <option value="">All Statuses</option>
            <option value="Processing">Processing</option>
            <option value="Completed">Completed</option>
            <option value="Failed">Failed</option>
            <option value="AwaitingApproval">Awaiting Approval</option>
          </select>
        </div>
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
