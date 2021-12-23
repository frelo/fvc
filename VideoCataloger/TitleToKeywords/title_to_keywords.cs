#region title_to_keywords


using System;
using System.IO;
using System.Runtime;
using System.Linq;
using System.Collections.Generic;
using VideoCataloger;

/// <summary>
///  Take the title and use it to generate keywords.  
///  If there is a @ in the title we treat that as an actor
///  the text before @ is first name and the text after is last name
///  if an actor with that name already exist we use that one.
/// </summary>
class Script
{
    static public async System.Threading.Tasks.Task Run(IScripting scripting, string arguments)
    { 
        scripting.GetConsole().Clear();
        var catalog = scripting.GetVideoCatalogService();
        ISelection selection = scripting.GetSelection();
        List<long> selected = selection.GetSelectedVideos();
        foreach (long video in selected)
        {
            // Get the video file entry
            var entry = catalog.GetVideoFileEntry(video);
            scripting.GetConsole().WriteLine(System.Convert.ToString("Processing..." + entry.FilePath));

            char[] separators = { ' ', ',', '.', '-', '[' ,']', '{', '}', '_' };
            string[] ignore_words = { "is", "are", "who", "where" };
            string title = entry.Title;
            string[] keywords = title.Split(separators);
            int min_length = 3;
            foreach (string word in keywords)
            {
                if (word.Length>= min_length)
                {
                    if (!ignore_words.Contains(word))
                    {
                        if (word.Contains("@"))
                        {
                            // Actor
                            string[] names = word.Split('@');
                            string first_name = names[0];
                            string last_name = names[1];

                            scripting.GetConsole().WriteLine( "Actor FirstName:"+ first_name + " LastName:" + last_name );

                            int actor_id = -1;
                            VideoCataloger.RemoteCatalogService.Actor[] current_actors = catalog.GetActors(null, first_name, last_name, true);
                            if (current_actors.Length >= 1)
                            {
                                actor_id = current_actors[0].ID;
                            }
                            else
                            {
                                VideoCataloger.RemoteCatalogService.Actor actor = new VideoCataloger.RemoteCatalogService.Actor();
                                actor.FirstName = first_name;
                                actor.LastName = last_name;
                                actor_id = catalog.AddActorToDB(actor);
                            }

                            if (actor_id != -1)
                                catalog.AddActorToVideo(video, actor_id);
                        }
                        else
                        {
                            // Keywords
                            scripting.GetConsole().WriteLine("Keyword:" + word );
                            scripting.GetVideoCatalogService().TagVideo(video, word);
                        }
                    }
                }
            }
        }

        // refresh the gui to show the changed file paths.
        scripting.GetGUI().Refresh("");
    } 
}
#endregion
