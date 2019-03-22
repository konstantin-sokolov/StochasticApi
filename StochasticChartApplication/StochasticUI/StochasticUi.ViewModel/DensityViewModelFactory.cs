using Autofac;
using EventApi.Implementation.DataProviders;
using EventsApi.Contracts;
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
            var scaler = _container.Resolve<IScaler>();
            var densityApi = _container.Resolve<IDensityApi>(new NamedParameter("dataProvider", provider));
            return new EventDensityViewModel(scaler, densityApi);
        }
    }
}
