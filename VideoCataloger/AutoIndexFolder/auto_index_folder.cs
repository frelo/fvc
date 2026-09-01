using System;
using System.IO;
using VideoCataloger;

// Add all videos from a folder (including subfolders) to the catalog and start indexing.
// Pass the folder path as the script argument. Videos already in the catalog are skipped.
//
// Demonstrates the IVideoIndexer interface: AddFolder and StartIndexing.
class Script
{
    static public async System.Threading.Tasks.Task Run(IScripting scripting, string arguments)
    {
        IConsole console = scripting.GetConsole();
        console.Clear();

        string folder = (arguments ?? "").Trim().Trim('"');
        if (folder.Length == 0)
        {
            console.WriteLine("Usage: pass the folder to index as the script argument, for example:");
            console.WriteLine("  C:\\Videos\\NewClips");
            return;
        }
        if (!Directory.Exists(folder))
        {
            console.WriteLine("Folder not found: " + folder);
            return;
        }

        IVideoIndexer indexer = scripting.GetVideoIndexer();
        indexer.AddFolder(folder, true, true); // include subfolders, skip videos already in the catalog
        indexer.StartIndexing();

        console.WriteLine("Queued videos in " + folder + " for indexing.");
        console.WriteLine("Watch progress in the Add videos window.");
    }
}
