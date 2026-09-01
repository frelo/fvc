using System;
using System.Drawing;
using VideoCataloger;

// Install an indexing filter that skips dark frames (black intros, fades) so they never
// end up as thumbnails in the catalog.
//
// Run the script to install the filter - it stays active for everything you index
// afterwards. Optionally pass a folder as the script argument to queue and index it right
// away. Run the script again with the argument "off" to remove the filter.
//
// Demonstrates the IVideoIndexerCallbacks interface: the ProcessFrame callback receives
// every candidate frame as a bitmap during indexing and can veto or alter it before it is
// stored in the catalog. Note that implementing ProcessFrame slows down indexing.
class Script
{
    class DarkFrameFilter : IVideoIndexerCallbacks
    {
        IConsole m_Console;
        int m_SkippedFrames;

        public DarkFrameFilter(IConsole console)
        {
            m_Console = console;
        }

        public bool StartingIndexing(VideoEntry video)
        {
            m_SkippedFrames = 0;
            return true; // return false to skip this video entirely
        }

        public bool ProcessFrame(int frame, double sample_time, ref Bitmap image)
        {
            // Average luminance over a coarse pixel grid. GetPixel is slow, so do not
            // sample every pixel.
            int step_x = Math.Max(1, image.Width / 32);
            int step_y = Math.Max(1, image.Height / 32);
            double luminance = 0;
            int samples = 0;
            for (int y = 0; y < image.Height; y += step_y)
            {
                for (int x = 0; x < image.Width; x += step_x)
                {
                    Color pixel = image.GetPixel(x, y);
                    luminance += 0.299 * pixel.R + 0.587 * pixel.G + 0.114 * pixel.B;
                    samples++;
                }
            }
            luminance /= samples;

            if (luminance < 16) // 0 is black, 255 is white
            {
                m_SkippedFrames++;
                return false; // do not store this frame in the catalog
            }
            return true;
        }

        public void VideoIndexedEnd(VideoEntry captured_video)
        {
            if (m_SkippedFrames > 0)
                m_Console.WriteLine("Skipped " + m_SkippedFrames + " dark frame(s) in " + captured_video.Title);
        }

        public void EndingIndexing()
        {
            m_Console.WriteLine("Indexing queue finished. The dark frame filter is still active.");
        }
    }

    static public async System.Threading.Tasks.Task Run(IScripting scripting, string arguments)
    {
        IConsole console = scripting.GetConsole();
        IVideoIndexer indexer = scripting.GetVideoIndexer();
        console.Clear();

        string argument = (arguments ?? "").Trim().Trim('"');
        if (argument.Equals("off", StringComparison.OrdinalIgnoreCase))
        {
            indexer.SetIndexingCallbacks(null);
            console.WriteLine("Dark frame filter removed.");
            return;
        }

        indexer.SetIndexingCallbacks(new DarkFrameFilter(console));
        console.WriteLine("Dark frame filter installed - frames darker than the threshold are not captured.");
        console.WriteLine("It stays active until you run this script with the argument \"off\" or restart the program.");

        if (argument.Length > 0)
        {
            if (System.IO.Directory.Exists(argument))
            {
                indexer.AddFolder(argument, true, true);
                indexer.StartIndexing();
                console.WriteLine("Queued videos in " + argument + " for indexing.");
            }
            else
            {
                console.WriteLine("Folder not found: " + argument);
            }
        }
    }
}
