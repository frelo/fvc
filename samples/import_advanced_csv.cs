#region samples_advanced_import_csv


using System.Collections.Generic;
using VideoCataloger;
using Microsoft.Win32;
using System;
using System.IO;

namespace VideoCataloger
{
    using VideoCatalogService;

    /// <summary>
    ///  This sample reads a csv file and assigns the strings as keywords to the currently selected video.
    /// </summary>
    public class ImportAdvancedCSV
    { 
        /// <summary>
        /// </summary>
        static public void Run(IScripting scripting, string argument)
        {
            var catalog = scripting.GetVideoCatalogService();

            string csv_path = "C:\\tmp\\hub\\pornhub.com-db\\pornhub.com-db.csv";

            char[] separator = { '|' };

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
                        int current_column = 0;
                        foreach (string new_tag_string in columns)
                        {
                            current_column++;

                            scripting.GetConsole().WriteLine("Column " + current_column + " data " + new_tag_string );
                        }

                        line = textReader.ReadLine();
                        return;
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



#endregion
