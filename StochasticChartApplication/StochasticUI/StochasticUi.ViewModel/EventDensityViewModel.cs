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
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using StochasticUi.ViewModel.Renders;
using StochasticUi.ViewModel.Scale;

namespace StochasticUi.ViewModel
{
    public class EventDensityViewModel : BindableBase, IDisposable
    {
        private const int IMAGE_WIDTH = 400;
        private const int DEFAULT_TIMEOUT = 1000;
        ReaderWriterLock rwl = new ReaderWriterLock();
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
            FirstDrawChart();
        }

        private List<DensityInfo> GetVisibleInfos(ScaleInfo scaleInfo)
        {
            var infos = GetCloneOfCurrent();
            if (infos == null || !infos.Any())
                return new List<DensityInfo>(0);
            return infos.Where(den => den.Start <= scaleInfo.CurrentStop && den.Stop >= scaleInfo.CurrentStart).ToList();
        }
        private CancellationToken CancelPreviousTasksAndGetNewToken()
        {
            _calcDensityTokenSource?.Cancel();

            _calcDensityTokenSource = new CancellationTokenSource();
            return _calcDensityTokenSource.Token;
        }
        private void GetDataAndRedraw(Func<ScaleInfo, long, CancellationToken, List<DensityInfo>> getFunc)
        {
            var correlationId = Guid.NewGuid();
            _currentInfoCorrelationId = correlationId;
            var token = CancelPreviousTasksAndGetNewToken();

            DrawCurrentImage(token, correlationId);
            RecalculateTimeLineImage();

            Task.Run(() =>
            {
                var scaleInfo = _scaler.GetCurrentScaleInfo();
                var groupInterval = scaleInfo.CurrentWidth / IMAGE_WIDTH;
                WriteToResource(getFunc(scaleInfo, groupInterval, token));
            }, token).ContinueWith(task => DrawCurrentImage(token, correlationId), token);
        }

        private void MoveRight()
        {
            _scaler.MoveRight();
            OnPropertyChanged(nameof(CanMoveRight));
            OnPropertyChanged(nameof(CanMoveLeft));

            GetDataAndRedraw((scInfo, groupInterval, token) => _densityApi.MoveToRight(GetVisibleInfos(scInfo), scInfo.CurrentStart, scInfo.CurrentStop, groupInterval, token));
        }
        private void MoveLeft()
        {
            _scaler.MoveLeft();
            OnPropertyChanged(nameof(CanMoveRight));
            OnPropertyChanged(nameof(CanMoveLeft));

            GetDataAndRedraw((scInfo, groupInterval, token) => _densityApi.MoveToLeft(GetVisibleInfos(scInfo), scInfo.CurrentStart, scInfo.CurrentStop, groupInterval, token));
        }

        private void FirstDrawChart()
        {
            GetDataAndRedraw((scaleInfo, groupInterval, token) => _densityApi.GetDensityInfo(scaleInfo.CurrentStart, scaleInfo.CurrentStop, groupInterval, token));
        }

        #region timeline
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
        #endregion timeline

        #region  read|write sync

        List<DensityInfo> GetCloneOfCurrent()
        {
            try
            {
                rwl.AcquireReaderLock(DEFAULT_TIMEOUT);
                try
                {
                    return _currentDensityInfo?.ToList();
                }
                finally
                {
                    rwl.ReleaseReaderLock();
                }
            }
            catch (ApplicationException)
            {
                //todo log
                return null;
            }
        }

        void WriteToResource(List<DensityInfo> infos)
        {
            try
            {
                rwl.AcquireWriterLock(DEFAULT_TIMEOUT);
                try
                {
                    _currentDensityInfo = infos;
                }
                finally
                {
                    rwl.ReleaseWriterLock();
                }
            }
            catch (ApplicationException)
            {
               //todo log it
            }
        }

        #endregion
        #region drawing current data

        private void DrawCurrentImage(CancellationToken token, Guid correlationId)
        {
            PrepareImageFromCurrentData().ContinueWith(image => RenderImage(image.Result, correlationId), token);
        }
        private Task<ImageSource> PrepareImageFromCurrentData()
        {
            var infos = GetCloneOfCurrent();
            if (infos == null)
                return Task.FromResult((ImageSource)null);

            return Task.Run(() =>
            {
                var scaleInfo = _scaler.GetCurrentScaleInfo();
                var visibleDensities = infos.Where(den => den.Start <= scaleInfo.CurrentStop && den.Stop >= scaleInfo.CurrentStart).ToList();

                return ChartRender.RenderData(visibleDensities, scaleInfo.CurrentStart, scaleInfo.CurrentWidth);
            });
        }
        private void RenderImage(ImageSource source,Guid correlationToken)
        {
            if (correlationToken != _currentInfoCorrelationId)
                return;
            _uiDispatcher.BeginInvoke(new Action(() => { ChartImageSource = source; }));
        }

        #endregion drawing current data

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

            if (decrease)
                GetDataAndRedraw((scaleInfo, groupInterval, token) => _densityApi.ScaleInto(GetVisibleInfos(scaleInfo), scaleInfo.CurrentStart, scaleInfo.CurrentStop, groupInterval, token));
            else
                GetDataAndRedraw((scaleInfo, groupInterval, token) => _densityApi.ScaleOut(GetVisibleInfos(scaleInfo), scaleInfo.CurrentStart, scaleInfo.CurrentStop, groupInterval, token));
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
