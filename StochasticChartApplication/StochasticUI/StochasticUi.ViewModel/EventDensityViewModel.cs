using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using EventsApi.Contracts;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using StochasticUi.ViewModel.Renders;
using StochasticUi.ViewModel.Scale;

namespace StochasticUi.ViewModel
{
    public class EventDensityViewModel : BindableBase
    {
        private const int IMAGE_WIDTH = 400;

        private readonly IScaler _scaler;
        private readonly IDensityApi _densityApi;
        private ImageSource _chartImageSource;
        private ImageSource _timeLineImageSource;

        private readonly Dispatcher _uiDispatcher;
        private double _currentWidth;

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
            Task.Run(() =>
            {
                var scaleInfo = _scaler.GetCurrentScaleInfo();
                var groupInterval = scaleInfo.CurrentWidth / IMAGE_WIDTH;
                var densities = _densityApi.GetDensityInfo(scaleInfo.CurrentStart, scaleInfo.CurrentStop, groupInterval);

                return ChartRender.RenderData(densities, scaleInfo.CurrentStart, scaleInfo.CurrentWidth);
            }).ContinueWith(imageSource => _uiDispatcher.BeginInvoke(new Action(() => { ChartImageSource = imageSource.Result; })));
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
            if (Math.Abs(newSizeWidth - _currentWidth)<10)
                return;

            _currentWidth = newSizeWidth;
            RecalculateTimeLineImage();
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

       
    }
}
