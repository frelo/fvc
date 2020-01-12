#region samples_selection_popup

using System.Runtime;
using System.Collections.Generic;
using VideoCataloger;

/// <summary>
///  This sample prints the ID of the currently selected videos.
/// </summary>
public class SelectionPopup
{
    /// <summary>
    ///  Run sample. This is the entry function called by fvc.
    /// </summary>
    static public void Run(IScripting scripting, string argument)
    {
        ISelection selection = scripting.GetSelection();
        var catalog = scripting.GetVideoCatalogService();
        List<long> selected = selection.GetSelectedVideos();
        foreach (long video in selected)
        {
            var entry = catalog.GetVideoFileEntry(selected[0]);
            System.Windows.MessageBox.Show(entry.FilePath, "Selected Video");
        }
    }
}

#endregion
