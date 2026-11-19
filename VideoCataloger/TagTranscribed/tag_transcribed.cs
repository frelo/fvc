using System;
using System.Linq;
using System.Collections.Generic;
using VideoCataloger;

/// <summary>
/// Tags each selected video "Transcribed" or "No transcript", and removes the other keyword
/// so the tags stay right when the script is run again after more transcription.
/// </summary>
class Script
{
    static public async System.Threading.Tasks.Task Run(IScripting scripting, string arguments)
    {
        const string HAS = "Transcribed";
        const string NONE = "No transcript";

        var console = scripting.GetConsole();
        console.Clear();
        var catalog = scripting.GetVideoCatalogService();
        List<long> selected = scripting.GetSelection().GetSelectedVideos();

        int with = 0, without = 0;
        foreach (long video in selected)
        {
            var lines = await catalog.GetAllSubtitlesAsync(video);
            bool has = lines != null && lines.Length > 0;
            string add = has ? HAS : NONE;
            string drop = has ? NONE : HAS;

            var tags = catalog.GetTagsForVideos(new long[] { video }) ?? new VideoCataloger.RemoteCatalogService.TagInstance[0];
            foreach (var t in tags.Where(t => t.Name == drop))
                catalog.RemoveTagsFromVideo(t.ID);
            if (!tags.Any(t => t.Name == add))
                catalog.TagVideo(video, add);

            if (has) with++; else without++;
        }

        console.WriteLine(selected.Count + " videos: " + with + " transcribed, " + without + " without a transcript.");
        scripting.GetGUI().Refresh("");
    }
}
