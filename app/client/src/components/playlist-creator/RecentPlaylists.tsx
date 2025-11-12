interface RecentPlaylist {
  id: number;
  actionType: string;
  inputPrompt: string;
  result: {
    playlistId: string;
    playlistName: string;
    playlistUri: string;
    trackCount: number;
  };
  createdAt: string;
}

interface RecentPlaylistsProps {
  playlists: RecentPlaylist[];
  isLoading: boolean;
}

export function RecentPlaylists({ playlists, isLoading }: RecentPlaylistsProps) {
  if (playlists.length === 0) return null;

  return (
    <div className="mt-8">
      <div className="flex items-center justify-between mb-4">
        <h2 className="text-2xl font-bold text-gray-900">Recently Created Playlists</h2>
        {isLoading && (
          <div className="animate-spin h-5 w-5 border-2 border-green-600 border-t-transparent rounded-full"></div>
        )}
      </div>
      <div className="space-y-3">
        {playlists.map((playlist) => (
          <div
            key={playlist.id}
            className="bg-white rounded-lg shadow-sm border border-gray-200 p-4 hover:border-green-300 transition-colors"
          >
            <div className="flex items-start justify-between gap-4">
              <div className="flex-1 min-w-0">
                <h3 className="font-semibold text-gray-900 truncate">
                  {playlist.result.playlistName}
                </h3>
                <p className="text-sm text-gray-600 mt-1 line-clamp-2">
                  "{playlist.inputPrompt}"
                </p>
                <div className="flex items-center gap-4 mt-2 text-xs text-gray-500">
                  <span>{playlist.result.trackCount} tracks</span>
                  <span>•</span>
                  <span>{new Date(playlist.createdAt).toLocaleDateString()}</span>
                </div>
              </div>
              <a
                href={playlist.result.playlistUri}
                target="_blank"
                rel="noopener noreferrer"
                className="shrink-0 inline-flex items-center px-4 py-2 text-sm font-medium text-white bg-green-600 hover:bg-green-700 rounded-md transition-colors"
              >
                <svg className="w-4 h-4 mr-2" fill="currentColor" viewBox="0 0 24 24">
                  <path d="M12 0C5.4 0 0 5.4 0 12s5.4 12 12 12 12-5.4 12-12S18.66 0 12 0zm5.521 17.34c-.24.359-.66.48-1.021.24-2.82-1.74-6.36-2.101-10.561-1.141-.418.122-.779-.179-.899-.539-.12-.421.18-.78.54-.9 4.56-1.021 8.52-.6 11.64 1.32.42.18.479.659.301 1.02zm1.44-3.3c-.301.42-.841.6-1.262.3-3.239-1.98-8.159-2.58-11.939-1.38-.479.12-1.02-.12-1.14-.6-.12-.48.12-1.021.6-1.141C9.6 9.9 15 10.561 18.72 12.84c.361.181.54.78.241 1.2zm.12-3.36C15.24 8.4 8.82 8.16 5.16 9.301c-.6.179-1.2-.181-1.38-.721-.18-.601.18-1.2.72-1.381 4.26-1.26 11.28-1.02 15.721 1.621.539.3.719 1.02.419 1.56-.299.421-1.02.599-1.559.3z" />
                </svg>
                Open in Spotify
              </a>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
