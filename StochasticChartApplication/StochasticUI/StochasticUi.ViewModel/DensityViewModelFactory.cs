using Autofac;
using EventsApi.Contracts;
using EventsApi.Contracts.DataProviders;
using EventsApi.Contracts.EventApi;
using NLog;
using StochasticUi.ViewModel.Renders;
using StochasticUi.ViewModel.Scale;

namespace StochasticUi.ViewModel
{
    public class DensityViewModelFactory
    {
        private readonly IContainer _container;

        public DensityViewModelFactory(IContainer container)
        {
            _container = container;
        }

        public EventDensityViewModel GetViewModel(IDataProvider provider)
        {
            var scaler = _container.Resolve<IScaler>(
                new NamedParameter("globalStart", provider.GetGlobalStartTick()),
                new NamedParameter("globalStop", provider.GetGlobalStopTick()));
            var densityApi = _container.Resolve<IDensityApi>(new NamedParameter("dataProvider", provider));
            var logger = _container.Resolve<ILogger>();
            var timeLineRender = _container.Resolve<ITimeLineRender>();
            var chartRender = _container.Resolve<IChartRender>();
            return new EventDensityViewModel(scaler, densityApi, logger, timeLineRender, chartRender);
        }
    }
}
