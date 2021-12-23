#region samples_import_csv


using System.Collections.Generic;
using System.Runtime;
using Microsoft.Win32;
using System;
using System.IO;
using VideoCataloger;

namespace VideoCataloger
{
    using RemoteCatalogService;

    /// <summary>
    ///  This sample reads a csv file and assigns the strings as keywords to the currently selected video.
    /// </summary>
    public class Script
    {
        /// <summary>
        /// </summary>
        static public async System.Threading.Tasks.Task Run(IScripting scripting, string argument)
        {
            ISelection selection = scripting.GetSelection();
            List<long> selected = selection.GetSelectedVideos();
            if (selected == null)
            {
                scripting.GetConsole().WriteLine("Please select videos to add keywords to");
                return;
            }

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "csvfile"; // Default file name
            dlg.DefaultExt = ".csv"; // Default file extension
            dlg.Filter = "csv file (.csv)|*.csv|All files (.*)|*.*"; // Filter files by extension
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                var catalog = scripting.GetVideoCatalogService();

                string csv_path = dlg.FileName;
                char[] separator = { ',', ';', '.' };

                try
                {
                    using (var textReader = new StreamReader(csv_path))
                    {
                        scripting.GetConsole().WriteLine("Reading from " + csv_path);
                        string line = textReader.ReadLine();
                        while (line != null)
                        {
                            string[] columns = line.Split(separator);

                            //perform your logic
                            foreach (string new_tag_string in columns)
                            {
                                foreach (long video_id in selected)
                                {
                                    catalog.TagVideo(video_id, new_tag_string);
                                    scripting.GetConsole().WriteLine("Adding keyword " + new_tag_string + " to video " + video_id);
                                }
                            }

                            line = textReader.ReadLine();
                        }

                        scripting.GetConsole().WriteLine("Import done");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }

}



#endregion
