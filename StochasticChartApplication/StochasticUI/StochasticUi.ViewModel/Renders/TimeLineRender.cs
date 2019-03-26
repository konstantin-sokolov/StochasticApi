using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using Color = System.Drawing.Color;
using FontFamily = System.Drawing.FontFamily;
using FontStyle = System.Drawing.FontStyle;


namespace StochasticUi.ViewModel.Renders
{
    public class TimeLineRender: BaseRender,ITimeLineRender
    {
        private const int TIME_LINE_MARGIN = 40;
        private const int TEXT_WIDTH = 80;
        private const int TIMELINE_HEIGHT = 50;

        public Task<ImageSource> RenderDataAsync(double imageWidth, long startTicks, long ticksCounts, CancellationToken token)
        {
            return Task.Run(() =>
            {
                var intImageWidth = (int) imageWidth;
                return RenderImage(intImageWidth, TIMELINE_HEIGHT, g => DrawTimeLine(g, intImageWidth, startTicks, ticksCounts,token));
            }, token);
        }

        private static void DrawTimeLine(Graphics g, int imageWidth, long startTicks, long ticksCounts, CancellationToken token)
        {
            for (var x = TIME_LINE_MARGIN; x < imageWidth; x += TEXT_WIDTH)
            {
                token.ThrowIfCancellationRequested();

                var font = new Font(FontFamily.GenericSerif, 14, FontStyle.Bold, GraphicsUnit.Point);
                var positionInTicks = (double) x * ticksCounts / imageWidth + startTicks;
                var timeText = GetTime((long) positionInTicks, ticksCounts);
                var textWidth = g.MeasureString(timeText, font).Width;
                g.DrawString(timeText, font, new SolidBrush(Color.Red), new PointF(x - textWidth/2, 0));
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
