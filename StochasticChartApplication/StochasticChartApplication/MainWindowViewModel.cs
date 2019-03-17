using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using StochasticUi.ViewModel;

namespace StochasticChartApplication
{
    public class MainWindowViewModel : BindableBase
    {
        public MainWindowViewModel(EventDensityViewModel eventsViewModel)
        {
            EventDensityViewModel = eventsViewModel;
            CalculateSourceCommand = new DelegateCommand(CalculateImage);
        }

        public ICommand CalculateSourceCommand { get; }

        public EventDensityViewModel EventDensityViewModel { get; }

        private void CalculateImage()
        {
            var randNum = new Random();
            var data = Enumerable
                .Range(0, StatisticRender.IMAGE_HEIGHT)
                .Select(i => randNum.NextDouble())
                .ToArray();
            EventDensityViewModel.ImageSource = StatisticRender.RenderData(data);
        }
    }
}
