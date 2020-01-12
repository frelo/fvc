#region samples_selection_genre

using System.Runtime;
using System.Collections.Generic;
using VideoCataloger;

/// <summary>
///  This sample sets the genre of all selected video. 
/// </summary>
public class SelectionGenre
{
    /// <summary>
    ///  Run sample. This is the entry function called by fvc.
    /// </summary>
    static public void Run(IScripting scripting, string argument)
    {
        if (argument == "")
        {
            scripting.GetConsole().WriteLine("Enter a genre string as argument");
            return;
        }
        string genre_to_set_to = argument;

        scripting.GetConsole().WriteLine("Setting genre for selected videos to " + genre_to_set_to);
        ISelection selection = scripting.GetSelection();
        var catalog = scripting.GetVideoCatalogService();
        List<long> selected = selection.GetSelectedVideos();
        foreach (long video in selected)
        {
            // set the Genre to ActionRating property to 1 for each of the selected videos
            scripting.GetVideoCatalogService().SetVideoGenre(video, genre_to_set_to);
        }
    }
}

#endregion
