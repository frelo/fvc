#region samples_title_case

using System;
using System.Runtime;
using System.Collections.Generic;
using VideoCataloger;
using VideoCataloger.RemoteCatalogService;


/// <summary>
///  This sample change casing of the title of a video.
/// </summary>
public class Script
{
    /// <summary>
    ///  Run sample. This is the entry function called by fvc.
    /// </summary>
    static public async System.Threading.Tasks.Task Run(IScripting scripting, string argument)
    {
        scripting.GetConsole().Clear();
        ISelection selection = scripting.GetSelection();
        var VideoCatalog = scripting.GetVideoCatalogService();
        List<long> selected = selection.GetSelectedVideos();
        foreach (long video in selected)
        {
            VideoFileEntry entry = VideoCatalog.GetVideoFileEntry(video);
            string Title = entry.Title;
            if (Title != "")
            {
                if (argument=="upper")
                    Title = Title.ToUpper();
                else if (argument == "lower")
                    Title = Title.ToLower();
                else if (argument == "sentence")
                {
                    Title = Title.ToLower();
                    char c = char.ToUpper(Title[0]);
                    Title = c + Title.Substring(1);
                }
                else if (argument == "toggle")
                {
                    string NewTitle = "";
                    for (int n = 0; n < Title.Length; ++n)
                    {
                        if (char.IsUpper( Title[n]) )
                        {
                            NewTitle += char.ToLower(Title[n]);
                        }
                        else
                        {
                            NewTitle += char.ToUpper(Title[n]);
                        }
                    }
                    Title = NewTitle;
                }
                else if (argument == "eachword")
                {
                    Title = Title.ToLower();
                    string NewTitle = "";
                    bool captitilize = true;
                    for (int n=0;n<Title.Length;++n)
                    {
                        if (Title[n]==' ')
                        {
                            NewTitle += ' ';
                            captitilize = true;
                        }
                        else
                        {
                            if (captitilize)
                            {
                                NewTitle += char.ToUpper(Title[n]);
                                captitilize = false;
                            }
                            else
                            {
                                NewTitle += Title[n];
                            }
                        }

                    }
                    Title = NewTitle;
                }
                else
                {
                    scripting.GetConsole().WriteLine("Unknown argument: " + argument);
                    return;
                }

                VideoCatalog.SetVideoProperty( video, "title", Title );
            }
        }
        scripting.GetGUI().Refresh("");
    }
}

#endregion
