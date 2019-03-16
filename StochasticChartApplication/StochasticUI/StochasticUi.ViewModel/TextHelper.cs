using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StochasticUi.ViewModel
{
    public class TextHelper
    {
        static int _stringMaxSize;
        static Size _size;
        public static Size MeasureStringMaxCached(string candidate, TextBlock textBlock)
        {
            if (candidate.Length > _stringMaxSize)
            {
                var formattedText = new FormattedText(
                    candidate,
                    CultureInfo.CurrentUICulture,
                    FlowDirection.LeftToRight,
                    new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch),
                    textBlock.FontSize,
                    Brushes.Black);


                _size = new Size(formattedText.Width, formattedText.Height);
                _stringMaxSize = candidate.Length;
            }
            return _size;
        }
    }
}
