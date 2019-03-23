using System;
using System.Drawing;
using System.Windows.Media;
using Color = System.Drawing.Color;
using FontFamily = System.Drawing.FontFamily;
using FontStyle = System.Drawing.FontStyle;


namespace StochasticUi.ViewModel.Renders
{
    public static class TimeLineRender
    {
        private const int TIME_LINE_MARGIN = 30;
        private const int TEXT_WIDTH = 80;
        private const int TIMELINE_HEIGHT = 50;

        public static ImageSource RenderData(double imageWidth,long startTicks, long ticksCounts)
        {
            var intImageWidth = (int) imageWidth;
            return BaseRender.RenderData(intImageWidth, TIMELINE_HEIGHT, g => DrawTimeLine(g, intImageWidth, startTicks, ticksCounts));
        }

        private static void DrawTimeLine(Graphics g, int imageWidth, long startTicks, long ticksCounts)
        {
            for (var i = TIME_LINE_MARGIN; i < imageWidth - TIME_LINE_MARGIN; i += TEXT_WIDTH)
            {
                var x = i;
                if (x > 0)
                {
                    var font = new Font(FontFamily.GenericSerif, 14, FontStyle.Bold, GraphicsUnit.Point);
                    var positionInTicks = (double) x * ticksCounts / imageWidth + startTicks;
                    var timeText = GetTime((long) positionInTicks, ticksCounts);
                    var textWidth = g.MeasureString(timeText, font).Width;
                    g.DrawString(timeText, font, new SolidBrush(Color.Red), new PointF(x - textWidth, 0));
                }
            }
        }

        private static string GetTime(long position, long ticksCounts)
        {
            var ts = TimeSpan.FromTicks(position);
            var rangeDuration = TimeSpan.FromTicks(ticksCounts);
            return GetTimeString(rangeDuration, ts);
        }

        private static string GetTimeString(TimeSpan rangeDuration, TimeSpan timeSpan)
        {
            if (rangeDuration.TotalSeconds < 1)
                return $"{timeSpan.Milliseconds}m";

            if (rangeDuration.TotalSeconds < 60)
                return $"{timeSpan.Seconds}s";

            return timeSpan.ToString(@"mm\:ss");
        }
    }
}
