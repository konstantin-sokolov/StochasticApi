using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using EventApi.Models;
using Brushes = System.Drawing.Brushes;
using Color = System.Drawing.Color;
using Pen = System.Drawing.Pen;
using Point = System.Drawing.Point;

namespace StochasticUi.ViewModel.Renders
{
    public static class ChartRender
    {
        public const int IMAGE_WIDTH = 1400;
        public const int IMAGE_HEIGHT = 1000;

        private static readonly Color _paintColor = Color.DeepSkyBlue;

        public static Task<ImageSource> RenderDataAsync(IEnumerable<DensityInfo> densities, long startTicks, long ticksCount, CancellationToken token)
        {
            return Task.Run(() =>
            {
                if (densities == null || !densities.Any())
                    return RenderEmptyData();

                return BaseRender.RenderData(IMAGE_WIDTH, IMAGE_HEIGHT, g => RenderDensityChart(g, densities, startTicks, ticksCount, token));
            }, token);
        }

        private static ImageSource RenderEmptyData()
        {
            return BaseRender.RenderData(IMAGE_WIDTH, IMAGE_HEIGHT, RenderEmptyObject);
        }

        private static void RenderDensityChart(Graphics g, IEnumerable<DensityInfo> densities, long startTicks, long ticksCount, CancellationToken token)
        {
            var maxDensity = densities.Max(t => t.EventsCount);

            var pen = new Pen(_paintColor);
            var brush = new SolidBrush(_paintColor);
            g.FillRectangle(Brushes.Black, new Rectangle(new Point(0, 0), new Size(IMAGE_WIDTH, IMAGE_HEIGHT)));

            foreach (var density in densities)
            {
                token.ThrowIfCancellationRequested();

                var y = (int)((double) density.EventsCount * IMAGE_HEIGHT / maxDensity);
                var startPosition = (int)Math.Floor((double) (density.Start - startTicks) * IMAGE_WIDTH / ticksCount);
                var endPosition = (int)Math.Ceiling((double)(density.Stop - startTicks) * IMAGE_WIDTH / ticksCount);
                var width = Math.Max(1, endPosition - startPosition);
                if (width == 1)
                    g.DrawLine(pen, startPosition, IMAGE_HEIGHT, startPosition, IMAGE_HEIGHT - y);
                else
                    g.FillRectangle(brush, startPosition, IMAGE_HEIGHT - y, width, y);

            }
        }

        private static void RenderEmptyObject(Graphics g)
        {
            g.FillRectangle(Brushes.Black, new Rectangle(new Point(0, 0), new Size(IMAGE_WIDTH, IMAGE_HEIGHT)));
        }
    }
}
