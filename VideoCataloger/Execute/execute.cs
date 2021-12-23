#region samples_execute


using System.Runtime;
using System.Collections.Generic;
using VideoCataloger;
using Microsoft.Win32;
using System;

/// <summary>
///  This sample executes a command line with the selected video file as argument. 
///  Run a command line that copies the selected video to a backup.
///  replace "startInfo.Arguments" with the command line you want to run.
/// </summary>
public class Script
{
    /// <summary>
    ///  the tool to execute is passed as argument.
    /// </summary>
    static public async System.Threading.Tasks.Task Run(IScripting scripting, string argument)
    {
        ISelection selection = scripting.GetSelection();
        var catalog = scripting.GetVideoCatalogService();
        List<long> selected = selection.GetSelectedVideos();
        if (selected.Count == 0)
        {
            System.Windows.MessageBox.Show( "No Video selected", "Execute sample");
            return;
        }

        foreach (long video in selected)
        {
            var entry = catalog.GetVideoFileEntry( video );

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            string cmd_line = " copy \"" + entry.FilePath + "\" \"" + entry.FilePath + "_bak\"";
            startInfo.Arguments = "/C " + cmd_line;
            process.StartInfo = startInfo;
           
            process.Start();
            process.WaitForExit();
        }
    }
}

#endregion
