using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Practices.Prism.Mvvm;
using FontFamily = System.Drawing.FontFamily;

namespace StochasticUi.ViewModel
{
    public class TimeLineViewModel:BindableBase
    {
        private const int TEXT_MARGIN = 30;
        private const int TEXT_WIDTH = 30;
        private int _currentWidth;
        private int _currentHeight;

        protected Canvas PaintCanvas { get; private set; }

        public int CurrentWidth
        {
            get => _currentWidth;
            set => SetProperty(ref _currentWidth, value);
        }

        public int CurrentHeight
        {
            get => _currentHeight;
            set => SetProperty(ref _currentHeight, value);
        }

        protected void Render()
        {
            if (PaintCanvas == null)
                return;

            PaintCanvas.Children.Clear();

            for (var i = TEXT_MARGIN; i < CurrentWidth - TEXT_MARGIN; i += TEXT_WIDTH)
            {
                var x = i;
                if (x > 0)
                {
                    var text = new TextBlock
                    {
                        Text = GetTime(i),
                        FontFamily = new System.Windows.Media.FontFamily("Arial"),
                        FontSize = 9,
                        Foreground = Brushes.Black
                    };
                    TextOptions.SetTextFormattingMode(text, TextFormattingMode.Display);

                    var line = new Line();
                    line.Stroke = Brushes.Black;
                    line.StrokeThickness = 1;
                    line.X1 = x;
                    line.X2 = x;
                    line.Y1 = CurrentHeight * (4 / (double)5);
                    line.Y2 = CurrentHeight;

                    Canvas.SetTop(text, 5);
                    var w = (int)TextHelper.MeasureStringMaxCached(text.Text, text).Width;
                    Canvas.SetLeft(text, x - w / 2);
                    PaintCanvas.Children.Add(line);
                    PaintCanvas.Children.Add(text);
                }
            }
        }
        private string GetTime(double position)
        {
            var duration = 0;//CurrentWidth > 0 ? (position * DurationProvider.Duration) / CurrentWidth : 0;
            var ts = TimeSpan.FromSeconds(duration);

            return GetTimeString(ts.Seconds, ts);
        }

        private string GetTimeString(double rangeDuration, TimeSpan timeSpan)
        {
            if (rangeDuration < 60)
                return timeSpan.ToString(@"mm\:ss\.fff");
            return timeSpan.ToString(@"hh\:mm\:ss");
        }
    }
}
