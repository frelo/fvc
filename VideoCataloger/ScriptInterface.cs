using System.Collections.Generic;
using System.Threading.Tasks;

namespace VideoCataloger
{
    ///<summary>
    ///Root of script interface to FVC. An object that implements this interface is passed to the run function.
    ///</summary>
    public interface IScripting
    {
        ///<summary>Get selection state object. This object is used to get the get and set the current selection int the video catalog.</summary>
        ///<returns>An object that implements the ISelection interface.</returns>
        ISelection GetSelection();

        ///<summary>Get the console interface used for printing text during script execution.</summary>
        ///<returns>An object that implements the IConsole interface.</returns>
        IConsole GetConsole();

        ///<summary>Get the current video catalog interface. Note that this interface is implemented in another namespace "VideoCataloger.RemoteCatalogService" 
        ///</summary>
        ///<returns>An object that implements the IVideoCatalogService interface.</returns>
        RemoteCatalogService.IVideoCatalogService GetVideoCatalogService();

        ///<summary>Get the windows interface for controlling windows.</summary>
        ///<returns>An object that implements the IConsole interface.</returns>
        IGUI GetGUI();

        ///<summary>Get the video indexer interface.</summary>
        ///<returns>An object that implements the IVideoIndexer interface.</returns>
        IVideoIndexer GetVideoIndexer();

        ///<summary>Get the utilities interface.</summary>
        ///<returns>An object that implements the IUtilities interface.</returns>
        IUtilities GetUtilities();
    };

    ///<summary>
    ///Current selection in the application.
    ///</summary>
    public interface ISelection
    {
        ///<summary>Get a list of the currently selected video ids.</summary>
        ///<returns>A list with video ids of the currently selected videos in the catalog window. An emty list if there is no selection.</returns>
        List<long> GetSelectedVideos();
        ///<summary>Set the selected videos in the program.</summary>
        ///<param name="new_selection">List with video ids</param>  
        void SetSelectedVideos(List<long> new_selection);
        ///<summary>Select one v ideo in the catalog window.</summary>
        ///<param name="video_id">Video id for the video to be selected</param>  
        void SetSelectedVideo(long video_id);
        ///<summary>Set no selected video in the video catalog window.</summary>
        Task SetNoSelectedVideo();

        ///<summary>Get the currently selected playlist. null if no selected playlist</summary>
        RemoteCatalogService.VideoPlaylist GetSelectedPlaylist();
        ///<summary>Set the currently selected playlist.</summary>
        ///<param name="playlist_id">Playlist id for the playlist to be selected</param>  
        void SetSelectedPlaylist(long playlist_id);
        ///<summary>Set the currently selected playlist clip assuming a playlist is selected.</summary>
        ///<param name="clip_id">clip to select</param>  
        void SetSelectedPlaylistClip(int clip_id);

        ///<summary>Get the currently selected playlist. null if no selected playlist</summary>
        RemoteCatalogService.Bin GetSelectedBin();
        ///<summary>Set the currently selected playlist.</summary>
        ///<param name="bin_id">Playlist id for the playlist to be selected</param>  
        void SetSelectedBin(long bin_id);

        ///<summary>Get the currently selected actor. null if no selected actor</summary>
        RemoteCatalogService.Actor GetSelectedActor();

        ///<summary>Set the currently selected playlist.</summary>
        ///<param name="actor_id">actor id for the selected actor</param>  
        void SetSelectedActor(long actor_id);
    }


    ///<summary>
    ///Console interface. This is the interface for scripts.
    ///Use this interface to run new scripts or to print output from scripts to the
    ///console window.
    ///</summary>
    public interface IConsole
    {
        /// <summary>
        /// Write a line of text to the console window
        /// </summary>
        /// <param name="line">Text to be output</param>
        void WriteLine(string line);

        /// <summary>
        /// Write text to the console window
        /// </summary>
        /// <param name="line">Text to be written</param>
        void Write(string line);

        /// <summary>
        /// Clear the console window from any text.
        /// </summary>
        void Clear();

        /// <summary>
        /// Load,compile and run a script by calling its main function with the provided arguments
        /// Errors are outputed to the console window.
        /// The function will return when the script has finished execution.
        /// </summary>
        /// <param name="script">Path to the script that will be run</param>
        /// <param name="arguments">Arguments to pass to the script</param>
        void RunScript(string script, string arguments);

        // Load,compile and run a script by calling its main function with the provided arguments
        /// <summary>
        /// Run a script without extra arguments. Errors are outputted to the console window.
        /// </summary>
        /// <param name="script">Path to the script to be run.</param>
        void RunScript(string script);
    }

    ///<summary>
    /// Program user interface. This is the interface for scripts.
    /// Use this interface to refresh the user interface after updating the catalog
    ///</summary>
    public interface IGUI
    {
        /// <summary>
        /// Refresh all windows
        /// </summary>
        /// <param name="hint">Hint on what to refresh, currently ignored</param>
        void Refresh(string hint);
    }

    ///<summary>
    /// Exposed utilities in fast video cataloger
    ///</summary>
    public interface IUtilities
    {
        /// <summary>
        /// Mask a video file given its id in the catalog
        /// </summary>
        /// <param name="video_id">id of video file</param>
        void Mask( long video_id );

        /// <summary>
        /// Unmask a video file given its id in the catalog
        /// </summary>
        /// <param name="video_id">id of video file</param>
        void Unmask(long video_id);

        /// <summary>
        /// Convert a path to a local path, replacing known [folder] folders given the 
        /// current settings for the local computer i.e make the path an absolut path. 
        /// If there is no need for conversion the same path will be returned.
        /// </summary>
        /// <param name="path">path to convert</param>
        string ConvertToLocalPath(string path);

        /// <summary>
        /// Convert a path from a local path, i.e. the opposite of ConvertToLocalPath
        /// </summary>
        /// <param name="path">path to convert</param>
        string ConvertFromLocalPath(string path);
    }

    ///<summary>
    /// VideoIndexer interface.
    /// Use this interface to queue index requests
    ///</summary>
    public interface IVideoIndexer
    {
        /// <summary>
        /// Capture an a frame at the provided time. This function will queue a capture of a given frame if a capture is in progress.
        /// If no capture is in progress it will queue the request and start working on the queue.
        /// </summary>
        /// <param name="video_id">Id of video to capture from</param>
        /// <param name="current_video_play_pos">Seconds from the start where we want to capture</param>
        void CaptureSingleFrame(long video_id, double current_video_play_pos);

        /// <summary>
        /// Capture an a frame at the provided time to a jpeg file. This function will queue a capture of a given frame if a capture is in progress.
        /// </summary>
        /// <param name="video_id">Id of video to capture from</param>
        /// <param name="current_video_play_pos">Seconds from the start where we want to capture</param>
        /// <param name="output_file">Path to where to save the file.</param>
        void CaptureSingleFrameToFile(long video_id, double current_video_play_pos, string output_file);

        /// <summary>
        /// Add a video file in the queue to be indexed.
        /// </summary>
        /// <param name="path">Path to the video file to be added. Please use windows paths and remember you need to use // to get a / in a C# string.</param>
        /// <param name="title">optional title of the video file</param>
        /// <param name="url">optional url for the video file.</param>
        /// <param name="desc">option desc for the videoe.</param>
        void AddVideoFile(string path, string title="", string url="", string desc="");

        /// <summary>
        /// Start processing the index queue.
        /// </summary>
        void StartIndexing();
    }

};
