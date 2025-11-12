import { useState, useEffect, useRef } from 'react';
import { MainLayout } from '../components/layout/MainLayout';
import { SpotifyConnectionAlert } from '../components/SpotifyConnectionAlert';
import { InfoBox } from '../components/InfoBox';
import { PlaylistSelector } from '../components/duplicate-cleaner/PlaylistSelector';
import { ScanResults } from '../components/duplicate-cleaner/ScanResults';
import { useAgent } from '../hooks/useAgent';
import { spotifyApi } from '../services/api';
import type { SpotifyPlaylist, RemoveDuplicatesResponse } from '../types/api';

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

  return (
    <MainLayout>
      <div className="max-w-4xl mx-auto">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">Duplicate Cleaner</h1>
        <p className="text-gray-600 mb-6">
          Scan your playlists for duplicate tracks and clean them up with AI assistance.
        </p>

        <div className="mb-6">
          <InfoBox
            type="info"
            items={[
              'AI analyzes track names and artist combinations to find duplicates',
              'The most popular version is recommended to keep',
              'Review selections before removing to ensure accuracy',
              'Removed tracks are permanently deleted from the playlist',
            ]}
          />
        </div>

        <SpotifyConnectionAlert />

        <PlaylistSelector
          playlists={playlists}
          selectedPlaylist={selectedPlaylist}
          setSelectedPlaylist={setSelectedPlaylist}
          onScan={handleScan}
          onSync={fetchPlaylists}
          isLoading={isLoading}
          conversationId={conversationId}
        />

        {scanResult && (
          <ScanResults
            scanResult={scanResult}
            selectedToRemove={selectedToRemove}
            onSelectRecommended={selectRecommended}
            onRemove={handleRemove}
            onToggle={toggleSelection}
            isLoading={isLoading}
          />
        )}
      </div>
    </MainLayout>
  );
}
