import { useState, useEffect, useRef } from 'react';
import { MainLayout } from '../components/layout/MainLayout';
import { SpotifyConnectionAlert } from '../components/SpotifyConnectionAlert';
import { SelectDropdown } from '../components/forms/SelectDropdown';
import { Button } from '../components/forms/Button';
import { useAgent } from '../hooks/useAgent';
import { spotifyApi } from '../services/api';
import type { SpotifyPlaylist, DuplicateGroup, RemoveDuplicatesResponse } from '../types/api';

export function DuplicateCleanerPage() {
  const [playlists, setPlaylists] = useState<SpotifyPlaylist[]>([]);
  const [selectedPlaylist, setSelectedPlaylist] = useState('');
  const [scanResult, setScanResult] = useState<RemoveDuplicatesResponse | null>(null);
  const [selectedToRemove, setSelectedToRemove] = useState<Set<string>>(new Set());
  const [conversationId, setConversationId] = useState<number | null>(null);
  const conversationCreated = useRef(false);

  const { isLoading, createConversation, scanDuplicates, confirmRemoveDuplicates } = useAgent();

  const fetchPlaylists = async () => {
    try {
      const data = await spotifyApi.getPlaylists() as SpotifyPlaylist[];
      console.log(`Fetched ${data.length} playlists from API`);
      setPlaylists(data);
    } catch (error) {
      console.error('Failed to fetch playlists:', error);
    }
  };

  useEffect(() => {
    let mounted = true;

    const init = async () => {
      if (conversationCreated.current) return;
      conversationCreated.current = true;

      try {
        const conversation = await createConversation('Duplicate Cleaner Session');
        if (mounted) {
          setConversationId(conversation.id);
        }
      } catch (error) {
        console.error('Failed to create conversation:', error);
        conversationCreated.current = false;
      }

      try {
        const data = await spotifyApi.getPlaylists() as SpotifyPlaylist[];
        if (mounted) {
          setPlaylists(data);
        }
      } catch (error) {
        console.error('Failed to fetch playlists:', error);
      }
    };

    init();

    return () => {
      mounted = false;
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleScan = async () => {
    if (!selectedPlaylist || !conversationId) return;

    try {
      const result = await scanDuplicates(conversationId, selectedPlaylist);
      setScanResult(result);
      setSelectedToRemove(new Set());
    } catch (error) {
      console.error('Scan failed:', error);
    }
  };

  const toggleSelection = (trackUri: string) => {
    const newSet = new Set(selectedToRemove);
    if (newSet.has(trackUri)) {
      newSet.delete(trackUri);
    } else {
      newSet.add(trackUri);
    }
    setSelectedToRemove(newSet);
  };

  const selectRecommended = () => {
    if (!scanResult) return;

    const recommended = new Set<string>();
    scanResult.duplicateGroups.forEach((group) => {
      group.duplicates.forEach((track) => {
        if (!track.isRecommendedToKeep) {
          recommended.add(track.uri);
        }
      });
    });
    setSelectedToRemove(recommended);
  };

  const handleRemove = async () => {
    if (!conversationId || !selectedPlaylist || selectedToRemove.size === 0) return;

    try {
      await confirmRemoveDuplicates(
        conversationId,
        selectedPlaylist,
        Array.from(selectedToRemove)
      );

      setScanResult(null);
      setSelectedToRemove(new Set());
    } catch (error) {
      console.error('Remove failed:', error);
    }
  };

  const playlistOptions = playlists.map((p) => ({
    value: p.id,
    label: `${p.name} (${p.totalTracks} tracks)`,
  }));

  console.log(`Rendering ${playlists.length} playlists as ${playlistOptions.length} options`);

  return (
    <MainLayout>
      <div className="max-w-4xl mx-auto">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">Duplicate Cleaner</h1>
        <p className="text-gray-600 mb-6">
          Scan your playlists for duplicate tracks and clean them up with AI assistance.
        </p>

        <SpotifyConnectionAlert />

        <div className="bg-white rounded-lg shadow-sm p-6 mb-6">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-xl font-semibold text-gray-900">Select Playlist</h2>
            <Button
              variant="ghost"
              size="sm"
              onClick={fetchPlaylists}
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
              onClick={handleScan}
              disabled={!selectedPlaylist || !conversationId || isLoading}
              isLoading={isLoading}
              className="w-full"
            >
              {isLoading ? 'Scanning...' : 'Scan for Duplicates'}
            </Button>
          </div>
        </div>

        {scanResult && (
          <>
            <div className="bg-white rounded-lg shadow-sm p-6 mb-6">
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
                    <Button variant="secondary" onClick={selectRecommended} size="sm">
                      Select Recommended
                    </Button>
                    <Button
                      variant="danger"
                      onClick={handleRemove}
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
                        onToggle={toggleSelection}
                      />
                    ))}
                  </div>
                </>
              )}
            </div>
          </>
        )}

        <div className="bg-blue-50 border-l-4 border-blue-400 p-4 rounded">
          <div className="flex items-start">
            <div className="shrink-0">
              <svg className="h-5 w-5 text-blue-400" viewBox="0 0 20 20" fill="currentColor">
                <path
                  fillRule="evenodd"
                  d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z"
                  clipRule="evenodd"
                />
              </svg>
            </div>
            <div className="ml-3">
              <h3 className="text-sm font-medium text-blue-800">How it works</h3>
              <div className="mt-2 text-sm text-blue-700">
                <ul className="list-disc list-inside space-y-1">
                  <li>AI analyzes track names and artist combinations to find duplicates</li>
                  <li>The most popular version is recommended to keep</li>
                  <li>Review selections before removing to ensure accuracy</li>
                  <li>Removed tracks are permanently deleted from the playlist</li>
                </ul>
              </div>
            </div>
          </div>
        </div>
      </div>
    </MainLayout>
  );
}

function DuplicateGroupCard({
  group,
  selectedToRemove,
  onToggle,
}: {
  group: DuplicateGroup;
  selectedToRemove: Set<string>;
  onToggle: (uri: string) => void;
}) {
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
