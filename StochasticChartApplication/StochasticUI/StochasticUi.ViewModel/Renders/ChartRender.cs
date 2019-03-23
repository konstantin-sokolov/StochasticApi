using System.Drawing;
using System.Windows.Media;
using Brushes = System.Drawing.Brushes;
using Color = System.Drawing.Color;
using Pen = System.Drawing.Pen;
using Point = System.Drawing.Point;

namespace StochasticUi.ViewModel.Renders
{
    internal static class ChartRender
    {
        public const int IMAGE_WIDTH = 1400;
        public const int IMAGE_HEIGHT = 1000;

        private static readonly Color _paintColor = Color.DeepSkyBlue;

        public static ImageSource RenderData(double[] densities)
        {
            return BaseRender.RenderData(IMAGE_WIDTH, IMAGE_HEIGHT, g => RenderObject(g, densities));
        }

        public static ImageSource RenderEmptyData()
        {
            return BaseRender.RenderData(IMAGE_WIDTH, IMAGE_HEIGHT, RenderEmptyObject);
        }

        private static void RenderObject(Graphics g, double[] densities)
        {
            var pen = new Pen(_paintColor);
            var brush = new SolidBrush(_paintColor);
            g.FillRectangle(Brushes.Black, new Rectangle(new Point(0, 0), new Size(IMAGE_WIDTH, IMAGE_HEIGHT)));
            double width = 1;
            if (densities.Length < IMAGE_WIDTH)
                width = (double) IMAGE_WIDTH / densities.Length;

            double x = 0;
            foreach (var density in densities)
            {
                if (density >= 0)
                {
                    var y = (int) (density * IMAGE_HEIGHT);

                    if (width <= 1)
                        g.DrawLine(pen, (int) x, IMAGE_HEIGHT, (int) x, IMAGE_HEIGHT - y);
                    else
                        g.FillRectangle(brush, (int) x, IMAGE_HEIGHT - y, (int) width + 1, y);
                }

                x += width;
            }
        }

        private static void RenderEmptyObject(Graphics g)
        {
            g.FillRectangle(Brushes.Black, new Rectangle(new Point(0, 0), new Size(IMAGE_WIDTH, IMAGE_HEIGHT)));
        }
    }
}
