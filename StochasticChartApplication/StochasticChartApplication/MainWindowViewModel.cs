using System;
using System.Windows.Input;
using System.Windows.Threading;
using EventApi.Implementation.DataProviders;
using Generators;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using StochasticUi.ViewModel;
using StochasticUi.ViewModel.GeneratorArgs;

namespace StochasticChartApplication
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly GeneratorFactory _generatorFactory;
        private readonly DensityViewModelFactory _viewModelFactory;
        private readonly long _defaultArrayDataSize = 1000 * 1000 * 10;
        private readonly long _defaultMmfDataSize = 100L * 1000L * 1000L;
        private readonly Dispatcher _uiDispatcher;
        private BaseGeneratorArgsViewModel _activeArgsViewModel;
        private readonly ArrayGeneratorArgsViewModel _arrayGeneratorArgsViewModel;
        private readonly MMFGeneratorArgsViewModel _mmfGeneratorArgsViewModel;
        private ProviderType _providerType;
        private double _progressValue;
        private string _progressStatus;
        private bool _isProcessingData;
        private EventDensityViewModel _eventsDensityViewModel;

        public MainWindowViewModel(GeneratorFactory generatorFactory, DensityViewModelFactory viewModelFactory)
        {
            _uiDispatcher = Dispatcher.CurrentDispatcher;
            _generatorFactory = generatorFactory;
            _viewModelFactory = viewModelFactory;
            GenerateDataCommand = new DelegateCommand(GenerateData);
            SkipSessionCommand = new DelegateCommand(SkipSession);
            _arrayGeneratorArgsViewModel = new ArrayGeneratorArgsViewModel()
            {
                CollectionSize = _defaultArrayDataSize.ToString()
            };
            _mmfGeneratorArgsViewModel = new MMFGeneratorArgsViewModel()
            {
                CollectionSize = _defaultMmfDataSize.ToString(),
                FilePath = "TestData.bin"
            };
            ProviderType = ProviderType.Array;
            ActiveArgsViewModel = _arrayGeneratorArgsViewModel;
        }

        private void GenerateData()
        {
            ProgressValue = 0;
            ProgressStatus = $"Test set generation: {ProgressValue}%";
            IsProcessingData = true;
            var generator = _generatorFactory.GetGenerator(ProviderType);
            generator.EventGenerateProgressChanged += OnEventGenerateProgressChanged;
            generator.GenerateDataProviderAsync(_activeArgsViewModel.GetCollectionSize(), _activeArgsViewModel.GetParameters())
                .ContinueWith(dataProvider =>
                {
                    generator.EventGenerateProgressChanged -= OnEventGenerateProgressChanged;
                    _uiDispatcher.BeginInvoke(new Action(() =>
                    {
                        IsProcessingData = false;
                        EventDensityViewModel = _viewModelFactory.GetViewModel(dataProvider.Result);
                    }));
                });
        }

        private void SkipSession()
        {
            EventDensityViewModel?.Dispose();
            EventDensityViewModel = null;
        }

        private void OnEventGenerateProgressChanged(int progress)
        {
            _uiDispatcher.BeginInvoke(new Action(() =>
            {
                ProgressValue = progress;
                ProgressStatus = $"Test set generation: {progress}%";
            }));
        }

        public ICommand GenerateDataCommand { get; }
        public ICommand SkipSessionCommand { get; }

        public EventDensityViewModel EventDensityViewModel
        {
            get => _eventsDensityViewModel;
            set => SetProperty(ref _eventsDensityViewModel, value);
        }
        public BaseGeneratorArgsViewModel ActiveArgsViewModel
        {
            get => _activeArgsViewModel;
            set => SetProperty(ref _activeArgsViewModel, value);
        }
        public ProviderType ProviderType
        {
            get => _providerType;
            set
            {
                if (SetProperty(ref _providerType, value))
                {
                    ActiveArgsViewModel = _providerType == ProviderType.Array ? 
                        (BaseGeneratorArgsViewModel)_arrayGeneratorArgsViewModel :
                        (BaseGeneratorArgsViewModel)_mmfGeneratorArgsViewModel;
                }
            }
        }

        public bool IsProcessingData
        {
            get => _isProcessingData;
            set => SetProperty(ref _isProcessingData, value);
        }
        public double ProgressValue
        {
            get => _progressValue;
            set => SetProperty(ref _progressValue, value);
        }
        public string ProgressStatus
        {
            get => _progressStatus;
            set => SetProperty(ref _progressStatus, value);
        }
    }
}