using System;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Brushes = System.Drawing.Brushes;
using Color = System.Drawing.Color;
using Pen = System.Drawing.Pen;
using Point = System.Drawing.Point;

namespace StochasticUi.ViewModel
{
    public static class StatisticRender
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        public const int IMAGE_WIDTH = 1400;
        public const int IMAGE_HEIGHT = 1000;
        private static Color _paintColor = Color.DeepSkyBlue;

        public static ImageSource RenderData(double[] densities)
        {
            BitmapSource bmpSource;
            using (var bmp = new Bitmap(IMAGE_WIDTH, IMAGE_HEIGHT))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    var pen = new Pen(_paintColor);
                    var brush = new SolidBrush(_paintColor);
                    g.FillRectangle(Brushes.Black, new Rectangle(new Point(0, 0), bmp.Size));
                    var width = 1;
                    if (densities.Length < IMAGE_WIDTH)
                        width = IMAGE_WIDTH / densities.Length;

                    double x = 0;
                    foreach (var density in densities)
                    {
                        if (density >= 0)
                        {
                            var y = (int) (density * IMAGE_HEIGHT);

                            if (width <= 1)
                                g.DrawLine(pen, (int) x, IMAGE_HEIGHT, (int) x, IMAGE_HEIGHT - y);
                            else
                                g.FillRectangle(brush, (int) x, IMAGE_HEIGHT - y, width + 1, y);
                        }

                        x += width;
                    }

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
