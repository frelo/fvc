//css_ref Microsoft.WindowsAPICodePack;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using System.Collections.Generic;
using System.Runtime;
using System;
using VideoCataloger;

// on properties for windows files: https://docs.microsoft.com/sv-se/windows/win32/properties/core-bumper

namespace VideoCataloger
{
  public class WriteTags
  {
    static public void Run(IScripting scripting, string argument)
    {
        scripting.GetConsole().Clear();
        var catalog = scripting.GetVideoCatalogService();
        ISelection selection = scripting.GetSelection();
        List<long> selected = selection.GetSelectedVideos();
        foreach (long video in selected)
        {
          try
          {
            var entry = catalog.GetVideoFileEntry(video);

            IUtilities utilities = scripting.GetUtilities();
            var selected_path = utilities.ConvertToLocalPath(entry.FilePath);

            var file = ShellFile.FromFilePath(selected_path );

            var selected_videos = new long[1];
            selected_videos[0] = video;
            var TagInstances = catalog.GetTagsForVideos(selected_videos);
            List<string> tag_list = new List<string>();
            foreach (var tag in TagInstances)
            {
              tag_list.Add( tag.Name );
            }

            scripting.GetConsole().Write( "Tagging : " + selected_path + " ..." );
            ShellPropertyWriter propertyWriter =  file.Properties.GetPropertyWriter();
            propertyWriter.WriteProperty(SystemProperties.System.Keywords, tag_list.ToArray() );
            int Rating = 0;
            if (entry.Rating==1)
              Rating = 1;
            if (entry.Rating==2)
              Rating = 25;
            if (entry.Rating==3)
              Rating = 50;
            if (entry.Rating==4)
              Rating = 75;
            if (entry.Rating==5)
              Rating = 99;
            propertyWriter.WriteProperty(SystemProperties.System.Rating, Rating );
            propertyWriter.WriteProperty(SystemProperties.System.Comment, entry.Description );
            propertyWriter.Close();

            scripting.GetConsole().WriteLine( "Done " );
          }
          catch (Exception ex)          
          {
            scripting.GetConsole().WriteLine( ex.Message );
          }
        }       
    }
  }
}
