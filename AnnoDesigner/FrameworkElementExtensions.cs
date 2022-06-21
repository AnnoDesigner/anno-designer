using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AnnoDesigner.Core.Extensions
{
    /// <summary>
    /// Provides extension methods for framework elements to render it to multiple targets.
    /// </summary>
    public static class FrameworkElementExtensions
    {
        /// <summary>
        /// Renders the given target to a bitmap.
        /// </summary>
        /// <param name="target">target to be rendered</param>
        /// <returns>Bitmap containing rendered framework element</returns>
        public static RenderTargetBitmap RenderToBitmap(this FrameworkElement target)
        {
            const int dpi = 96;
            var rtb = new RenderTargetBitmap((int)target.ActualWidth, (int)target.ActualHeight, dpi, dpi, PixelFormats.Default);
            rtb.Render(target);

            return rtb;
        }

        /// <summary>
        /// Renders the given target to an image file, png encoded.
        /// </summary>
        /// <param name="target">target to be rendered</param>
        /// <param name="filename">output filename</param>
        public static void RenderToFile(this FrameworkElement target, string filename)
        {
            var rtb = target.RenderToBitmap();
            // put result into bitmap
            var encoder = Constants.GetExportImageEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            // save file
            using (var file = new FileStream(filename, FileMode.Create))
            {
                encoder.Save(file);
            }
        }
    }
}