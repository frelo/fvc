using System;
using VideoCataloger;
using VideoCataloger.RemoteCatalogService;

// Visual search from a script: describe a scene in words and show the best matching scenes
// in the program, as if the description had been typed into the search window. Pass the
// description as the script argument, e.g.  a dog running on a beach
//
// Demonstrates the visual search members of SceneQuery (10.4): DescribeText ranks every
// indexed scene by how well it matches the words; SimilarToThumbnailId ranks by likeness to a
// scene instead; MaxVisualResults caps the list. Pushed through IGUI.SetQuery the program
// encodes the description itself, so the Visual search model has to be downloaded (Manage AI
// models) and the catalog needs its search index (built while indexing, or from AI Scenes).
class Script
{
    static public async System.Threading.Tasks.Task Run(IScripting scripting, string arguments)
    {
        IConsole console = scripting.GetConsole();
        console.Clear();

        string description = (arguments ?? "").Trim();
        if (description.Length == 0)
        {
            console.WriteLine("Pass a description of the scene as the script argument, for example: a dog running on a beach");
            return;
        }

        SceneQuery scene_query = new SceneQuery();
        scene_query.DescribeText = description;
        scene_query.MaxVisualResults = 100;          // the 100 best matches; 0 means the default (200)
        // To rank by likeness to a scene instead, set scene_query.SimilarToThumbnailId to its
        // thumbnail id and leave DescribeText empty - only one of the two applies.

        VideoQuery video_query = new VideoQuery();   // empty: rank scenes in every video

        scripting.GetGUI().SetQuery(video_query, scene_query);
        console.WriteLine("Showing the " + scene_query.MaxVisualResults + " scenes that best match: " + description);
    }
}
