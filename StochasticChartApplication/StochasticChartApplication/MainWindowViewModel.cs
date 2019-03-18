using System.Windows.Input;
using EventsApi.Contracts;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using StochasticUi.ViewModel;

namespace StochasticChartApplication
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IEventApi _eventApi;
        private readonly int _dataSize = 1000 * 1000 * 10;

        public MainWindowViewModel(EventDensityViewModel eventsViewModel, IEventApi eventApi)
        {
            _eventApi = eventApi;
            EventDensityViewModel = eventsViewModel;
            GenerateDataCommand = new DelegateCommand(GenerateData);
        }

        public ICommand GenerateDataCommand { get; }

        public EventDensityViewModel EventDensityViewModel { get; }

        private void GenerateData()
        {
            //var result = new PayloadEvent[DATA_SIZE];
            //for (int i = 0; i < DATA_SIZE; i++)
            //    result[i] = new PayloadEvent(i % 2 == 0 ? EventType.start : EventType.stop, i);

            //_eventApi.LoadData(result);
            //EventDensityViewModel.StartDraw();
            ////EventDensityViewModel.ImageSource = StatisticRender.RenderData(data);
        }
    }
}