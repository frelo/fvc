#region samples_selection_rating

using System.Runtime;
using System.Collections.Generic;
using VideoCataloger;

/// <summary>
///  This sample sets the rating of all selected videos to 1.
/// </summary>
public class Script
{
    /// <summary>
    ///  Run sample. This is the entry function called by fvc.
    /// </summary>
    static public async System.Threading.Tasks.Task Run(IScripting scripting, string argument)
    {
        ISelection selection = scripting.GetSelection();
        var catalog = scripting.GetVideoCatalogService();
        List<long> selected = selection.GetSelectedVideos();
        foreach (long video in selected)
        {
            // set the Rating property to 1 for each of the selected videos
            scripting.GetVideoCatalogService().SetVideoProperty(video, "Rating", "1");
        }
    }
}

#endregion
