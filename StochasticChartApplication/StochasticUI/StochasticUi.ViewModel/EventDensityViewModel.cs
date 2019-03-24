using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using EventApi.Models;
using EventsApi.Contracts;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using StochasticUi.ViewModel.Renders;
using StochasticUi.ViewModel.Scale;

namespace StochasticUi.ViewModel
{
    public class EventDensityViewModel : BindableBase, IDisposable
    {
        private const int IMAGE_WIDTH = 400;

        private readonly IScaler _scaler;
        private readonly IDensityApi _densityApi;
        private ImageSource _chartImageSource;
        private ImageSource _timeLineImageSource;

        private readonly Dispatcher _uiDispatcher;
        private double _currentWidth;
        private Task _swapTask;
        private CancellationTokenSource _calcDensityTokenSource;

        private List<DensityInfo> _currentDensityInfo;
        private Guid _currentInfoCorrelationId;
 
        public EventDensityViewModel(IScaler scaler, IDensityApi densityApi)
        {
            _scaler = scaler;
            _densityApi = densityApi;
            _uiDispatcher = Dispatcher.CurrentDispatcher;
            MoveLeftCommand = new DelegateCommand(MoveLeft);
            MoveRightCommand = new DelegateCommand(MoveRight);
            RecalculateWholeImage();
        }

        private void MoveRight()
        {
            _scaler.MoveRight();
            OnPropertyChanged(nameof(CanMoveRight));
            OnPropertyChanged(nameof(CanMoveLeft));
            RecalculateWholeImage();
        }

        private void MoveLeft()
        {
            _scaler.MoveLeft();
            OnPropertyChanged(nameof(CanMoveRight));
            OnPropertyChanged(nameof(CanMoveLeft));
            RecalculateWholeImage();
        }

        private void RecalculateWholeImage()
        {
            RecalculateChartImage();
            RecalculateTimeLineImage();
        }

        private void RecalculateChartImage()
        {
            _calcDensityTokenSource?.Cancel();

            _calcDensityTokenSource = new CancellationTokenSource();
            var token = _calcDensityTokenSource.Token;
            var correlationId = Guid.NewGuid();
            _currentInfoCorrelationId = correlationId;

            DrawCurrentImage(token, correlationId);

            GetDataFromApi(token).ContinueWith(data =>
            {
                if (_currentInfoCorrelationId != correlationId)
                    return;

                _currentDensityInfo = data.Result;
                DrawCurrentImage(token, correlationId);
            }, token);
        }

        private void RecalculateTimeLineImage()
        {
            if (_currentWidth < 50)
                return;

            Task.Run(() =>
            {
                var scaleInfo = _scaler.GetCurrentScaleInfo();
                return TimeLineRender.RenderData(_currentWidth, scaleInfo.CurrentStart, scaleInfo.CurrentWidth);
            }).ContinueWith(imageSource => _uiDispatcher.BeginInvoke(new Action(() => { TimeLineImageSource = imageSource.Result; })));
        }

        private void ScheduleTimeLineRedraw()
        {
            if (_swapTask != null)
                return;

            _swapTask = Task.Run(async () => { await Task.Delay(300); }).ContinueWith(t => _uiDispatcher.BeginInvoke(new Action(() =>
            {
                _swapTask = null;
                RecalculateTimeLineImage();
            })));
        }

        private Task<List<DensityInfo>> GetDataFromApi(CancellationToken token)
        {
            return Task.Run(() =>
            {
                var scaleInfo = _scaler.GetCurrentScaleInfo();
                var groupInterval = scaleInfo.CurrentWidth / IMAGE_WIDTH;
                return _densityApi.GetDensityInfo(scaleInfo.CurrentStart, scaleInfo.CurrentStop, groupInterval, token);

            }, token);
        }

        private void DrawCurrentImage(CancellationToken token, Guid correlationId)
        {
            PrepareImageFromCurrentData().ContinueWith(image => RenderImage(image.Result, correlationId), token);
        }

        private Task<ImageSource> PrepareImageFromCurrentData()
        {
            if (_currentDensityInfo == null)
                return Task.FromResult((ImageSource)null);

            return Task.Run(() =>
            {
                var scaleInfo = _scaler.GetCurrentScaleInfo();
                var visibleDensities = _currentDensityInfo.Where(den => den.Start <= scaleInfo.CurrentStop && den.Stop >= scaleInfo.CurrentStart).ToList();

                return ChartRender.RenderData(visibleDensities, scaleInfo.CurrentStart, scaleInfo.CurrentWidth);
            });
        }

        private void RenderImage(ImageSource source,Guid correlationToken)
        {
            if (correlationToken != _currentInfoCorrelationId)
                return;
            _uiDispatcher.BeginInvoke(new Action(() => { ChartImageSource = source; }));
        }

        #region public bindings

        public bool CanMoveRight => _scaler.CanMoveRight;

        public bool CanMoveLeft => _scaler.CanMoveLeft;

        public ICommand MoveLeftCommand { get; }

        public ICommand MoveRightCommand { get; }

        public void ChangeScale(double centerRelativePos, bool decrease)
        {
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
            _densityApi?.Dispose();
        }
    }
}
