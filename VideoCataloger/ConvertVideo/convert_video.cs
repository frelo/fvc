#region samples_convert_video

using System.Collections.Generic;
using VideoCataloger;
using System.Runtime;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System;


/// <summary>
///  This sample shows a dialog with buttons where the user can call a window cmd line.
/// </summary>
public class ConvertVideo
{
    /// <summary>
    ///  Run sample. This is the entry function called by fvc.
    /// </summary>
 
    IScripting m_scripting = null;
    string m_SelectedVideoPath = null;
    string m_SelectedVideoTitle = null;
    System.Drawing.Image m_VideoImage = null;

    /// <summary>
    ///  Class constructor, extract the selected data from the interface and store in class members.
    /// </summary>
    public ConvertVideo(IScripting scripting)
    {
        m_scripting = scripting;

        ISelection selection = m_scripting.GetSelection();
        var catalog = m_scripting.GetVideoCatalogService();
        List<long> selected = selection.GetSelectedVideos();
        if (selected.Count == 1)
        {
            long video = selected[0];
            var entry = catalog.GetVideoFileEntry(video);

            IUtilities utilities = m_scripting.GetUtilities();
            m_SelectedVideoPath = utilities.ConvertToLocalPath(entry.FilePath);
            m_SelectedVideoTitle = entry.Title;


            byte[] image_data = catalog.GetVideoFileImage(video);
            if (image_data == null)
                return;

            MemoryStream stream = new MemoryStream(image_data);
            m_VideoImage = System.Drawing.Bitmap.FromStream(stream);
        }
    }

    /// <summary>
    ///  Construct the path to the cmd line tool. Note that ffmpeg is not included with the installer
    /// </summary>
    public string GetFFMPEGPath()
    {
        string current_folder = System.IO.Directory.GetCurrentDirectory();
        string tool_path = current_folder + "\\ffmpeg\\bin\\ffmpeg.exe";
        if (!File.Exists(tool_path))
            System.Windows.MessageBox.Show(tool_path, "ffmpeg missing");
        return tool_path;
    }

    /// <summary>
    /// convert the selected video to mp4, please note that the conversion will take long.
    /// </summary>
    public void convert_to_mp4( object sender, EventArgs args )
    {
        Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
        dlg.FileName = System.IO.Path.GetFileNameWithoutExtension( m_SelectedVideoTitle);
        dlg.DefaultExt = ".mp4";
        dlg.Filter = "mp4 file (.mp4)|*.mp4";
        Nullable<bool> result = dlg.ShowDialog();
        if (result == false)
        {
            return;
        }

        Convert(dlg.FileName, "");
    }

    /// <summary>
    /// extract to audio track and convert to mp3.
    /// </summary>
    public void convert_to_mp3(object sender, EventArgs args)
    {
        Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
        dlg.FileName = System.IO.Path.GetFileNameWithoutExtension(m_SelectedVideoTitle);
        dlg.DefaultExt = ".mp3"; 
        dlg.Filter = "mp3 file (.mp3)|*.mp3";
        Nullable<bool> result = dlg.ShowDialog();
        if (result == false)
        {
            return;
        }

        Convert(dlg.FileName, "");
    }

    /// <summary>
    /// Call the cmd line tool. The sample just use -i that goes on the file extensions.
    /// Use the conversion_parameters for more advanced conversions.
    /// </summary>
    private void Convert( string out_file, string conversion_parameters )
    {
        string tool_path = GetFFMPEGPath();

        System.Diagnostics.Process process = new System.Diagnostics.Process();
        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
        startInfo.FileName = "cmd.exe";
        string cmd_line = " -i \"" + m_SelectedVideoPath + "\" " + conversion_parameters + " \"" + out_file + "\"";
        startInfo.Arguments = "/C " + tool_path + " " + cmd_line;
        process.StartInfo = startInfo;
        m_scripting.GetConsole().WriteLine("Running " + startInfo.Arguments);

        process.Start();
        process.WaitForExit();
    }

    /// <summary>
    /// Setup the buttons, text on the button and the function to call when clicking the button.
    /// </summary>
    private void SetInfoForButton( Button btn, int button_no)
    {
        switch (button_no)
        {
            case 0:
                btn.Text = "Convert to MP4";
                btn.Click += new EventHandler(convert_to_mp4);
                break;
            case 1:
                btn.Text = "Convert to MP3";
                btn.Click += new EventHandler(convert_to_mp3);
                break;
        }
    }

    /// <summary>
    /// Setup the winform dialog. Note that the dialog is modal so fast video cataloger will not 
    /// be running any gui while the dialog is visible.
    /// </summary>
    public void ShowForm()
    {
        using (Form form = new Form())
        {
            form.Text = "Video File Converter";

            int padding = 10;
            int buttons_left = 320;
            Size button_size = new Size(128, 32);

            form.StartPosition = FormStartPosition.CenterScreen;
            form.Size = new Size(640, 480);

            int nof_buttons = 2;
            for (int n=0;n<nof_buttons;++n)
            {
                Button btn = new Button();
                btn.Left = buttons_left + padding;
                btn.Top = n * (button_size.Height + padding);
                btn.Size = button_size;
                SetInfoForButton(btn, n);
                form.Controls.Add(btn);
            }

            // Show the picture of the video so the user can be sure its the right video
            PictureBox pb = new PictureBox();
            pb.Dock = DockStyle.None;
            pb.Size = new Size(320, 256);
            pb.Image = m_VideoImage;

            form.Controls.Add(pb);
            form.ShowDialog();
        }
    }

    /// <summary>
    ///  Display a winforms dialog with the currently selected video file image.
    /// </summary>
    public void ShowVideoPopup()
    {
        if (m_SelectedVideoPath == null)
        {
            m_scripting.GetConsole().WriteLine("Please select one video file");
            return;
        }

        ShowForm();
    }
}

public class Script
{
    static public async System.Threading.Tasks.Task Run(IScripting scripting, string argument)
    {
        ConvertVideo converter = new ConvertVideo(scripting);
        converter.ShowVideoPopup();
    }
}

#endregion
