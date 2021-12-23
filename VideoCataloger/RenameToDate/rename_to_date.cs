#region samples_rename_to_date


using System;
using System.IO;
using System.Runtime;
using System.Collections.Generic;
using VideoCataloger;

/// <summary>
///  This sample takes the currently selected videos, check the date of the video file and rename the 
///  video file to the date of the file.  
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
            // Get the video file entry
            var entry = scripting.GetVideoCatalogService().GetVideoFileEntry(video);
            scripting.GetConsole().WriteLine(System.Convert.ToString("Processing..." + entry.FilePath));

            // Get the date of the file
            DateTime creation = File.GetCreationTime(entry.FilePath);
            String creeation_string = creation.ToString("yyyyMMdd_HHmmss");

            // Copy the video file type extension
            var extension_start = entry.FilePath.LastIndexOf(".");
            creeation_string += entry.FilePath.Substring(extension_start);

            // and create the path to the file.
            var path_end = entry.FilePath.LastIndexOf("\\");
            String target_path = entry.FilePath.Substring(0, path_end);
            scripting.GetConsole().WriteLine(System.Convert.ToString("Newname to:" + target_path + "\\" + creeation_string));

            // rename the file
            System.IO.File.Move(entry.FilePath, target_path + "\\" + creeation_string);

            // and update the catalog witb the new file path
            scripting.GetVideoCatalogService().SetVideoProperty(video, "FilePath", target_path + "\\" + creeation_string);
        }

        // refresh the gui to show the changed file paths.
        scripting.GetGUI().Refresh("");
    } 
}
#endregion
