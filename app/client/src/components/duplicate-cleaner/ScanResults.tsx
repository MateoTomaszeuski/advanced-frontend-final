import { Button } from '../forms/Button';
import { DuplicateGroupCard } from './DuplicateGroupCard';
import type { RemoveDuplicatesResponse } from '../../types/api';

interface ScanResultsProps {
  scanResult: RemoveDuplicatesResponse;
  selectedToRemove: Set<string>;
  onSelectRecommended: () => void;
  onRemove: () => void;
  onToggle: (uri: string) => void;
  isLoading: boolean;
}

export function ScanResults({
  scanResult,
  selectedToRemove,
  onSelectRecommended,
  onRemove,
  onToggle,
  isLoading,
}: ScanResultsProps) {
  return (
    <div className="bg-theme-card rounded-lg shadow-sm p-6 mb-6">
      <div className="flex items-center justify-between mb-4">
        <h2 className="text-xl font-semibold text-gray-900">Scan Results</h2>
        <div className="text-sm text-gray-600">
          {scanResult.totalDuplicateGroups === 0 ? (
            <span className="text-green-600 font-medium">✓ No duplicates found</span>
          ) : (
            <span>
              Found <span className="font-semibold text-red-600">{scanResult.totalDuplicateTracks}</span>{' '}
              duplicates in {scanResult.totalDuplicateGroups} groups
            </span>
          )}
        </div>
      </div>

      {scanResult.totalDuplicateGroups > 0 && (
        <>
          <div className="flex gap-2 mb-4">
            <Button variant="secondary" onClick={onSelectRecommended} size="sm">
              Select Recommended
            </Button>
            <Button
              variant="danger"
              onClick={onRemove}
              disabled={selectedToRemove.size === 0 || isLoading}
              isLoading={isLoading}
              size="sm"
            >
              Remove Selected ({selectedToRemove.size})
            </Button>
          </div>

          <div className="space-y-6">
            {scanResult.duplicateGroups.map((group, idx) => (
              <DuplicateGroupCard
                key={idx}
                group={group}
                selectedToRemove={selectedToRemove}
                onToggle={onToggle}
              />
            ))}
          </div>
        </>
      )}
    </div>
  );
}
