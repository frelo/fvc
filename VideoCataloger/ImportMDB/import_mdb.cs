#region samples_import_mdb


using System;
using System.Runtime;
using System.IO;
using System.Collections.Generic;
using Microsoft.Win32;
using VideoCataloger;

namespace VideoCataloger
{
    using RemoteCatalogService;
    using System.Data.OleDb;

    /// <summary>
    ///  This sample reads a mdb file (Microsoft Access) database file.
    ///  This specific example takes a movie from the "All my movies"
    ///  software and use that to create videos entries in your catalog.
    /// </summary>
    public class Script
    {
        /// <summary>
        /// </summary>
        static public async System.Threading.Tasks.Task Run(IScripting scripting, string argument)
        {
            scripting.GetConsole().Clear();

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "access"; // Default file name
            dlg.DefaultExt = ".mdb"; // Default file extension
            dlg.Filter = "mdb file (.mdb)|*.mdb|AllMyMovies file (.amm)|*.amm|All files (.*)|*.*"; // Filter files by extension
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {

                try
                {
                    // https://www.microsoft.com/en-us/download/details.aspx?id=13255
                    // to download the access database runtime of the provider is not installed
                    // note you need the 64 bit version

                    OleDbConnection connection = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + dlg.FileName + @";User Id=admin;Password =;");
                    connection.Open();
                    OleDbDataReader reader = null;
                    OleDbCommand command = new OleDbCommand("SELECT * from  movies", connection);
                    reader = command.ExecuteReader();

                    // Setup the extended video properties
                    var catalog = scripting.GetVideoCatalogService();
                    catalog.SetPropertyMeta("video_property", "year", "edit");
                    catalog.SetPropertyMeta("video_property", "studio", "edit");
                    catalog.SetPropertyMeta("video_property", "trailer", "edit");
                    catalog.SetPropertyMeta("video_property", "length", "edit");
                    catalog.SetPropertyMeta("video_property", "comments", "edit");

                    while (reader.Read())
                    {
                        string name = reader["Name"].ToString();
                        string path = reader["LocalPath"].ToString();
                        string description = reader["description"].ToString();
                        string url = reader["url"].ToString();

                        int video_id = catalog.AddVideo(path, name, 0, description, 0, 0, url, null, 0, null, null);

                        string year = reader["year"].ToString();
                        catalog.SetVideoFileExtendedProperty(video_id, "year", year);
                        string studio = reader["studio"].ToString();
                        catalog.SetVideoFileExtendedProperty(video_id, "studio", studio);
                        string trailer = reader["trailer"].ToString();
                        catalog.SetVideoFileExtendedProperty(video_id, "trailer", trailer);
                        string length = reader["length"].ToString();
                        catalog.SetVideoFileExtendedProperty(video_id, "length", length);
                        string comments = reader["comments"].ToString();
                        catalog.SetVideoFileExtendedProperty(video_id, "comments", comments);
                    }

                    connection.Close();
                    scripting.GetGUI().Refresh("");
                }
                catch (Exception ex)
                {
                    scripting.GetConsole().WriteLine( ex.Message );
                }
            }
        }
    }

}



#endregion
