#region unmask_to_source_folder


using System;
using System.IO;
using System.Runtime;
using System.Collections.Generic;
using VideoCataloger;

/// <summary>
///  This sample unmask a masked video file to the folder where the source file is but not to where it was masked from
/// </summary>
class Script
{
    static public async System.Threading.Tasks.Task Run(IScripting scripting, string arguments)
    {
        scripting.GetConsole().Clear();
        ISelection selection = scripting.GetSelection();
        List<long> selected = selection.GetSelectedVideos();
        foreach (long video in selected)
        {
            var entry = scripting.GetVideoCatalogService().GetVideoFileEntry(video);
            var path_start = entry.FilePath.LastIndexOf("\\");
            string folder_string = entry.FilePath.Substring(0, path_start);
            var name_start = entry.Encrypted.LastIndexOf("\\");
            string name_string = entry.Encrypted.Substring(name_start);
            string target_path = folder_string + name_string;
            scripting.GetVideoCatalogService().SetVideoProperty(video, "Encrypted", target_path);

            scripting.GetUtilities().Unmask(video);
        }

        scripting.GetGUI().Refresh("");
    }
}
#endregion
