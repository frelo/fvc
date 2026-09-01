using System;
using VideoCataloger;

// Capture the frame at the current playback position in the video player and add it as a
// new thumbnail for the video.
//
// Demonstrates the IVideoPlayer interface (play position, pause), IGUI.IsWindowOpened and
// IVideoIndexer.CaptureSingleFrame.
class Script
{
    static public async System.Threading.Tasks.Task Run(IScripting scripting, string arguments)
    {
        IConsole console = scripting.GetConsole();
        IGUI gui = scripting.GetGUI();
        console.Clear();

        if (!gui.IsWindowOpened("Player"))
        {
            console.WriteLine("The video player window is not open.");
            return;
        }

        IVideoPlayer player = scripting.GetVideoPlayer();
        long video_id = player.GetSelectedVideoID();
        if (video_id <= 0)
        {
            console.WriteLine("No video selected in the video player.");
            return;
        }

        // Pause so the user sees exactly which frame is captured.
        player.PauseNoToggleMovie();
        double position = player.GetPlayPosition();

        scripting.GetVideoIndexer().CaptureSingleFrame(video_id, position);
        console.WriteLine("Capturing frame at " + TimeSpan.FromSeconds(position).ToString(@"hh\:mm\:ss") + ".");
    }
}
