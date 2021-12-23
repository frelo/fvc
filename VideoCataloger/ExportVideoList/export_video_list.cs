#region export_video_list


using System;
using System.IO;
using System.Runtime;
using System.Collections.Generic;
using VideoCataloger;
using VideoCataloger.RemoteCatalogService;


/// <summary>
///  This sample exports the list of selected videos
/// </summary>
class Script
{

    static public async System.Threading.Tasks.Task Run(IScripting scripting, string arguments)
    {
        scripting.GetConsole().Clear();

        var service = scripting.GetVideoCatalogService();
        ISelection selection = scripting.GetSelection();
        List<long> selected = selection.GetSelectedVideos();
        if (selected.Count == 0)
        {
            scripting.GetConsole().WriteLine("Select videos to export");
            return;
        }

        string target_folder = "c:\\export\\";
        try
        {
            DirectoryInfo info = Directory.CreateDirectory(target_folder);
        }
        catch (Exception ex)
        {
            scripting.GetConsole().WriteLine(ex.Message);
        }

        string html_header = "<html><head></head><body>";
        string html_footer = "</body></html>";
        string filename = target_folder + "index.html";

        System.IO.File.WriteAllText(filename, html_header);


        int item = 0;
        int items_per_row = 1;
        bool in_row = false;
        string table_start = "<table>";
        string table_end = "</table>";

        System.IO.File.AppendAllText(filename, table_start);

        foreach (long video_id in selected)
        {
            if (in_row && (item % items_per_row) == 0)
            {
                System.IO.File.AppendAllText(filename, "</tr>");
                in_row = false;
            }
            if ((item % items_per_row)==0)
            {
                System.IO.File.AppendAllText(filename, "<tr>");
                in_row = true;
            }


            var video_file_entry = service.GetVideoFileEntry(video_id);

            string line = "";
            string video_thumb_name = item + ".jpg";
            line = "<td>" + video_file_entry.ID.ToString() + "</td>";
            line = "<td><img src=\"";
            System.IO.File.AppendAllText(filename, line);
            line = video_thumb_name;
            System.IO.File.AppendAllText(filename, line);
            line = "\"></td>";
            System.IO.File.AppendAllText(filename, line);
            string path = scripting.GetUtilities().ConvertToLocalPath(video_file_entry.FilePath);
            line = "<td><a href=\"" + path + "\">" + video_file_entry.Title + "</a></td>";
            System.IO.File.AppendAllText(filename, line);
            line = "<td>" + video_file_entry.Rating + "</td>";
            System.IO.File.AppendAllText(filename, line);
            line = "<td>" + video_file_entry.Description + "</td>";
            System.IO.File.AppendAllText(filename, line);

            byte[] video_image = service.GetVideoFileImage( video_id);
            System.IO.File.WriteAllBytes(target_folder + video_thumb_name, video_image);
            item++;
        }

        if (in_row)
        {
            System.IO.File.AppendAllText(filename, "</tr>");
        }

        System.IO.File.AppendAllText(filename, table_end);
        System.IO.File.AppendAllText(filename, html_footer);
        scripting.GetConsole().WriteLine("Html exported to: " + filename);

    }
}
#endregion
