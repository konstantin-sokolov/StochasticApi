using System;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Brushes = System.Drawing.Brushes;
using Color = System.Drawing.Color;
using FontFamily = System.Drawing.FontFamily;
using FontStyle = System.Drawing.FontStyle;
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
        private const int TIME_LINE_HEIGHT = 50;
        private const int TIME_LINE_MARGIN = 30;
        private const int TEXT_WIDTH = 80;
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
                    double width = 1;
                    if (densities.Length < IMAGE_WIDTH)
                        width = (double)IMAGE_WIDTH / densities.Length;

                    double x = 0;
                    foreach (var density in densities)
                    {
                        if (density >= 0)
                        {
                            var y = (int)(density * IMAGE_HEIGHT);

                            if (width <= 1)
                                g.DrawLine(pen, (int)x, IMAGE_HEIGHT, (int)x, IMAGE_HEIGHT - y - TIME_LINE_HEIGHT);
                            else
                                g.FillRectangle(brush, (int)x, IMAGE_HEIGHT - y, (int)width + 1, y - TIME_LINE_HEIGHT);
                        }
                        x += width;
                    }

                    DrawTimeLine(g);

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

        private static void DrawTimeLine(Graphics g)
        {
            for (var i = TIME_LINE_MARGIN; i < IMAGE_WIDTH - TIME_LINE_MARGIN; i += TEXT_WIDTH)
            {
                var x = i;
                if (x > 0)
                {
                    var font = new Font(FontFamily.GenericSerif, 14, FontStyle.Bold, GraphicsUnit.Point);
                    var timeText = GetTime(x);
                    var textWidth = g.MeasureString(timeText, font).Width;
                    g.DrawString(timeText, font, new SolidBrush(Color.Red), new PointF(x - textWidth, IMAGE_HEIGHT - TIME_LINE_HEIGHT));
                }
            }
        }
        private static string GetTime(double position)
        {
            var duration = 20;//CurrentWidth > 0 ? (position * DurationProvider.Duration) / CurrentWidth : 0;
            var ts = TimeSpan.FromSeconds(duration);

            return GetTimeString(ts.Seconds, ts);
        }

        private static string GetTimeString(double rangeDuration, TimeSpan timeSpan)
        {
            if (rangeDuration < 1)
                return $"{timeSpan.Milliseconds}m";

            if (rangeDuration < 60)
                return $"{timeSpan.Seconds}s";

            return timeSpan.ToString(@"mm\:ss");
        }
    }
}
