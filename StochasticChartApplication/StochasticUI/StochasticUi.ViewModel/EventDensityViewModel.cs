using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media;
using EventsApi.Contracts;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using StochasticUi.ViewModel.Scale;
using Point = System.Windows.Point;

namespace StochasticUi.ViewModel
{
    public class EventDensityViewModel:BindableBase
    {
        private readonly IScaler _scaler;
        private readonly IDensityApi _densityApi;
        private ImageSource _imageSource;

        public EventDensityViewModel(IScaler scaler,IDensityApi densityApi)
        {
            _scaler = scaler;
            _densityApi = densityApi;
            MoveLeftCommand = new DelegateCommand(MoveLeft);
            MoveRightCommand = new DelegateCommand(MoveRight);
        }

        private void MoveRight()
        {
            _scaler.MoveRight();
            OnPropertyChanged(nameof(CanMoveRight));
            OnPropertyChanged(nameof(CanMoveLeft));
        }

        private void MoveLeft()
        {
            _scaler.MoveLeft();
            OnPropertyChanged(nameof(CanMoveRight));
            OnPropertyChanged(nameof(CanMoveLeft));
        }

        #region public bindings

        public bool CanMoveRight => _scaler.CanMoveRight;

        public bool CanMoveLeft => _scaler.CanMoveLeft;

        public ICommand MoveLeftCommand { get; }

        public ICommand MoveRightCommand { get; }

        public void ChangeScale(double centerRelativePos,bool decrease)
        {
            _scaler.Scale(centerRelativePos, decrease);
            OnPropertyChanged(nameof(CanMoveLeft));
        }

        public ImageSource ImageSource
        {
            get => _imageSource;
            set => SetProperty(ref _imageSource, value);
        }

        #endregion public bindings

        public void StartDraw()
        {
            //todo
        }
    }
}
