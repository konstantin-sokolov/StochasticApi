using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using EventsApi.Contracts;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using StochasticUi.ViewModel.Scale;

namespace StochasticUi.ViewModel
{
    public class EventDensityViewModel : BindableBase
    {
        private const int IMAGE_WIDTH = 2000;

        private readonly IScaler _scaler;
        private readonly IDensityApi _densityApi;
        private ImageSource _imageSource;
        private readonly Dispatcher _uiDispatcher;
        public EventDensityViewModel(IScaler scaler, IDensityApi densityApi)
        {
            _scaler = scaler;
            _densityApi = densityApi;
            _uiDispatcher = Dispatcher.CurrentDispatcher;
            MoveLeftCommand = new DelegateCommand(MoveLeft);
            MoveRightCommand = new DelegateCommand(MoveRight);
            RecalculateImage();
        }

        private void MoveRight()
        {
            _scaler.MoveRight();
            OnPropertyChanged(nameof(CanMoveRight));
            OnPropertyChanged(nameof(CanMoveLeft));
            RecalculateImage();
        }

        private void MoveLeft()
        {
            _scaler.MoveLeft();
            OnPropertyChanged(nameof(CanMoveRight));
            OnPropertyChanged(nameof(CanMoveLeft));
            RecalculateImage();
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
            RecalculateImage();
        }

        public ImageSource ImageSource
        {
            get => _imageSource;
            set => SetProperty(ref _imageSource, value);
        }

        #endregion public bindings

        private void RecalculateImage()
        {
            Task.Run(() =>
            {
                var scaleInfo = _scaler.GetCurrentScaleInfo();
                var groupInterval = scaleInfo.CurrentWidth / IMAGE_WIDTH;
                var densities = _densityApi.GetDensityInfo(scaleInfo.CurrentStart, scaleInfo.CurrentStop, groupInterval);
                var maxDensity = densities.Max(t => t.Count);
                return StatisticRender.RenderData(densities.Select(d => (double)d.Count/maxDensity).ToArray());
            }).ContinueWith(imageSource => _uiDispatcher.BeginInvoke(new Action(() => { ImageSource = imageSource.Result; })));
        }
    }
}
