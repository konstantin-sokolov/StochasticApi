using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Point = System.Windows.Point;

namespace StochasticUi.ViewModel
{
    public class EventDensityViewModel:BindableBase
    {
        private readonly IScaleInfo _scaleInfo;
        private ImageSource _imageSource;

        public EventDensityViewModel(IScaleInfo scaleInfo)
        {
            _scaleInfo = scaleInfo;
            MoveLeftCommand = new DelegateCommand(MoveLeft);
            MoveRightCommand = new DelegateCommand(MoveRight);
        }

        private void MoveRight()
        {
            _scaleInfo.MoveRight();
        }

        private void MoveLeft()
        {
            _scaleInfo.MoveLeft();
        }

        #region public bindings

        public bool CanMoveRight => _scaleInfo.CanMoveRight;

        public bool CanMoveLeft => _scaleInfo.CanMoveLeft;

        public ICommand MoveLeftCommand { get; }

        public ICommand MoveRightCommand { get; }

        public void ChangeScale(int delta, Point center)
        {
            Trace.WriteLine("Test");
            _scaleInfo.Scale(delta, center);
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
