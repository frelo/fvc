#region samples_import_database
// Reads video records from another program's database and creates a catalog entry from
// each. The database here is SQLite, read with Microsoft.Data.Sqlite, which Fast Video
// Cataloger ships and has loaded already (its own catalogs are SQLite), so the script runs
// with nothing installed. Any database with an ADO.NET provider is read the same way:
// SqlConnection for SQL Server, NpgsqlConnection for PostgreSQL, OleDbConnection for
// Access (.mdb/.accdb; needs the System.Data.OleDb package in the program folder, loaded
// with //css_ref, and Microsoft's 64-bit Access Database Engine). Open a connection, run
// a query, read the rows, add one video per row.
//
// The example expects a table
//   movies (Name, LocalPath, description, url, year, studio, trailer, length, comments)
// Change the query and the column names to match your database. The database file can
// be given as the script argument; without one a file dialog asks for it.

using System;
using System.IO;
using Microsoft.Win32;
using Microsoft.Data.Sqlite;
using VideoCataloger;

namespace VideoCataloger
{
    using RemoteCatalogService;

    public class Script
    {
        // The columns stored as extended properties of each video
        static readonly string[] Properties = { "year", "studio", "trailer", "length", "comments" };

        static public async System.Threading.Tasks.Task Run(IScripting scripting, string argument)
        {
            IConsole console = scripting.GetConsole();
            console.Clear();

            // 1. The database file: the script argument, or a file dialog
            string file = argument;
            if (string.IsNullOrWhiteSpace(file))
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Filter = "SQLite database (*.sqlite;*.db;*.sqlite3)|*.sqlite;*.db;*.sqlite3|All files (*.*)|*.*";
                if (dlg.ShowDialog() != true)
                    return;
                file = dlg.FileName;
            }

            var catalog = scripting.GetVideoCatalogService();

            // 2. Register the extended properties the rows fill in, so they show in the
            //    Video Details window, the search filter and the column picker
            foreach (string property in Properties)
                catalog.SetPropertyMeta("video_property", property, "edit");

            int added = 0, skipped = 0;
            try
            {
                // 3. Open the database read-only and run the query
                using (SqliteConnection connection = new SqliteConnection("Data Source=" + file + ";Mode=ReadOnly"))
                {
                    connection.Open();
                    using (SqliteCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT Name, LocalPath, description, url, year, studio, trailer, length, comments FROM movies";
                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            // 4. One catalog entry per row
                            while (reader.Read())
                            {
                                string path = Text(reader, "LocalPath");
                                if (path == "" || !File.Exists(path))
                                {
                                    console.WriteLine("Skipped, file not found: " + path);
                                    skipped++;
                                    continue;
                                }

                                long video_id = catalog.AddVideo(path, Text(reader, "Name"), 0, Text(reader, "description"), 0, 0, Text(reader, "url"), null, 0, null, null);
                                foreach (string property in Properties)
                                {
                                    string value = Text(reader, property);
                                    if (value != "")
                                        catalog.SetVideoFileExtendedProperty(video_id, property, value);
                                }
                                added++;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                console.WriteLine("Import failed: " + ex.Message);
            }

            console.WriteLine(added + " videos added, " + skipped + " skipped.");

            // 5. Show the new entries. They have no thumbnails yet: select them in the Video
            //    Catalog window and choose Reindex file from the Index menu.
            scripting.GetGUI().Refresh("");
        }

        /// <summary>A column of the current row as text, empty when NULL.</summary>
        static string Text(SqliteDataReader reader, string column)
        {
            int i = reader.GetOrdinal(column);
            return reader.IsDBNull(i) ? "" : reader.GetValue(i).ToString();
        }
    }
}
#endregion
