using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AnnoDesigner
{
    /// <summary>
    /// Provides I/O methods
    /// </summary>
    public static class DataIO
    {
        #region Render to file

        /// <summary>
        /// Renders the given target to an image file, png encoded.
        /// </summary>
        /// <param name="target">target to be rendered</param>
        /// <param name="filename">output filename</param>
        public static void RenderToFile(FrameworkElement target, string filename)
        {
            // render control
            const int dpi = 96;
            var rtb = new RenderTargetBitmap((int)target.ActualWidth, (int)target.ActualHeight, dpi, dpi, PixelFormats.Default);
            rtb.Render(target);
            // put result into bitmap
            var encoder = Constants.GetExportImageEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            // save file
            using (var file = new FileStream(filename, FileMode.Create))
            {
                encoder.Save(file);
            }
        }

        #endregion       
    }
}