#region samples_concat_videos

using System.Collections.Generic;
using VideoCataloger;
using System.IO;
using System;

/// <summary>
///  This sample shows a dialog with buttons where the user can call a window cmd line.
///  https://ffmpeg.org/ to download ffmpeg.exe
/// </summary>
public class ConcatVideos
{
    /// <summary>
    ///  This is the entry function called by fvc.
    /// </summary>
    static public void Run(IScripting scripting, string argument)
    {
        scripting.GetConsole().Clear();
        ISelection selection = scripting.GetSelection();
        List<long> selected = selection.GetSelectedVideos();
        if (selected.Count == 1)
        {
            scripting.GetConsole().WriteLine("Select more than one video");
            return;
        }

        ConcatVideos merger = new ConcatVideos(scripting);
        merger.Concat(null,null);
    }

    IScripting m_scripting = null;

    /// <summary>
    ///  Class constructor, extract the selected data from the interface and store in class members.
    /// </summary>
    ConcatVideos(IScripting scripting)
    {
        m_scripting = scripting;
    }

    /// <summary>
    ///  Construct the path to the cmd line tool. Note that ffmpeg is not included with the installer
    /// </summary>
    public string GetFFMPEGPath()
    {
        string tool_path = "c:\\ffmpeg\\bin\\ffmpeg.exe";
        if (!File.Exists(tool_path))
        {
            System.Windows.MessageBox.Show(tool_path, "ffmpeg missing");
        }
        return tool_path;
    }


    /// <summary>
    /// Call the cmd line tool. The sample just use -i that goes on the file extensions.
    /// Use the conversion_parameters for more advanced conversions.
    /// </summary>
    private void Concat( string out_file, string conversion_parameters )
    {
        string tool_path = GetFFMPEGPath();
        string tmp_file_path = System.IO.Path.GetTempFileName();

        var stream_writer = System.IO.File.CreateText(tmp_file_path);
        if (stream_writer == null)
        {
            System.Windows.MessageBox.Show(tool_path, "Failed to write temporary text file");
            return;
        }

        ISelection selection = m_scripting.GetSelection();
        IUtilities utilities = m_scripting.GetUtilities();
        var catalog = m_scripting.GetVideoCatalogService();
        string extension = null;
        string video_format = null;
        string video_width = null;
        string video_height = null;
        string audio_format = null;
        List<long> selected = selection.GetSelectedVideos();
        bool can_do_pure_concat = true;
        foreach (long video_id in selected)
        {
            var entry = catalog.GetVideoFileEntry(video_id);
            string video_path = utilities.ConvertToLocalPath(entry.FilePath);

            var extended = catalog.GetVideoFileExtendedProperty((int)video_id);
            foreach (var prop in extended )
            {
                if (prop.Property == "video_Format")
                {
                    if (video_format == null)
                    {
                        video_format = prop.Value;
                        m_scripting.GetConsole().WriteLine( video_path + " - Video format is "  + video_format );
                    }
                    else if (video_format != prop.Value)
                    {
                        string msg = "Aborting. All videos must be in the same video format.\n'";
                        msg += video_path + "' is in " + prop.Value + "\n";
                        msg += " previous was in " + video_format + "\n";
                        m_scripting.GetConsole().WriteLine( msg);
                        can_do_pure_concat = false;
                    }
                }
                else if (prop.Property == "video_Width")
                {
                    if (video_width == null)
                    {
                        video_width = prop.Value;
                        m_scripting.GetConsole().WriteLine(video_path + " - Video width is " + video_width);
                    }
                    else if (video_width != prop.Value)
                    {
                        string msg = "Aborting. All videos must be in the same dimension.\n'";
                        msg += video_path + "' width is " + prop.Value + "\n";
                        msg += " previous was in " + video_width + "\n";
                        m_scripting.GetConsole().WriteLine(msg);
                        can_do_pure_concat = false;
                    }
                }
                if (prop.Property == "video_Height")
                {
                    if (video_height == null)
                    {
                        video_height = prop.Value;
                        m_scripting.GetConsole().WriteLine(video_path + " - Video height is " + video_height);
                    }
                    else if (video_height != prop.Value)
                    {
                        string msg = "Aborting. All videos must be in the same dimension.\n'";
                        msg += video_path + "' height is " + prop.Value + "\n";
                        msg += " previous was in " + video_height + "\n";
                        m_scripting.GetConsole().WriteLine(msg);
                        can_do_pure_concat = false;
                    }
                }
                if (prop.Property == "audio_Format")
                {
                    if (audio_format == null)
                    {
                        audio_format = prop.Value;
                        m_scripting.GetConsole().WriteLine(video_path + " - Audio format is " + audio_format);
                    }
                    else if (audio_format != prop.Value)
                    {
                        string msg = "Aborting. All videos must be in the same audio format.\n";
                        msg += video_path + "is in " + prop.Value + "\n";
                        msg += " previous was in " + audio_format + "\n";
                        m_scripting.GetConsole().WriteLine(msg);
                        can_do_pure_concat = false;
                    }
                }
            }
            stream_writer.Write("file '" + video_path + "'" );
            stream_writer.WriteLine();

            // if we can not do a pure concat we abort here. To continue we would need to re-encode the file and that 
            // takes a bit more care and user intervention
            if (!can_do_pure_concat)
                return;
            if (extension==null)
            {
                int extension_start = video_path.LastIndexOf(".");
                extension = video_path.Substring(extension_start);
            }
        }
        stream_writer.Close();

        if (out_file == null)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "concat" + extension;
            dlg.DefaultExt = extension;
            dlg.Filter = "All files|*.*";
            Nullable<bool> result = dlg.ShowDialog();
            if (result == false)
            {
                return;
            }
            out_file = dlg.FileName;
        }

        //
        // do concatination
        //
        System.Diagnostics.Process process = new System.Diagnostics.Process();
        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
        startInfo.FileName = "cmd.exe";
        string cmd_line = " -f concat -safe 0 -i " + tmp_file_path + conversion_parameters + " -c copy \"" + out_file + "\"";
        startInfo.Arguments = "/C " + tool_path + " " + cmd_line; // use /K instead of /C to keep the cmd window up
        process.StartInfo = startInfo;

        m_scripting.GetConsole().WriteLine("Running " + startInfo.Arguments);

        process.Start();
        process.WaitForExit();

        File.Delete(tmp_file_path);

        //
        // Show video in shell player
        //
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
        {
            FileName = out_file,
            UseShellExecute = true,
            Verb = "open"
        });
       
    }
}

#endregion
