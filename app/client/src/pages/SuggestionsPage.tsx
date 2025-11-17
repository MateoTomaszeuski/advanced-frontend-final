import { useState, useEffect, useRef } from 'react';
import { MainLayout } from '../components/layout/MainLayout';
import { SpotifyConnectionAlert } from '../components/SpotifyConnectionAlert';
import { InfoBox } from '../components/InfoBox';
import { SuggestionSettings } from '../components/suggestions/SuggestionSettings';
import { SuggestionResults } from '../components/suggestions/SuggestionResults';
import { useAgent } from '../hooks/useAgent';
import { spotifyApi } from '../services/api';
import { showToast } from '../utils/toast';
import type { SpotifyPlaylist, SuggestMusicResponse } from '../types/api';

export function SuggestionsPage() {
  const [playlists, setPlaylists] = useState<SpotifyPlaylist[]>([]);
  const [selectedPlaylist, setSelectedPlaylist] = useState('');
  const [context, setContext] = useState('');
  const [limit, setLimit] = useState('10');
  const [suggestions, setSuggestions] = useState<SuggestMusicResponse | null>(null);
  const [conversationId, setConversationId] = useState<number | null>(null);
  const [selectedTracks, setSelectedTracks] = useState<Set<string>>(new Set());
  const [isAdding, setIsAdding] = useState(false);
  const conversationCreated = useRef(false);

  const { isLoading, createConversation, suggestMusic } = useAgent();

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
        const conversation = await createConversation('Music Suggestions Session');
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

  const handleGenerate = async () => {
    if (!selectedPlaylist || !context || !conversationId) return;

    try {
      const result = await suggestMusic(conversationId, selectedPlaylist, context, parseInt(limit));
      setSuggestions(result);
      setSelectedTracks(new Set());
    } catch (error) {
      console.error('Generate failed:', error);
    }
  };

  const toggleTrackSelection = (trackUri: string) => {
    setSelectedTracks(prev => {
      const newSet = new Set(prev);
      if (newSet.has(trackUri)) {
        newSet.delete(trackUri);
      } else {
        newSet.add(trackUri);
      }
      return newSet;
    });
  };

  const handleAddToPlaylist = async () => {
    if (!selectedPlaylist || selectedTracks.size === 0) return;

    setIsAdding(true);
    try {
      await spotifyApi.addTracksToPlaylist(selectedPlaylist, Array.from(selectedTracks));
      showToast.success(`Added ${selectedTracks.size} tracks to playlist!`);
      setSelectedTracks(new Set());
    } catch (error) {
      console.error('Failed to add tracks:', error);
      showToast.error('Failed to add tracks to playlist');
    } finally {
      setIsAdding(false);
    }
  };

  const handleSelectAll = () => {
    if (!suggestions) return;
    
    const allTrackUris = suggestions.suggestions.map(track => track.uri);
    setSelectedTracks(new Set(allTrackUris));
  };

  const handleDeselectAll = () => {
    setSelectedTracks(new Set());
  };

  return (
    <MainLayout>
      <div className="max-w-4xl mx-auto">
        <h1 className="text-3xl font-bold text-theme-text mb-2">Music Suggestions</h1>
        <p className="text-theme-text opacity-80 mb-6">
          Get AI-powered music recommendations based on your playlist and context.
        </p>

        <div className="mb-6">
          <InfoBox
            type="info"
            items={[
              'AI analyzes your playlist\'s style and characteristics',
              'Generates suggestions based on your context description',
              'Filters out tracks already in the playlist',
              'Click "Play" to open tracks in Spotify',
            ]}
          />
        </div>

        <SpotifyConnectionAlert />

        <SuggestionSettings
          playlists={playlists}
          selectedPlaylist={selectedPlaylist}
          setSelectedPlaylist={setSelectedPlaylist}
          context={context}
          setContext={setContext}
          limit={limit}
          setLimit={setLimit}
          onGenerate={handleGenerate}
          onSync={fetchPlaylists}
          isLoading={isLoading}
          conversationId={conversationId}
        />

        {suggestions && (
          <SuggestionResults
            suggestions={suggestions}
            selectedTracks={selectedTracks}
            onToggleTrack={toggleTrackSelection}
            onSelectAll={handleSelectAll}
            onDeselectAll={handleDeselectAll}
            onAddToPlaylist={handleAddToPlaylist}
            isAdding={isAdding}
          />
        )}
      </div>
    </MainLayout>
  );
}
