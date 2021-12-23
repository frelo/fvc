#region samples_make_playlist

using System.Runtime;
using System.Collections.Generic;
using VideoCataloger;

/// <summary>
///  This sample create a playlist from the first 10 secs of the selected videos
/// </summary>
public class Script
{
    /// <summary>
    ///  Run sample. This is the entry function called by fvc.
    /// </summary>
    static public async System.Threading.Tasks.Task Run(IScripting scripting, string argument)
    {
        scripting.GetConsole().Clear();
        ISelection selection = scripting.GetSelection();
        var catalog = scripting.GetVideoCatalogService();

        long summary_playlist = -1;
        string playlist_name = "Sumary";
        var all_playlists = catalog.GetAllVideoPlaylists( );
        if (all_playlists != null)
        {
            foreach (var playlist in all_playlists)
            {
                if (playlist.Name == playlist_name)
                {
                    int[] playlist_clips = catalog.GetPlaylistClipIDs(playlist.ID);
                    foreach (int video_clip in playlist_clips)
                    {
                        catalog.RemovePlaylistClip(playlist.ID, video_clip);
                        catalog.DeleteVideoClip(video_clip);
                    }
                    summary_playlist = playlist.ID;
                }
            }
        }

        if (summary_playlist == -1)
            summary_playlist = catalog.CreateVideoPlaylist(playlist_name);

        VideoCataloger.RemoteCatalogService.VideoClip clip = new VideoCataloger.RemoteCatalogService.VideoClip();
        clip.ID = -1; // -1 to create new clip
        clip.StartTime = 20;
        clip.EndTime = 25;
        int playlist_position = 0;
        List<long> selected = selection.GetSelectedVideos();
        if (selected != null)
        {
            foreach (long video in selected)
            {
                clip.VideoFileID = video;
                int new_clip_id = catalog.SetVideoClip(clip);
                catalog.SetClipToPlaylist(summary_playlist, playlist_position++, new_clip_id);
            }
        }
        scripting.GetGUI().Refresh("");
    }
}

#endregion
