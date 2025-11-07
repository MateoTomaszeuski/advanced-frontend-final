import { useState, useEffect } from 'react';
import { MainLayout } from '../components/layout/MainLayout';
import { SpotifyConnectionAlert } from '../components/SpotifyConnectionAlert';
import { SelectDropdown } from '../components/forms/SelectDropdown';
import { TextInput } from '../components/forms/TextInput';
import { Button } from '../components/forms/Button';
import { useAgent } from '../hooks/useAgent';
import { spotifyApi } from '../services/api';
import type { SpotifyPlaylist, SuggestMusicResponse } from '../types/api';

export function SuggestionsPage() {
  const [playlists, setPlaylists] = useState<SpotifyPlaylist[]>([]);
  const [selectedPlaylist, setSelectedPlaylist] = useState('');
  const [context, setContext] = useState('');
  const [suggestions, setSuggestions] = useState<SuggestMusicResponse | null>(null);
  const [conversationId, setConversationId] = useState<number | null>(null);

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
      try {
        const conversation = await createConversation('Music Suggestions Session');
        if (mounted) {
          setConversationId(conversation.id);
        }
      } catch (error) {
        console.error('Failed to create conversation:', error);
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
      const result = await suggestMusic(conversationId, selectedPlaylist, context);
      setSuggestions(result);
    } catch (error) {
      console.error('Generate failed:', error);
    }
  };

  const playlistOptions = playlists.map((p) => ({
    value: p.id,
    label: `${p.name} (${p.totalTracks} tracks)`,
  }));

  const contextExamples = [
    'more upbeat and energetic',
    'similar but more chill',
    'different artists with same vibe',
    'newer releases in the same genre',
    'deeper cuts and b-sides',
  ];

  return (
    <MainLayout>
      <div className="max-w-4xl mx-auto">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">Music Suggestions</h1>
        <p className="text-gray-600 mb-6">
          Get AI-powered music recommendations based on your playlist and context.
        </p>

        <SpotifyConnectionAlert />

        <div className="bg-white rounded-lg shadow-sm p-6 mb-6">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-xl font-semibold text-gray-900">Configure Suggestions</h2>
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
              placeholder="Choose a playlist"
            />

            <TextInput
              label="Context / Description"
              value={context}
              onChange={(e) => setContext(e.target.value)}
              placeholder="e.g., more upbeat and energetic"
              helperText="Describe what kind of music you're looking for"
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
              onClick={handleGenerate}
              disabled={!selectedPlaylist || !context || !conversationId || isLoading}
              isLoading={isLoading}
              className="w-full"
            >
              {isLoading ? 'Generating...' : 'Generate Suggestions'}
            </Button>
          </div>
        </div>

        {suggestions && suggestions.suggestionCount > 0 && (
          <div className="bg-white rounded-lg shadow-sm p-6 mb-6">
            <h2 className="text-xl font-semibold text-gray-900 mb-2">
              Suggestions for "{suggestions.playlistName}"
            </h2>
            <p className="text-sm text-gray-600 mb-4">
              Context: <span className="italic">{suggestions.context}</span>
            </p>

            <div className="space-y-3">
              {suggestions.suggestions.map((track) => (
                <div
                  key={track.id}
                  className="flex items-start gap-4 p-4 bg-gray-50 rounded-lg border border-gray-200 hover:border-green-300 transition-colors"
                >
                  <div className="flex-1 min-w-0">
                    <h3 className="font-semibold text-gray-900 truncate">{track.name}</h3>
                    <p className="text-sm text-gray-600 truncate">
                      {track.artists.join(', ')}
                    </p>
                    <p className="text-xs text-gray-500 mt-1 italic">{track.reason}</p>
                  </div>

                  <div className="flex items-center gap-2 shrink-0">
                    <span className="text-xs text-gray-500">
                      ♪ {track.popularity}
                    </span>
                    <a
                      href={track.uri}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="inline-flex items-center px-3 py-1.5 text-sm font-medium text-green-700 bg-green-50 hover:bg-green-100 rounded-md transition-colors"
                    >
                      <svg
                        className="w-4 h-4 mr-1"
                        fill="currentColor"
                        viewBox="0 0 24 24"
                      >
                        <path d="M12 0C5.4 0 0 5.4 0 12s5.4 12 12 12 12-5.4 12-12S18.66 0 12 0zm5.521 17.34c-.24.359-.66.48-1.021.24-2.82-1.74-6.36-2.101-10.561-1.141-.418.122-.779-.179-.899-.539-.12-.421.18-.78.54-.9 4.56-1.021 8.52-.6 11.64 1.32.42.18.479.659.301 1.02zm1.44-3.3c-.301.42-.841.6-1.262.3-3.239-1.98-8.159-2.58-11.939-1.38-.479.12-1.02-.12-1.14-.6-.12-.48.12-1.021.6-1.141C9.6 9.9 15 10.561 18.72 12.84c.361.181.54.78.241 1.2zm.12-3.36C15.24 8.4 8.82 8.16 5.16 9.301c-.6.179-1.2-.181-1.38-.721-.18-.601.18-1.2.72-1.381 4.26-1.26 11.28-1.02 15.721 1.621.539.3.719 1.02.419 1.56-.299.421-1.02.599-1.559.3z" />
                      </svg>
                      Play
                    </a>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}

        {suggestions && suggestions.suggestionCount === 0 && (
          <div className="bg-yellow-50 border-l-4 border-yellow-400 p-4 rounded">
            <p className="text-yellow-800">
              No suggestions found for this context. Try a different description or playlist.
            </p>
          </div>
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
                  <li>AI analyzes your playlist's style and characteristics</li>
                  <li>Generates suggestions based on your context description</li>
                  <li>Filters out tracks already in the playlist</li>
                  <li>Click "Play" to open tracks in Spotify</li>
                </ul>
              </div>
            </div>
          </div>
        </div>
      </div>
    </MainLayout>
  );
}
