#region export_thumbnails


using System;
using System.IO;
using System.Collections.Generic;
using VideoCataloger;
using VideoCataloger.RemoteCatalogService;


/// <summary>
///  This sample exports the thumbnails of selected videos to the folder of the videos
/// </summary>
class ExportThumbnails
{

    static public void Run(IScripting scripting, string arguments)
    {
        scripting.GetConsole().Clear();

        var service = scripting.GetVideoCatalogService();
        ISelection selection = scripting.GetSelection();
        List<long> selected = selection.GetSelectedVideos();
        foreach (long video_id in selected)
        {
            var video_file_entry = service.GetVideoFileEntry(video_id);
            string target_folder = video_file_entry.FilePath; // this is where the images are saved
            int path_end = target_folder.LastIndexOf('\\');
            target_folder = target_folder.Substring(0, path_end+1);
            target_folder += "Thumbnails\\";
            try
            {
                DirectoryInfo info = Directory.CreateDirectory(target_folder);
            }
            catch (Exception ex)
            {
                scripting.GetConsole().WriteLine( ex.Message );
            }

            long image_no = 1;
            Dictionary<long, ThumbnailEntry> thumbnails = service.GetThumbnailsForVideo(video_id, true );
            foreach (KeyValuePair<long, ThumbnailEntry> thumbnail_entry in thumbnails )
            {
                byte[] image_data = thumbnail_entry.Value.Image;

                string filename = target_folder + image_no.ToString() + ".jpg";
                scripting.GetConsole().WriteLine("Saving image to : " + filename);
                System.IO.File.WriteAllBytes(filename, image_data);
                image_no++;
            }
        }

    }
}
#endregion
