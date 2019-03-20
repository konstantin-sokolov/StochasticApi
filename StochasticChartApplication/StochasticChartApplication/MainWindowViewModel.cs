using System;
using System.Windows.Input;
using EventsApi.Contracts;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using StochasticUi.ViewModel;

namespace StochasticChartApplication
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly int _dataSize = 1000 * 100;

        public MainWindowViewModel(EventDensityViewModel eventsViewModel)
        {
            EventDensityViewModel = eventsViewModel;
            GenerateDataCommand = new DelegateCommand(GenerateData);
        }

        public ICommand GenerateDataCommand { get; }

        public EventDensityViewModel EventDensityViewModel { get; }

        private void GenerateData()
        {
            var random = new Random();
            var result = new double[_dataSize];
            for (int i = 0; i < _dataSize; i++)
                result[i] = random.NextDouble();

            EventDensityViewModel.ImageSource = StatisticRender.RenderData(result);
        }
    }
}