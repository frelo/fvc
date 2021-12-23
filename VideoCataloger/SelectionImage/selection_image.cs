#region samples_selection_image

using System.Collections.Generic;
using VideoCataloger;
using System.Runtime;
using System.IO;        // For memorystream
using System.Drawing;   // for bitmap
using System.Windows.Forms; // for winforms

/// <summary>
///  This sample prints the ID of the currently selected videos.
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
        if (selected.Count > 0)
        {
            byte[] image = catalog.GetVideoFileImage(selected[0]);
            ShowImagePopup( image );
        }
    }

    /// <summary>
    ///  Display a winforms dialog with the currently selected video file image.
    /// </summary>
    static private void ShowImagePopup(byte[] image_data)
    {
        MemoryStream stream = new MemoryStream(image_data);
        System.Drawing.Image image = System.Drawing.Bitmap.FromStream(stream);

        using (Form form = new Form())
        {
            form.StartPosition = FormStartPosition.CenterScreen;
            form.Size = image.Size;

            form.Width += 100;
            form.Height += 100;


            PictureBox pb = new PictureBox();
            pb.Dock = DockStyle.Fill;
            pb.Image = image;

            form.Controls.Add(pb);
            form.ShowDialog();
        }
    }

}

#endregion
