#region samples_filter_to_bin

using System.Runtime;
using VideoCataloger;
using VideoCataloger.RemoteCatalogService;

/// <summary>
///  Filter the entire catalog and put the matching videos in a bin.
///  Write your filter criteria in the IsVideoPassingFilter() function
/// </summary>
public class FilterToBin
{
    IScripting m_Scripting;
    string m_BinLabel;

    public FilterToBin(IScripting scripting)
    {
        m_Scripting = scripting;
    }

    public void SetBinTarget(string bin_label)
    {
        m_BinLabel = bin_label;
    }

    public void Filter()
    {
        var catalog = m_Scripting.GetVideoCatalogService();

        VideoQuery query = new VideoQuery();
        VideoFileEntry[] videos = catalog.SearchVideos(query);
        if (videos == null)
            return;

        long bin_id = CreateBin();

        foreach (VideoFileEntry entry in videos)
        {
            if (IsVideoPassingFilter(entry))
                catalog.AddVideoToBin(entry.ID, bin_id);
        }
    }

    private bool IsVideoPassingFilter( VideoFileEntry entry )
    {
        // Here we can do our advanced filtering. 
        // In our example filter out any video shorter than 60 seconds
        // you can ofcource get extended properties for the video and filter on that
        // do analysis of thumbnails in the video, scan a video for facial recognition etc.
        if ( entry.LengthSeconds < 60)
            return true;
        return false;
    }

    private long CreateBin()
    {
        var catalog = m_Scripting.GetVideoCatalogService();
        Bin[] all_bins = catalog.GetAllBins();
        foreach (Bin bin in all_bins)
        {
            if (bin.Label == m_BinLabel)
            {
                VideoFileEntry[] videos = catalog.GetVideosInBin(bin.BinID);
                foreach (VideoFileEntry entry in videos)
                {
                    catalog.RemoveVideoFromBin( bin.BinID, entry.ID );
                }

                return bin.BinID;
            }
        }
        // clear the bin first.
        Bin target_bin = catalog.CreateBin(m_BinLabel, -1, 0xffffff);
        return target_bin.BinID;
    }

}

/// <summary>
///  Run sample. This is the entry function called by fvc.
/// </summary>
public class Script
{
    static public async System.Threading.Tasks.Task Run(IScripting scripting, string argument)
    {
        FilterToBin instance = new FilterToBin(scripting);
        instance.SetBinTarget("short videos");
        instance.Filter();
        scripting.GetGUI().Refresh("");
    }
}

#endregion
