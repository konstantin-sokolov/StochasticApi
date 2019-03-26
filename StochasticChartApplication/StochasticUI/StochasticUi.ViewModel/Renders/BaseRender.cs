using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StochasticUi.ViewModel.Renders
{
    public abstract class BaseRender
    {
        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        protected ImageSource RenderImage(int imageWidth, int imageHeight, Action<Graphics> renderObject)
        {
            BitmapSource bmpSource;
            using (var bmp = new Bitmap(imageWidth, imageHeight))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    renderObject(g);
                }

                var hBitmap = bmp.GetHbitmap();

                try
                {
                    bmpSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        hBitmap,
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromWidthAndHeight(bmp.Width, bmp.Height));
                }
                finally
                {
                    DeleteObject(hBitmap);
                }
            }

            bmpSource.Freeze();
            return bmpSource;
        }
    }
}
