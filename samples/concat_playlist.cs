#region samples_concat_playlist

using VideoCataloger;
using System.IO;
using System;
using VideoCataloger.RemoteCatalogService;
using System.Globalization;
using System.Collections.Generic;

/// <summary>
///  This sample shows a dialog with buttons where the user can call a window cmd line.
///  https://ffmpeg.org/ to download ffmpeg.exe
/// </summary>
public class ConcatPlaylist
{
    static public string GetFFMPEGPath()
    {
        string tool_path = "c:\\ffmpeg\\bin\\ffmpeg.exe";
        if (!File.Exists(tool_path))
        {
            System.Windows.MessageBox.Show(tool_path, "ffmpeg missing");
            return null;
        }
        return tool_path;
    }

    static public string GetTempFolder()
    {
        string temp_folder = "c:\\tmp\\"; // needs to exist, to temporary store clips before combining them again
        if (!Directory.Exists(temp_folder))
        {
            System.Windows.MessageBox.Show(temp_folder, "Need a temporary folder to create videos in");
            return null;
        }

        return temp_folder;
    }

    /// <summary>
    ///  This is the entry function called by fvc.
    /// </summary>
    static public void Run(IScripting scripting, string argument)
    {
        scripting.GetConsole().Clear();
        ISelection selection = scripting.GetSelection();
        var playlist = selection.GetSelectedPlaylist();
        if (playlist == null)
        {
            scripting.GetConsole().WriteLine("Select a playlist");
            return;
        }

        if (GetFFMPEGPath() == null)
            return;

        if (GetTempFolder() == null)
            return;

        ConcatPlaylist merger = new ConcatPlaylist(scripting);
        merger.Concat(null, null);
    }



    IScripting m_scripting = null;
    List<string> m_FilesToDelete;

    /// <summary>
    ///  Class constructor, extract the selected data from the interface and store in class members.
    /// </summary>
    ConcatPlaylist(IScripting scripting)
    {
        m_scripting = scripting;
        m_FilesToDelete = new List<string>();
    }

    ~ConcatPlaylist()
    {
        foreach (string path in m_FilesToDelete)
        {
            File.Delete(path);
        }
    }

    private void SaveClip(string path, double start, double end, string out_path)
    {
        // need to fix so this is not whole seconds...
        DateTime start_time = DateTime.SpecifyKind( new DateTime(1990, 1, 1,  0, 0, 0), DateTimeKind.Unspecified);
        start_time = start_time.AddMilliseconds(start*1000);

        double duration = end - start;
        DateTime duration_time = DateTime.SpecifyKind( new DateTime(1990, 1, 1, 0, 0, 0,0,0 ), DateTimeKind.Unspecified);
        duration_time = duration_time.AddMilliseconds(duration * 1000);

        string cmd_line = " -i " + path + " -ss " + string.Format("{0:HH:mm:ss.f}", start_time ) + " -t " + string.Format("{0:HH:mm:ss.f}", duration_time) + " " + out_path;

        m_FilesToDelete.Add(out_path);

        RunFFMPEG( cmd_line );
    }

    private void RunFFMPEG( string arguments )
    {
        string ffmpeg = GetFFMPEGPath();
        System.Diagnostics.Process process = new System.Diagnostics.Process();
        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
        startInfo.FileName = "cmd.exe";
        startInfo.Arguments = "/C " + ffmpeg + " " + arguments; // use /K instead of /C to keep the cmd window up
        process.StartInfo = startInfo;
        process.Start();
        process.WaitForExit();
    }

    private void PlayVideo( string out_file )
    {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
        {
            FileName = out_file,
            UseShellExecute = true,
            Verb = "open"
        });
    }

    /// <summary>
    /// Call the cmd line tool. The sample just use -i that goes on the file extensions.
    /// Use the conversion_parameters for more advanced conversions.
    /// </summary>
    private void Concat( string out_file, string conversion_parameters )
    {
        string tool_path = GetFFMPEGPath();
        string tmp_file_path = System.IO.Path.GetTempFileName();
        m_FilesToDelete.Add(tmp_file_path);

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
        var playlist = selection.GetSelectedPlaylist();
        int[] all_clip_ids = catalog.GetPlaylistClipIDs(playlist.ID);

        int part = 1;
        bool can_do_pure_concat = true;
        foreach (int clip_id in all_clip_ids)
        {
            var clip = catalog.GetVideoClip( clip_id );
            var video_entry = catalog.GetVideoFileEntry(clip.VideoFileID);
            string video_path = utilities.ConvertToLocalPath(video_entry.FilePath);

            var extended = catalog.GetVideoFileExtendedProperty((int)clip.VideoFileID);
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

            if (extension == null)
            {
                int extension_start = video_path.LastIndexOf(".");
                extension = video_path.Substring(extension_start);
            }

            if (can_do_pure_concat)
            {
                string out_path = GetTempFolder() + "clip_" + part + extension;
                part = part + 1;
                SaveClip(video_path, clip.StartTime, clip.EndTime, out_path);

                stream_writer.Write("file '" + out_path + "'");
                stream_writer.WriteLine();
            }
            else
            {
                selection.SetSelectedPlaylistClip(clip_id); // select the offending clip
                // if we can not do a pure concat we abort here. To continue we would need to re-encode the file and that 
                // takes a bit more care and user intervention
                return;
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

        RunFFMPEG("-f concat - safe 0 - i " + tmp_file_path + conversion_parameters + " - c copy \"" + out_file + "\"");

        PlayVideo(out_file);
    }
}

#endregion
