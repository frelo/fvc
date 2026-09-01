using System;
using VideoCataloger;
using VideoCataloger.RemoteCatalogService;

// Run a search from a script: find all videos with a rating at or above a threshold and
// show the result in the program, as if the query had been entered in the search window.
// Pass the minimum rating (1-5) as the script argument; the default is 4.
//
// Demonstrates building a VideoQuery and pushing it to the user interface with
// IGUI.SetQuery.
class Script
{
    static public async System.Threading.Tasks.Task Run(IScripting scripting, string arguments)
    {
        IConsole console = scripting.GetConsole();
        console.Clear();

        int min_rating = 4;
        string argument = (arguments ?? "").Trim();
        if (argument.Length > 0 && !int.TryParse(argument, out min_rating))
        {
            console.WriteLine("Pass the minimum rating (1-5) as the script argument.");
            return;
        }

        VideoQuery query = new VideoQuery();
        query.Properties = new VideoQueryProperties();
        query.Properties.Rating = min_rating;
        query.Properties.RatingCompare = ">=";

        scripting.GetGUI().SetQuery(query, null); // null scene query - only search videos
        console.WriteLine("Showing videos with rating >= " + min_rating + ".");
    }
}
