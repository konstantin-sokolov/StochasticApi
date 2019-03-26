using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using EventApi.Models;
using EventsApi.Contracts;
using EventsApi.Contracts.EventApi;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using NLog;
using StochasticUi.ViewModel.Renders;
using StochasticUi.ViewModel.Scale;

namespace StochasticUi.ViewModel
{
    public class EventDensityViewModel : BindableBase, IDisposable
    {
        private const int IMAGE_WIDTH = 400;

        private readonly IScaler _scaler;
        private readonly IDensityApi _densityApi;
        private readonly ILogger _logger;
        private readonly ITimeLineRender _timeLineRender;
        private readonly IChartRender _chartRender;
        private ImageSource _chartImageSource;
        private ImageSource _timeLineImageSource;

        private double _currentWidth;
        private Task _swapTask;
        private CancellationTokenSource _calcDensityTokenSource;
        private List<DensityInfo> _currentDensityInfo;
        private Guid _currentInfoCorrelationId;

        public EventDensityViewModel(IScaler scaler, 
            IDensityApi densityApi,
            ILogger logger,
            ITimeLineRender timeLineRender,
            IChartRender chartRender)
        {
            _scaler = scaler;
            _densityApi = densityApi;
            _logger = logger;
            _timeLineRender = timeLineRender;
            _chartRender = chartRender;
            MoveLeftCommand = new DelegateCommand(MoveLeft);
            MoveRightCommand = new DelegateCommand(MoveRight);
            RecalculateWholeImage();
        }

        private void MoveRight()
        {
            _logger.Info("Move chart right");
            _scaler.MoveRight();
            OnPropertyChanged(nameof(CanMoveRight));
            OnPropertyChanged(nameof(CanMoveLeft));
            RecalculateWholeImage();
        }

        private void MoveLeft()
        {
            _logger.Info("Move chart left");
            _scaler.MoveLeft();
            OnPropertyChanged(nameof(CanMoveRight));
            OnPropertyChanged(nameof(CanMoveLeft));
            RecalculateWholeImage();
        }

        private void RecalculateWholeImage()
        {
            _logger.Info("RecalculateWholeImage");
            RecalculateChartImage();
            RecalculateTimeLineImage();
        }

        private async Task RecalculateChartImage()
        {
            try
            {
                _logger.Info("RecalculateChartImage");
                _calcDensityTokenSource?.Cancel();

                _calcDensityTokenSource = new CancellationTokenSource();
                var token = _calcDensityTokenSource.Token;
                var correlationId = Guid.NewGuid();
                _currentInfoCorrelationId = correlationId;

                var drawTask = DrawCurrentImage(token, correlationId);

                var getDataTask = GetDataFromApi(token);
                await Task.WhenAll(getDataTask, drawTask);

                _currentDensityInfo = getDataTask.Result;
                await DrawCurrentImage(token, correlationId);
            }
            catch (AggregateException e )
            {
                foreach (var innerException in e.InnerExceptions)
                {
                    if (innerException is OperationCanceledException)
                    {
                        _logger.Info("RecalculateChartImage: Task was cancelled");
                        continue;
                    }
                    _logger.Error($"RecalculateChartImage: {innerException.Message} - {innerException.StackTrace}");
                }
            }
            catch (OperationCanceledException)
            {
                _logger.Info("RecalculateChartImage: Task was cancelled");
            }
            catch (Exception ex)
            {
                _logger.Error($"RecalculateChartImage: {ex.Message} - {ex.StackTrace}");
            }
        }

        private async Task RecalculateTimeLineImage()
        {
            _logger.Info("RecalculateTimeLineImage");
            if (_currentWidth < 50)
            {
                _logger.Info("RecalculateTimeLineImage: Width less than 50 px. Don't draw anything");
                return;
            }

            var scaleInfo = _scaler.GetCurrentScaleInfo();
            var timeLineImage = await _timeLineRender.RenderDataAsync(_currentWidth, scaleInfo.CurrentStart, scaleInfo.CurrentWidth, CancellationToken.None);
            TimeLineImageSource = timeLineImage;
        }

        private async Task ScheduleTimeLineRedraw()
        {
            if (_swapTask != null)
                return;

            _swapTask = Task.Run(async () => { await Task.Delay(50); });
            await _swapTask;
            _swapTask = null;
            RecalculateTimeLineImage();
        }

        private async Task<List<DensityInfo>> GetDataFromApi(CancellationToken token)
        {
            var scaleInfo = _scaler.GetCurrentScaleInfo();
            var groupInterval = scaleInfo.CurrentWidth / IMAGE_WIDTH;
            return await _densityApi.GetDensityInfoAsync(scaleInfo.CurrentStart, scaleInfo.CurrentStop, groupInterval, token);
        }

        private async Task DrawCurrentImage(CancellationToken token, Guid correlationId)
        {
            var scaleInfo = _scaler.GetCurrentScaleInfo();

            var visibleDensities = _currentDensityInfo?.Where(den => den.Start <= scaleInfo.CurrentStop && den.Stop >= scaleInfo.CurrentStart).ToList();
            var chartImage = await _chartRender.RenderDataAsync(visibleDensities, scaleInfo.CurrentStart, scaleInfo.CurrentWidth, token);
            if (_currentInfoCorrelationId != correlationId)
                return;
            ChartImageSource = chartImage;
        }

        #region public bindings

        public bool CanMoveRight => _scaler.CanMoveRight;

        public bool CanMoveLeft => _scaler.CanMoveLeft;

        public ICommand MoveLeftCommand { get; }

        public ICommand MoveRightCommand { get; }

        public void ChangeScale(double centerRelativePos, bool decrease)
        {
            _logger.Info("ChangeScale");
            _scaler.Scale(centerRelativePos, decrease);
            OnPropertyChanged(nameof(CanMoveLeft));
            OnPropertyChanged(nameof(CanMoveRight));
            RecalculateWholeImage();
        }

        public void ChangeWidth(double newSizeWidth)
        {
            if (Math.Abs(newSizeWidth - _currentWidth) < 10)
                return;

            _currentWidth = newSizeWidth;
            ScheduleTimeLineRedraw();
        }

        public ImageSource ChartImageSource
        {
            get => _chartImageSource;
            set => SetProperty(ref _chartImageSource, value);
        }

        public ImageSource TimeLineImageSource
        {
            get => _timeLineImageSource;
            set => SetProperty(ref _timeLineImageSource, value);
        }

        #endregion public bindings

        public void Dispose()
        {
            _logger.Info("Dispose of EventDensityViewModel");
            _densityApi?.Dispose();
        }
    }
}
