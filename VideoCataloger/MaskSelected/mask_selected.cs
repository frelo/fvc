using System;
using System.Collections.Generic;
using VideoCataloger;

// Mask (encrypt) all selected videos. The counterpart to the UnmaskToSource sample.
//
// Demonstrates IUtilities.Mask.
class Script
{
    static public async System.Threading.Tasks.Task Run(IScripting scripting, string arguments)
    {
        IConsole console = scripting.GetConsole();
        console.Clear();

        List<long> selected = scripting.GetSelection().GetSelectedVideos();
        if (selected.Count == 0)
        {
            console.WriteLine("No videos selected.");
            return;
        }

        IUtilities utilities = scripting.GetUtilities();
        foreach (long video_id in selected)
        {
            console.WriteLine("Masking video " + video_id + ".");
            utilities.Mask(video_id);
        }

        scripting.GetGUI().Refresh("");
    }
}
