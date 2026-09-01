using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VideoCataloger;
using VideoCataloger.RemoteCatalogService;

// Find scenes (thumbnails) that contain faces in the selected videos and select them in
// the video scenes window.
//
// Demonstrates face detection on catalog thumbnails with IScripting.GetFaceRecognition
// and thumbnail selection with ISelection.SetSelectedThumbnails. See the LearnActorFaces
// sample for the actor learning side of the face recognition API.
class Script
{
    static public async System.Threading.Tasks.Task Run(IScripting scripting, string arguments)
    {
        IConsole console = scripting.GetConsole();
        IGUI gui = scripting.GetGUI();
        ISelection selection = scripting.GetSelection();
        var catalog = scripting.GetVideoCatalogService();
        console.Clear();

        // The face engine is initialized on demand; null means it is not available.
        var face_service = scripting.GetFaceRecognition();
        if (face_service == null)
        {
            console.WriteLine("Face recognition is not initialized. Download the face models (AI settings) and try again.");
            return;
        }

        List<long> selected = selection.GetSelectedVideos();
        if (selected.Count == 0)
        {
            console.WriteLine("No videos selected.");
            return;
        }

        gui.SetCancelSupported(true);

        var thumbnails_with_faces = new List<long>();
        int scanned = 0;
        for (int i = 0; i < selected.Count; ++i)
        {
            if (gui.IsCancelRequested())
                break;
            gui.SetProgress(i, 0, selected.Count, "Scanning video " + selected[i] + " for faces");

            long video_id = selected[i];
            Dictionary<long, ThumbnailEntry> thumbnails =
                await Task.Run(() => catalog.GetThumbnailsForVideo(video_id, true));

            foreach (var pair in thumbnails)
            {
                if (gui.IsCancelRequested())
                    break;
                if (pair.Value.Image == null)
                    continue;

                // Fast HOG detector; pass true instead for the slower but more accurate
                // CNN detector.
                int face_count = await Task.Run(() => face_service.DetectFaceCount(pair.Value.Image, false));
                scanned++;
                if (face_count > 0)
                    thumbnails_with_faces.Add(pair.Key);
            }
        }

        if (thumbnails_with_faces.Count > 0)
            selection.SetSelectedThumbnails(thumbnails_with_faces);

        gui.SetProgress(selected.Count, 0, selected.Count, "Face scan finished");
        console.WriteLine("Done. " + thumbnails_with_faces.Count + " of " + scanned + " scanned thumbnails contain faces.");
    }
}
