using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using StochasticUi.ViewModel;

namespace TestApplication
{
    public class MainWindowViewModel : BindableBase
    {
        private ImageSource _imageSource;
        public MainWindowViewModel()
        {
            CalculateSourceCommand = new DelegateCommand(CalculateImage);
        }

        public ICommand CalculateSourceCommand { get; }

        public ImageSource ImageSource
        {
            get => _imageSource;
            set => SetProperty(ref _imageSource, value);
        }

        private void CalculateImage()
        {
            var randNum = new Random();
            var data = Enumerable
                .Range(0, StatisticRender.IMAGE_HEIGHT)
                .Select(i => randNum.NextDouble())
                .ToArray();
            ImageSource = StatisticRender.RenderData(data);
        }

        public void ChangeScale(int eDelta, Point getPosition)
        {
            Trace.WriteLine("Test");
        }
    }
}
