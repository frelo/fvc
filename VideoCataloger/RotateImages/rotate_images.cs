#region samples_rotate_images

using System.Collections.Generic;
using System.Runtime;
using System.IO;        // For memorystream
using System.Drawing;   // for bitmap
using System.Drawing.Imaging;
using VideoCataloger;
using VideoCataloger.RemoteCatalogService;

/// <summary>
///  This sample shows how to do image manipulation on the thumbnails of a video
/// </summary>
public class Script
{
    static private ImageCodecInfo GetEncoder(ImageFormat format)
    {
        ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
        foreach (ImageCodecInfo codec in codecs)
        {
            if (codec.FormatID == format.Guid)
            {
                return codec;
            }
        }
        return null;
    }

    /// <summary>
    ///  Run sample. This is the entry function called by fvc.
    /// </summary>
    static public async System.Threading.Tasks.Task Run(IScripting scripting, string argument)
    {
        ISelection selection = scripting.GetSelection();
        var catalog = scripting.GetVideoCatalogService();
        List<long> selected = selection.GetSelectedVideos();
        ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
        EncoderParameter jpeg_param = new EncoderParameter(Encoder.Quality, 85L);
        EncoderParameters jpeg_params = new EncoderParameters(1);
        jpeg_params.Param[0] = jpeg_param;

        foreach (long video_id in selected )
        {
            Dictionary<long, ThumbnailEntry > all_images = catalog.GetThumbnailsForVideo( video_id, true );

            long frame_no = 0;
            foreach ( var pair in all_images )
            {
                MemoryStream stream = new MemoryStream(pair.Value.Image);
                System.Drawing.Image image = System.Drawing.Bitmap.FromStream(stream);
                image.RotateFlip(RotateFlipType.Rotate90FlipNone);

                MemoryStream out_stream = new MemoryStream();
                image.Save(out_stream, jpgEncoder, jpeg_params);
                pair.Value.Image = out_stream.ToArray();

                pair.Value.FrameNo = frame_no++;
                catalog.AddVideoThumbnail(pair.Value);
                catalog.DeleteThumbnail(pair.Key);
                stream.Close();
                out_stream.Close();
                image.Dispose();
            }

            scripting.GetGUI().Refresh("");
        }
    }


    }

#endregion
