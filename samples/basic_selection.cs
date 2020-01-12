#region samples_selection

using System.Runtime;
using System.Collections.Generic;
using VideoCataloger;

/// <summary>
///  This sample prints the ID of the currently selected videos. ID are used to reference videos
///  in the whole api.
/// </summary>
public class Selection
{
    /// <summary>
    ///  Run sample. This is the entry function called by fvc.
    /// </summary>
    static public void Run(IScripting scripting, string argument)
    {
        scripting.GetConsole().Clear();
        ISelection selection = scripting.GetSelection();
        List<long> selected = selection.GetSelectedVideos();
        foreach (long video in selected)
            scripting.GetConsole().WriteLine(System.Convert.ToString(video));
    }
}

#endregion
